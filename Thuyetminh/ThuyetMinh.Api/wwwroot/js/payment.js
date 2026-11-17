// Thanh toan QR mo phong: giu ma don dang theo doi qua cac lan tai lai trang.
// Thanh toan duoc xac nhan o backend; JavaScript KHONG cong giay nghe.
document.addEventListener("DOMContentLoaded", () => {
    const createButton = document.getElementById("create-order-btn");
    const enterButton = document.getElementById("enter-app-btn");
    const csrfToken = document.querySelector('#demo-payment-form input[name="__RequestVerificationToken"]')?.value;
    const paymentStatus = document.getElementById("payment-status");
    const orderStatus = document.getElementById("order-status");
    const feedback = document.getElementById("payment-feedback");
    const orderCodeText = document.getElementById("payment-order-code");
    const issueQrButton = document.getElementById("demo-qr-btn");
    const ACTIVE_ORDER_KEY = "tm_demo_payment_active_order";
    let currentOrderCode = null;
    let orderPollingTimer = null;
    let polling = false;
    let completed = false;

    function say(el, key, fallback, ...args) {
        if (!el) return;
        if (window.TMUI) window.TMUI.bind(el, key, ...args);
        else el.textContent = fallback;
    }

    function savedOrder() {
        try { return localStorage.getItem(ACTIVE_ORDER_KEY); }
        catch { return null; }
    }
    function rememberOrder(code) {
        try { localStorage.setItem(ACTIVE_ORDER_KEY, code); }
        catch { /* Private browsing may block localStorage. Current tab still works. */ }
    }
    function forgetOrder() {
        try { localStorage.removeItem(ACTIVE_ORDER_KEY); }
        catch { /* ignore */ }
    }
    async function callApi(url, options = {}) {
        const response = await fetch(url, {
            credentials: "same-origin",
            cache: "no-store",
            ...options
        });
        const data = await response.json().catch(() => ({}));
        if (!response.ok) {
            const error = new Error(data.message || `HTTP ${response.status} - ${url}`);
            error.status = response.status;
            throw error;
        }
        return data;
    }
    function updateStatus(key, fallback) {
        say(paymentStatus, key, fallback);
        say(orderStatus, key, fallback);
    }
    function stopPolling() {
        if (orderPollingTimer !== null) clearInterval(orderPollingTimer);
        orderPollingTimer = null;
    }
    function startPolling() {
        stopPolling();
        if (!completed && currentOrderCode) {
            orderPollingTimer = setInterval(checkOrder, 1500);
            void checkOrder();
        }
    }
    async function checkOrder() {
        if (!currentOrderCode || polling || completed) return;
        const checkedCode = currentOrderCode;
        polling = true;
        try {
            const order = await callApi(`/api/payment/orders/${encodeURIComponent(checkedCode)}`);
            if (checkedCode !== currentOrderCode) return;
            if (order.status === "Paid") {
                completed = true;
                stopPolling();
                forgetOrder();
                updateStatus("paid", "Đã xác nhận QR (DEMO)");
                say(feedback, "paidFeedback", "Thanh toán đã xác nhận. Thiết bị này được cộng 900 giây và đang mở trang thuyết minh...");
                window.location.assign("/Narration");
                return;
            }
            if (order.status === "Failed" || order.status === "Expired") {
                stopPolling();
                forgetOrder();
                currentOrderCode = null;
                updateStatus(order.status === "Failed" ? "failed" : "expired", order.status === "Failed" ? "Thanh toán thất bại" : "Đơn hết hạn");
                return;
            }
            // Do not decide expiration using the browser clock: SQLite dates can
            // arrive without a timezone. The API returns the authoritative status
            // (including Expired), and Paid is always checked first.
        } catch (error) {
            if (error.status === 404 || error.status === 401) {
                stopPolling();
                forgetOrder();
                currentOrderCode = null;
                say(feedback, "orderRead", "Không thể đọc đơn thanh toán của trình duyệt này. Hãy tạo đơn/QR mới trong cùng cửa sổ laptop.");
            } else {
                say(feedback, "syncError", "Không thể đồng bộ trạng thái đơn: " + error.message);
                // Giữ bộ theo dõi để thử lại khi mạng phục hồi.
            }
        } finally {
            polling = false;
        }
    }
    function displayOrder(order, restored = false) {
        currentOrderCode = order.orderCode;
        completed = false;
        rememberOrder(currentOrderCode);
        updateStatus("waiting", "Đang chờ quét QR thanh toán");
        if (orderCodeText) {
            orderCodeText.hidden = false;
            say(orderCodeText, "orderCode", `Mã đơn hàng: ${currentOrderCode}`, currentOrderCode);
        }
        if (restored) {
            say(feedback, "restored", "Đã khôi phục mã đơn đang theo dõi. Hệ thống đang kiểm tra thanh toán...");
        }
        startPolling();
    }
    async function createOrder() {
        const order = await callApi("/api/payment/orders", { method: "POST" });
        displayOrder(order);
        // QR cũ (nếu có) không được sử dụng cho đơn mới.
        const canvas = document.getElementById("demo-qr-canvas");
        const placeholder = document.querySelector(".qr-placeholder");
        const help = document.getElementById("demo-qr-instruction");
        if (canvas) canvas.hidden = true;
        if (placeholder) placeholder.hidden = false;
        if (help) help.hidden = true;
        return order;
    }
    async function initializeVisitor() {
        if (createButton) createButton.disabled = true;
        try {
            const visitor = await callApi("/api/visitor/me");
            if (visitor.remainingSeconds > 0 && enterButton) {
                enterButton.hidden = false;
                say(feedback, "remaining", `Bạn còn ${visitor.remainingSeconds} giây nghe; có thể vào ứng dụng.`, visitor.remainingSeconds);
            } else {
                say(feedback, "empty", "Bạn chưa có thời lượng nghe. Hãy tạo QR thanh toán mô phỏng.");
            }
            const previousOrder = savedOrder();
            if (previousOrder) displayOrder({ orderCode: previousOrder }, true);
        } catch (error) {
            say(feedback, "apiError", "Không thể kết nối API: " + error.message);
        } finally {
            if (createButton) createButton.disabled = false;
        }
    }
    createButton?.addEventListener("click", async () => {
        createButton.disabled = true;
        try {
            await createOrder();
            if (issueQrButton) issueQrButton.click();
        } catch (error) {
            say(feedback, "createError", "Không thể tạo đơn: " + error.message);
        } finally { createButton.disabled = false; }
    });
    issueQrButton?.addEventListener("click", async () => {
        const canvas = document.getElementById("demo-qr-canvas");
        const help = document.getElementById("demo-qr-instruction");
        try {
            // QR target uses the configured demo ngrok domain.
            const publicHost = document.querySelector(".payment-container")?.dataset.demoPublicHost?.trim();
if (!publicHost) throw Object.assign(new Error("Chua cau hinh Demo:PublicHost cho may nay."), {uiKey: "publicHostMissing"});
const url = new URL(`https://${publicHost}`);
if (url.hostname.toLowerCase() !== publicHost.toLowerCase())
    throw Object.assign(new Error("Ten mien QR khong hop le."), {uiKey: "publicHostInvalid"});
            if (!csrfToken) throw Object.assign(new Error("Không tìm thấy mã xác thực biểu mẫu. Vui lòng tải lại trang."), {uiKey: "csrfMissing"});
            issueQrButton.disabled = true;
            if (!currentOrderCode) await createOrder();
            const ticket = await callApi("/api/demo/qr/issue", {
                method: "POST",
                headers: {
                    "RequestVerificationToken": csrfToken,
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ orderCode: currentOrderCode })
            });
            const paymentUrl = new URL("/demo/qr/pay", url.origin);
            paymentUrl.searchParams.set("code", ticket.code);
            paymentUrl.searchParams.set("lang", window.TMUI?.language() || "vi");
            if (typeof window.renderDemoQr !== "function") throw Object.assign(new Error("Thư viện QR chưa tải được."), {uiKey: "qrLibraryMissing"});
            window.renderDemoQr(canvas, paymentUrl.toString());
            const placeholder = document.querySelector(".qr-placeholder");
            if (placeholder) placeholder.hidden = true;
            if (canvas) canvas.hidden = false;
            if (help) {
                help.hidden = false;
                say(help, "scanInstruction", "Dùng điện thoại quét để xác nhận thanh toán mô phỏng. Thiết bị đang hiển thị QR sẽ nhận 900 giây.");
            }
            say(document.getElementById("qr-heading"), "demoQr", "QR THANH TOÁN (MÔ PHỎNG)");
            say(document.querySelector(".qr-note"), "creditNote", "900 giây được cộng cho thiết bị hiển thị QR; điện thoại quét chỉ xác nhận thanh toán.");
            say(feedback, "qrReady", "QR đã tạo. Chờ điện thoại xác nhận; thiết bị này sẽ tự mở Narration sau khi thanh toán thành công.");
            updateStatus("waiting", "Chờ quét QR thanh toán");
            void checkOrder();
        } catch (error) {
            say(feedback, error.uiKey || "qrError", "Không tạo được QR: " + error.message);
        } finally { issueQrButton.disabled = false; }
    });
    // Bộ duyệt có thể tạm hoãn timer khi tab bị ẩn; kiểm tra ngay khi quay về.
    window.addEventListener("focus", () => { void checkOrder(); });
    window.addEventListener("pageshow", () => {
        if (currentOrderCode && orderPollingTimer === null && !completed) startPolling();
        else void checkOrder();
    });
    document.addEventListener("visibilitychange", () => {
        if (document.visibilityState === "visible") void checkOrder();
    });
    window.addEventListener("pagehide", stopPolling);
    void initializeVisitor();
});
