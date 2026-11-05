document.addEventListener("DOMContentLoaded", () => {
    const secondsEl = document.getElementById("remaining-time");
    const statusEl = document.getElementById("audio-status");
    const playBtn = document.getElementById("narration-demo-play");
    const pauseBtn = document.getElementById("narration-demo-pause");
    const nativeAudio = document.querySelector(".audio-controls audio");
    const antiForgery = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    const shopId = Number(document.body.dataset.shopId || 0);
    const spokenText = document.querySelector(".tour-description p")?.textContent?.trim() || "Chào mừng bạn đến với địa điểm tham quan.";
    const synth = window.speechSynthesis;
    let sessionId = null, playing = false, paused = false, updating = false, mode = null;
    let authorizingAudio = false;
    let timer = null, utterance = null;
    const format = s => `${Math.floor(Math.max(0, s) / 60)}:${String(Math.max(0, s) % 60).padStart(2, "0")}`;
    const isLocalPreview =
    (location.hostname === "localhost" ||
     location.hostname === "127.0.0.1") &&
    new URLSearchParams(location.search).get("preview") === "1";
    function showBalance(seconds) {
        secondsEl.textContent = format(seconds);
        if (seconds <= 0 && !isLocalPreview) {
            stopLocal();
            statusEl.textContent = "Đã hết thời lượng. Vui lòng mua thêm để tiếp tục.";
            setTimeout(() => window.location.replace("/payment"), 900);
        }
    }
    async function api(url, body = undefined) {
        const r = await fetch(url, {
            method: "POST", credentials: "same-origin", cache: "no-store",
            headers: { "Content-Type": "application/json", "RequestVerificationToken": antiForgery || "" },
            body: JSON.stringify(body || {})
        });
        const data = await r.json().catch(() => ({}));
        if (!r.ok) throw new Error(data.message || `HTTP ${r.status}`);
        return data;
    }
    const stopTimer = () => { if (timer) clearInterval(timer); timer = null; };
    function stopLocal() {
        playing = false; paused = false; stopTimer();
        if (mode === "speech" && synth) synth.cancel();
        if (mode === "audio" && nativeAudio && !nativeAudio.paused) nativeAudio.pause();
        mode = null; utterance = null;
        if (playBtn) playBtn.textContent = "Nghe giới thiệu (giọng trình duyệt)";
        if (pauseBtn) pauseBtn.hidden = true;
    }
    async function change(action) {
        if (!sessionId || updating) return;
        updating = true;
        try {
            const data = await api(`/api/listening/sessions/${sessionId}/${action}`);
            showBalance(data.remainingSeconds);
            if (data.status === "Ended") {
                stopLocal(); sessionId = null;
            }
            return data;
        } catch (e) {
            statusEl.textContent = "Mất kết nối tính thời lượng: " + e.message;
            stopLocal();
        } finally { updating = false; }
    }
    async function begin(which) {
        if (!shopId || !antiForgery) {
            statusEl.textContent = "Chưa có địa điểm được cấu hình để bắt đầu phiên nghe.";
            return false;
        }
        try {
            const result = await api("/api/listening/start", { shopId });
            sessionId = result.sessionId; playing = true; paused = false; mode = which;
            showBalance(result.remainingSeconds);
            statusEl.textContent = "Đang phát · thời lượng được tính bởi backend.";
            stopTimer();
            timer = setInterval(() => {
                if (sessionId && playing && !paused) void change("heartbeat");
            }, 3000);
            return true;
        } catch (e) {
            statusEl.textContent = "Không thể bắt đầu nghe: " + e.message;
            if (nativeAudio && which === "audio") nativeAudio.pause();
            return false;
        }
    }
    async function finish() {
        stopTimer();
        if (sessionId) await change("end");
        sessionId = null;
        stopLocal();
    }
    if (nativeAudio) {
        nativeAudio.addEventListener("play", async () => {
            if (mode === "audio" && paused) { paused = false; await change("resume"); return; }
            if (mode === "audio" && playing) return;
            if (!nativeAudio.currentSrc) return;
            // Pause pending browser playback until the server authorizes this session.
            authorizingAudio = true;
            nativeAudio.pause();
            try {
                if (mode === "speech") await finish();
                const ok = await begin("audio");
                if (ok) {
                    await nativeAudio.play().catch(() => { void finish(); });
                }
            } finally {
                authorizingAudio = false;
            }
        });
        nativeAudio.addEventListener("pause", () => {
            if (authorizingAudio) return;
            if (mode === "audio" && sessionId && !nativeAudio.ended) {
                paused = true; void change("pause");
            }
        });
        nativeAudio.addEventListener("ended", () => { if (mode === "audio") void finish(); });
    }
    if (!synth || !window.SpeechSynthesisUtterance) {
        if (playBtn) playBtn.disabled = true;
        statusEl.textContent = "Trình duyệt không hỗ trợ giọng đọc; cần file MP3 từ module Nhong.";
    }
    if (playBtn) playBtn.addEventListener("click", async () => {
        if (mode === "speech" && paused) {
            const result = await change("resume");
            if (!result || result.status === "Ended") return;
            synth.resume(); paused = false;
            playBtn.textContent = "Đang nghe...";
            pauseBtn.hidden = false;
            return;
        }
        if (mode === "speech" && playing) return;
        if (sessionId) await finish();
        const ok = await begin("speech");
        if (!ok) return;
        const u = new SpeechSynthesisUtterance(spokenText);
        u.lang = "vi-VN";
        u.rate = 0.85;
        utterance = u;
        u.onend = () => { if (utterance === u) void finish(); };
        u.onerror = () => { if (utterance === u) void finish(); };
        synth.cancel(); synth.speak(u);
        playBtn.textContent = "Đang nghe...";
        pauseBtn.hidden = false;
    });
    if (pauseBtn) pauseBtn.addEventListener("click", async () => {
        if (mode !== "speech" || !sessionId || paused) return;
        synth.pause(); paused = true;
        await change("pause");
        playBtn.textContent = "Tiếp tục nghe";
        pauseBtn.hidden = true;
    });
    document.addEventListener("visibilitychange", () => {
        if (document.hidden && mode === "speech" && sessionId && !paused) pauseBtn?.click();
        if (document.hidden && mode === "audio" && nativeAudio && !nativeAudio.paused) nativeAudio.pause();
    });
    window.addEventListener("pagehide", () => { stopTimer(); if (synth) synth.cancel(); });
    fetch("/api/visitor/me", { credentials: "same-origin", cache: "no-store" })
        .then(r => r.json()).then(x => showBalance(x.remainingSeconds))
        .catch(() => { statusEl.textContent = "Không thể cập nhật thời lượng nghe."; });
});
