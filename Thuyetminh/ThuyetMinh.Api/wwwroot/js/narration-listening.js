document.addEventListener("DOMContentLoaded", () => {
    const audio = document.getElementById("narration-audio");
    const startButton = document.getElementById("narration-start-audio");
    const status = document.getElementById("audio-status");
    const secondsEl = document.getElementById("remaining-time");
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    const shopId = Number(document.body.dataset.shopId || 0);
    const local = ["localhost", "127.0.0.1", "[::1]"].includes(location.hostname);
    const preview = local && new URLSearchParams(location.search).get("preview") === "1";
    const gpsRequired = document.body.dataset.requireGps === "true" && !local;
    if (!audio || !startButton || !status || !secondsEl) return;
    function say(key, fallback) {
        if (window.TMNarration) window.TMNarration.say(status, key);
        else status.textContent = fallback;
    }

    let audioId = 0, sessionId = null, generation = 0, state = "idle";
    let autoPending = false, latestFix = null, gpsWatch = null, retryAfter = 0, gpsRetryTimer = null;
    let queue = Promise.resolve(), timer = null, pending = false, leaving = false;
    let played = 0, anchor = null, serverBalance = 0, serverCharged = 0;
    const playedSeconds = () => Math.floor(played + 0.0001);
    function beginProgress() {
        if (!sessionId || state !== "playing" || audio.paused || audio.seeking) return;
        if (anchor) { sampleProgress(); return; }
        anchor = { media: audio.currentTime, clock: performance.now(), rate: audio.playbackRate || 1 };
    }
    function sampleProgress() {
        if (!anchor) return;
        const now = performance.now(), media = audio.currentTime;
        const delta = media - anchor.media;
        const rate = Math.max(0.1, anchor.rate);
        const elapsed = Math.max(0, (now - anchor.clock) / 1000);
        // A seek is a discontinuity, not listening time. Media time does not advance while buffering.
        if (!audio.seeking && Number.isFinite(delta) && delta > 0 && delta <= elapsed * rate + 0.5)
            played += delta / rate;
        anchor = { media, clock: now, rate: audio.playbackRate || 1 };
    }
    function stopProgress() { sampleProgress(); anchor = null; }
    function renderProgress() {
        const remaining = Math.max(0, serverBalance - Math.max(0, playedSeconds() - serverCharged));
        showBalance(remaining, false);
        return remaining;
    }
    function acceptSnapshot(r) {
        serverBalance = r.remainingSeconds;
        serverCharged = r.chargedSeconds;
        if (r.remainingSeconds <= 0) showBalance(0);
        else renderProgress();
    }
    function enqueue(work) {
        queue = queue.then(work).catch(e => { say('stopped',e.message); });
        return queue;
    }
    function stopTimer() { clearInterval(timer); timer = null; }
    function refreshButton() {
        startButton.hidden = preview || !audioId || state !== "idle" || leaving;
    }
    function showBalance(seconds, redirect = true) {
        if (!Number.isFinite(seconds)) return;
        const s = Math.max(0, seconds);
        secondsEl.textContent = Math.floor(s / 60) + ":" + String(s % 60).padStart(2, "0");

        const headerBalance = document.getElementById("header-remaining-time");
        if (headerBalance) {
            headerBalance.textContent = secondsEl.textContent;
        }
        if (redirect && !s && !preview && !leaving) {
            say('exhausted',"Đã hết thời lượng. Vui lòng mua thêm.");
            setTimeout(() => { if (!leaving) location.replace("/payment"); }, 900);
        }
    }
    function validFix(p) {
        if (!p || !Number.isFinite(p.timestamp) ||
            Date.now() - p.timestamp > 15000 ||
            p.timestamp > Date.now() + 10000) return false;

        const { latitude, longitude, accuracy } = p.coords;
        const lat = Number(document.body.dataset.shopLatitude);
        const lon = Number(document.body.dataset.shopLongitude);
        const radius = Number(document.body.dataset.shopRadius);

        if (![latitude, longitude, accuracy, lat, lon, radius].every(Number.isFinite) ||
            Math.abs(latitude) > 90 || Math.abs(longitude) > 180 ||
            Math.abs(lat) > 90 || Math.abs(lon) > 180 ||
            radius <= 0 || accuracy <= 0 ||
            accuracy > Math.max(100, radius)) return false;

        const rad = n => n * Math.PI / 180;

        const a = Math.sin(rad(latitude - lat) / 2) ** 2 +
            Math.cos(rad(lat)) * Math.cos(rad(latitude)) *
            Math.sin(rad(longitude - lon) / 2) ** 2;

        return 6371000 * 2 * Math.asin(Math.min(1, Math.sqrt(a))) <= radius;
    }

    function tryAutoStart() {
        if (!autoPending || leaving || document.hidden ||
            state !== "idle" || Date.now() < retryAfter) return;

        if (!gpsRequired || validFix(latestFix))
            start(gpsRequired ? latestFix : null);
        else
            say('gpsLooking', "Đang xác định vị trí...");
    }

    function watchGps() {
        if (!gpsRequired || preview || !shopId ||
            !navigator.geolocation || gpsWatch !== null) return;

        const accept = p => {
            if (leaving || (latestFix &&
                (p.timestamp < latestFix.timestamp ||
                (p.timestamp === latestFix.timestamp &&
                 p.coords.accuracy >= latestFix.coords.accuracy)))) return;

            latestFix = p;
            tryAutoStart();
        };

        navigator.geolocation.getCurrentPosition(
            accept, () => {},
            {
                enableHighAccuracy: false,
                maximumAge: 5000,
                timeout: 4000
            }
        );

        gpsWatch = navigator.geolocation.watchPosition(
            accept,
            e => {
                if (autoPending && state === "idle")
                    say(e.code === 1 ? 'gpsDenied' : 'gpsFailed',
                        "Không lấy được GPS.");
            },
            {
                enableHighAccuracy: true,
                maximumAge: 0,
                timeout: 20000
            }
        );
    }

    function stopGpsWatch() {
        if (gpsWatch !== null)
            navigator.geolocation.clearWatch(gpsWatch);

        clearTimeout(gpsRetryTimer);
        gpsRetryTimer = null;
        gpsWatch = null;
        latestFix = null;
    }

    async function gps(fix) {
        if (!gpsRequired) return {};
        if (!navigator.geolocation) throw new Error("Trình duyệt không hỗ trợ GPS.");
        const p = validFix(fix) ? fix : await new Promise((resolve, reject) => navigator.geolocation.getCurrentPosition(
            resolve, reject, { enableHighAccuracy: true, timeout: 12000, maximumAge: 5000 }));
        return { latitude: p.coords.latitude, longitude: p.coords.longitude,
            accuracyMeters: p.coords.accuracy, capturedAt: new Date(p.timestamp).toISOString() };
    }
    async function api(url, body = {}, keepalive = false) {
        const controller = new AbortController();
        const deadline = setTimeout(() => controller.abort(), 12000);
        let r;
        try {
            r = await fetch(url, { method: "POST", credentials: "same-origin", cache: "no-store", keepalive,
                signal: controller.signal,
                headers: { "Content-Type": "application/json", "RequestVerificationToken": token || "" },
                body: JSON.stringify(body) });
        } finally { clearTimeout(deadline); }
        const data = await r.json().catch(() => ({}));
        if (!r.ok) throw Object.assign(new Error(data.message || "HTTP " + r.status), { httpStatus: r.status });
        return data;
    }
    const update = (id, action, data = {}) => api("/api/listening/sessions/" + id + "/" + action,
        { ...data, playedSeconds: playedSeconds() }, action === "end");
    function stopMedia() {
        stopTimer();
        stopProgress();
        audio.pause();
        if (audio.hasAttribute("src")) { audio.removeAttribute("src"); audio.load(); }
    }
    async function finish() {
        const id = sessionId;
        stopProgress();
        sessionId = null;
        state = "stopping";
        stopMedia();
        try { if (id) showBalance((await update(id, "end")).remainingSeconds); }
        catch { if (id) showBalance(serverBalance, false); /* Unconfirmed playback is not a debit. */ }
        finally { state = "idle"; refreshButton(); }
    }
    function requestFinish(message, uiKey) {
        autoPending = false;
        generation++;
        state = "stopping";
        stopMedia();
        refreshButton();
        void enqueue(async () => {
            await finish();
            if (message) say(uiKey || 'stopped',message);
        });
    }
    function heartbeat() {
        stopTimer();
        timer = setInterval(() => {
            if (!sessionId || state !== "playing" || pending) return;
            const id = sessionId, version = generation;
            pending = true;
            void enqueue(async () => {
                try {
                    if (id !== sessionId || version !== generation || state !== "playing") return;
                    const position = await gps();
                    if (id !== sessionId || version !== generation || state !== "playing") return;
                    sampleProgress();
                    const r = await update(id, "heartbeat", position);
                    acceptSnapshot(r);
                    if (r.status === "Ended" || r.remainingSeconds <= 0) await finish();
                } catch (e) { await finish(); say('stopped',"Đã dừng nghe: " + (e.message || "GPS không hợp lệ.")); }
                finally { pending = false; }
            });
        }, 3000);
    }
    async function playMedia() {
        let timeout;
        anchor = { media: audio.currentTime, clock: performance.now(), rate: audio.playbackRate || 1 };
        try {
            await Promise.race([audio.play(), new Promise((_, reject) => {
                timeout = setTimeout(() => reject(new Error("Audio tải quá lâu.")), 12000);
            })]);
        } finally { clearTimeout(timeout); }
    }
    function start(fix = null) {
        if (preview || leaving || state !== "idle" || !shopId || !audioId || !token) return;

        const automatic = autoPending;
        autoPending = false;

        const version = generation, chosen = audioId;
        state = "starting";
        refreshButton();
        void enqueue(async () => {
            try {
                const position = await gps(fix);
                if (version !== generation || leaving) return;
                const r = await api("/api/listening/start", { shopId, audioId: chosen, ...position });
                sessionId = r.sessionId;
                played = 0; anchor = null;
                acceptSnapshot(r);
                if (version !== generation || leaving) { await finish(); return; }
                audio.src = "/api/listening/sessions/" + sessionId + "/audio";
                state = "playing";
                await playMedia();
                if (state === "playing" && version === generation && !leaving) {
                    try {
                        sessionStorage.setItem(
                            "tm_auto_gps_played_shop_v2",
                            String(shopId)
                        );
                    } catch {}
                    heartbeat(); say('playing',"Đang phát · thời lượng được tính bởi backend.");
                }
            } catch (e) {
                await finish();

                if (automatic && gpsRequired &&
                    version === generation && !leaving &&
                    (e.code === 2 || e.code === 3 ||
                    (e.httpStatus === 403 && /GPS/i.test(e.message)))) {

                    latestFix = null;
                    autoPending = true;
                    retryAfter = Date.now() + 3000;

                    clearTimeout(gpsRetryTimer);

                    gpsRetryTimer = setTimeout(() => {
                        if (!autoPending || leaving ||
                            version !== generation) return;

                        tryAutoStart();

                        if (autoPending) {
                            stopGpsWatch();
                            watchGps();
                        }
                    }, 3100);
                }

                say(e.name === "NotAllowedError" ? 'autoplayBlocked' : 'startFailed',
                    e.name === "NotAllowedError"
                        ? "Trình duyệt chặn tự phát. Nhấn Phát thuyết minh để nghe."
                        : "Không thể bắt đầu nghe: " + (e.message || "Kiểm tra GPS."));
            }
        });
    }
    function ready(id) {
        const next = Number(id);
        if (!Number.isInteger(next) || next <= 0) return;
        audioId = next;
        void enqueue(async () => {
            if (audioId === next && !leaving) {
                autoPending = true;
                retryAfter = 0;
                refreshButton();
                watchGps();
                tryAutoStart();
            }
        });
    }
    document.addEventListener("tm:audio-changing", () => { audioId = 0; requestFinish(); });
    document.addEventListener("tm:audio-ready", e => {
        if (Number(e.detail?.shopId) === shopId) ready(e.detail.audioId);
    });
    if (audio.dataset.audioId) ready(audio.dataset.audioId);
    startButton.addEventListener("click", () => { autoPending = false; start(latestFix); });
    audio.addEventListener("playing", beginProgress);
    audio.addEventListener("waiting", stopProgress);
    audio.addEventListener("seeking", () => { anchor = null; });
    audio.addEventListener("seeked", () => { if (audio.readyState >= 3) beginProgress(); });
    audio.addEventListener("ratechange", sampleProgress);
    audio.addEventListener("timeupdate", () => {
        if (!sessionId || state !== "playing") return;
        sampleProgress();
        if (renderProgress() === 0) requestFinish("Đã hết thời lượng nghe.",'exhausted');
    });
    audio.addEventListener("pause", () => {
        if (state !== "playing" || !sessionId || audio.ended) return;
        stopProgress();
        const id = sessionId;
        state = "paused"; stopTimer();
        void enqueue(async () => {
            if (id !== sessionId) return;
            try {
                const r = await update(id, "pause");
                acceptSnapshot(r);
                if (r.status === "Ended") await finish();
            } catch { await finish(); }
        });
    });
    audio.addEventListener("play", () => {
        if (state === "playing") return;
        if (state !== "paused" || !sessionId) { audio.pause(); return; }
        const id = sessionId, version = generation;
        state = "resuming"; audio.pause();
        void enqueue(async () => {
            try {
                if (id !== sessionId || version !== generation) return;
                const position = await gps();
                if (id !== sessionId || version !== generation || leaving) return;
                const r = await update(id, "resume", position);
                acceptSnapshot(r);
                if (r.status !== "Playing" || r.remainingSeconds <= 0 ||
                    version !== generation || leaving) { await finish(); return; }
                state = "playing";
                await playMedia();
                if (state === "playing" && version === generation) heartbeat();
            } catch (e) { await finish(); say('resumeFailed',"Không thể tiếp tục: " + e.message); }
        });
    });
    audio.addEventListener("ended", () => requestFinish("Đã phát xong.",'finished'));
    audio.addEventListener("error", () => {
        if (sessionId && state !== "stopping") requestFinish("Không tải được audio.",'audioError');
    });
    document.addEventListener("visibilitychange", () => {
        if (document.hidden && state === "playing") audio.pause();
        else if (!document.hidden) tryAutoStart();
    });
    window.addEventListener("offline", () => {
        if (sessionId || state === "starting") requestFinish("Mất kết nối. Đã dừng audio.",'offline');
    });
    window.addEventListener("pagehide", () => {
        stopGpsWatch();
        autoPending = false;
        leaving = true; generation++; state = "stopping"; stopMedia();
        const id = sessionId; sessionId = null;
        if (id) void update(id, "end").catch(() => {});
    });
    window.addEventListener("pageshow", e => {
        if (e.persisted) { leaving = false; state = "idle"; refreshButton(); }
    });
    fetch("/api/visitor/me", { credentials: "same-origin", cache: "no-store" })
        .then(r => r.json()).then(data => { if (!sessionId && state === "idle") showBalance(data.remainingSeconds); })
        .catch(() => { say('balanceError',"Không thể cập nhật thời lượng nghe."); });
});
