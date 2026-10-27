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
    function updateStatus(text) {
        if (paymentStatus) paymentStatus.textContent = text;
        if (orderStatus) orderStatus.textContent = text;
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
                updateStatus("Đã xác nhận QR (DEMO)");
                feedback.textContent = "Thanh toán đã xác nhận. Thiết bị này được cộng 900 giây và đang mở trang thuyết minh...";
                window.location.assign("/Narration");
                return;
            }
            if (order.status === "Failed" || order.status === "Expired") {
                stopPolling();
                forgetOrder();
                currentOrderCode = null;
                updateStatus(order.status === "Failed" ? "Thanh toán thất bại" : "Đơn hết hạn");
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
                feedback.textContent = "Không thể đọc đơn thanh toán của trình duyệt này. Hãy tạo đơn/QR mới trong cùng cửa sổ laptop.";
            } else {
                feedback.textContent = "Không thể đồng bộ trạng thái đơn: " + error.message;
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
        updateStatus("Đang chờ quét QR thanh toán");
        if (orderCodeText) {
            orderCodeText.hidden = false;
            orderCodeText.textContent = `Mã đơn hàng: ${currentOrderCode}`;
        }
        if (restored) {
            feedback.textContent = "Đã khôi phục mã đơn đang theo dõi. Hệ thống đang kiểm tra thanh toán...";
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
                feedback.textContent = `Bạn còn ${visitor.remainingSeconds} giây nghe; có thể vào ứng dụng.`;
            } else {
                feedback.textContent = "Bạn chưa có thời lượng nghe. Hãy tạo QR thanh toán mô phỏng.";
            }
            const previousOrder = savedOrder();
            if (previousOrder) displayOrder({ orderCode: previousOrder }, true);
        } catch (error) {
            feedback.textContent = "Không thể kết nối API: " + error.message;
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
            feedback.textContent = "Không thể tạo đơn: " + error.message;
        } finally { createButton.disabled = false; }
    });
    issueQrButton?.addEventListener("click", async () => {
        const canvas = document.getElementById("demo-qr-canvas");
        const help = document.getElementById("demo-qr-instruction");
        try {
            // QR target uses the configured demo ngrok domain.
            const url = new URL("https://glory-exact-employer.ngrok-free.dev");
            if (!csrfToken) throw new Error("Không tìm thấy mã xác thực biểu mẫu. Vui lòng tải lại trang.");
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
            if (typeof window.renderDemoQr !== "function") throw new Error("Thư viện QR chưa tải được.");
            window.renderDemoQr(canvas, paymentUrl.toString());
            const placeholder = document.querySelector(".qr-placeholder");
            if (placeholder) placeholder.hidden = true;
            if (canvas) canvas.hidden = false;
            if (help) {
                help.hidden = false;
                help.textContent = "Dùng điện thoại quét để xác nhận thanh toán mô phỏng. Thiết bị đang hiển thị QR sẽ nhận 900 giây.";
            }
            document.getElementById("qr-heading").textContent = "QR THANH TOÁN (MÔ PHỎNG)";
            document.querySelector(".qr-note").textContent = "900 giây được cộng cho thiết bị hiển thị QR; điện thoại quét chỉ xác nhận thanh toán.";
            feedback.textContent = "QR đã tạo. Chờ điện thoại xác nhận; thiết bị này sẽ tự mở Narration sau khi thanh toán thành công.";
            updateStatus("Chờ quét QR thanh toán");
            void checkOrder();
        } catch (error) {
            feedback.textContent = "Không tạo được QR: " + error.message;
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
