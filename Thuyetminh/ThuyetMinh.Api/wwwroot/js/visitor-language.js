// Visitor UI preferences only. Never use these values for payment authorization.
(() => {
    const key = 'tm_visitor_ui_language';
    const codes = ['vi', 'en', 'zh', 'km'];
    const names = ['Tiếng Việt', 'English', '简体中文', 'ភាសាខ្មែរ'];
    const rows = {
        choose: ['Chọn ngôn ngữ', 'Choose your language', '选择语言', 'ជ្រើសរើសភាសា'],
        brand: ['Thuyết minh tự động đa ngôn ngữ', 'Multilingual Audio Guide', '多语言自动语音导览', 'មគ្គុទ្ទេសក៍សំឡេងស្វ័យប្រវត្តិពហុភាសា'],
        payment: ['Thanh toán gói nghe', 'Purchase listening time', '购买收听时长', 'ទិញរយៈពេលស្តាប់'],
        paymentSub: ['Thanh toán một lần để nhận 15 phút thuyết minh.', 'Pay once for 15 minutes of audio guidance.', '一次支付，获得15分钟语音导览。', 'បង់ប្រាក់ម្តងដើម្បីទទួលបានការណែនាំជាសំឡេងរយៈពេល 15 នាទី។'],
        back: ['← Vào ứng dụng', '← Open the guide', '← 进入导览', '← ចូលកម្មវិធី'],
        package: ['GÓI NGHE DUY NHẤT', 'LISTENING PACKAGE', '收听套餐', 'កញ្ចប់ស្តាប់'],
        duration: ['15 phút nghe · 900 giây', '15 minutes · 900 seconds', '15分钟 · 900秒', '15 នាទី · 900 វិនាទី'],
        minutes: ['15 phút', '15 minutes', '15分钟', '15 នាទី'],
        featureLanguage: ['Thuyết minh đa ngôn ngữ', 'Multilingual audio guidance', '多语言语音导览', 'ការណែនាំជាសំឡេងពហុភាសា'],
        featureGps: ['Tự động phát theo vị trí GPS', 'Automatic playback based on GPS', '根据GPS位置自动播放', 'ចាក់ដោយស្វ័យប្រវត្តិតាមទីតាំង GPS'],
        featurePause: ['Tạm dừng và tiếp tục nghe', 'Pause and resume listening', '暂停和继续收听', 'ផ្អាក និងបន្តស្តាប់'],
        note: ['Thời lượng được cộng sau khi thanh toán được xác nhận và chỉ trừ khi phiên nghe hợp lệ đang phát.', 'Time is added after payment is confirmed and deducted only during valid audio playback.', '付款确认后增加时长，仅在有效播放时扣除。', 'រយៈពេលត្រូវបានបន្ថែមបន្ទាប់ពីការបង់ប្រាក់ត្រូវបានបញ្ជាក់ ហើយកាត់តែនៅពេលកំពុងចាក់សំឡេងដែលមានសុពលភាព។'],
        qr: ['THANH TOÁN BẰNG QR', 'QR PAYMENT', '二维码支付', 'បង់ប្រាក់តាម QR'],
        demoQr: ['QR THANH TOÁN (MÔ PHỎNG)', 'DEMO PAYMENT QR', '模拟支付二维码', 'QR បង់ប្រាក់សាកល្បង'],
        notCreated: ['Chưa tạo đơn', 'No order yet', '尚未创建订单', 'មិនទាន់បង្កើតការបញ្ជាទិញ'],
        placeholder: ['Mã QR thanh toán', 'Payment QR code', '支付二维码', 'កូដ QR បង់ប្រាក់'],
        noBank: ['Chưa kết nối ngân hàng', 'No bank connection', '未连接银行', 'មិនភ្ជាប់ធនាគារ'],
        noQr: ['Chưa có mã QR thanh toán', 'No payment QR yet', '尚无支付二维码', 'មិនទាន់មាន QR បង់ប្រាក់'],
        amount: ['Số tiền thanh toán', 'Payment amount', '支付金额', 'ចំនួនទឹកប្រាក់'],
        received: ['Thời lượng nhận được', 'Listening time received', '获得的收听时长', 'រយៈពេលស្តាប់ដែលទទួលបាន'],
        orderStatus: ['Trạng thái đơn', 'Order status', '订单状态', 'ស្ថានភាពការបញ្ជាទិញ'],
        create: ['Tạo đơn thanh toán', 'Create payment order', '创建支付订单', 'បង្កើតការបញ្ជាទិញ'],
        enter: ['Vào ứng dụng thuyết minh', 'Open the audio guide', '进入语音导览', 'ចូលមគ្គុទ្ទេសក៍សំឡេង'],
        issue: ['Tạo QR thanh toán (DEMO)', 'Create demo payment QR', '生成模拟支付二维码', 'បង្កើត QR បង់ប្រាក់សាកល្បង'],
        simulation: ['Chế độ mô phỏng: chưa liên kết ngân hàng, không phát sinh giao dịch tiền thật. Quét QR để mô phỏng giao dịch.', 'Demo only: no bank connection or real money transfer. Scan the QR to simulate payment.', '仅供演示：未连接银行，不产生真实资金交易。扫描二维码模拟付款。', 'សម្រាប់សាកល្បងប៉ុណ្ណោះ៖ មិនភ្ជាប់ធនាគារ និងមិនផ្ទេរប្រាក់ពិតទេ។ ស្កេន QR ដើម្បីក្លែងធ្វើការបង់ប្រាក់។'],
        footer: ['Thuyết minh tự động đa ngôn ngữ · Trải nghiệm trên máy tính và điện thoại', 'Multilingual Audio Guide · Available on desktop and mobile', '多语言自动语音导览 · 支持电脑和手机', 'មគ្គុទ្ទេសក៍សំឡេងពហុភាសា · ប្រើលើកុំព្យូទ័រ និងទូរស័ព្ទ'],
        paid: ['Đã xác nhận QR (DEMO)', 'Demo payment confirmed', '模拟付款已确认', 'បានបញ្ជាក់ការបង់ប្រាក់សាកល្បង'],
        paidFeedback: ['Thanh toán đã xác nhận. Thiết bị này được cộng 900 giây và đang mở trang thuyết minh...', 'Payment confirmed. 900 seconds added to this device. Opening the guide...', '付款已确认。本设备已增加900秒，正在打开导览…', 'ការបង់ប្រាក់ត្រូវបានបញ្ជាក់។ បានបន្ថែម 900 វិនាទីទៅឧបករណ៍នេះ។ កំពុងបើកមគ្គុទ្ទេសក៍...'],
        failed: ['Thanh toán thất bại', 'Payment failed', '付款失败', 'ការបង់ប្រាក់បរាជ័យ'],
        expired: ['Đơn hết hạn', 'Order expired', '订单已过期', 'ការបញ្ជាទិញផុតកំណត់'],
        orderRead: ['Không thể đọc đơn thanh toán của trình duyệt này. Hãy tạo đơn/QR mới trong cùng trình duyệt.', 'Unable to read this browser’s order. Create a new order or QR in the same browser.', '无法读取此浏览器的订单，请在同一浏览器中创建新订单或二维码。', 'មិនអាចអានការបញ្ជាទិញក្នុងកម្មវិធីរុករកនេះ។ សូមបង្កើតការបញ្ជាទិញ ឬ QR ថ្មីក្នុងកម្មវិធីដដែល។'],
        syncError: ['Không thể đồng bộ trạng thái đơn. Vui lòng thử lại.', 'Unable to sync order status. Please try again.', '无法同步订单状态，请重试。', 'មិនអាចធ្វើសមកាលកម្មស្ថានភាពបានទេ។ សូមព្យាយាមម្តងទៀត។'],
        waiting: ['Đang chờ quét QR thanh toán', 'Waiting for QR scan', '等待扫描二维码', 'កំពុងរង់ចាំការស្កេន QR'],
        orderCode: ['Mã đơn hàng: {0}', 'Order code: {0}', '订单编号：{0}', 'លេខកូដការបញ្ជាទិញ៖ {0}'],
        restored: ['Đã khôi phục mã đơn đang theo dõi. Hệ thống đang kiểm tra thanh toán...', 'Order restored. Checking payment...', '已恢复订单，正在检查付款…', 'បានស្ដារការបញ្ជាទិញ។ កំពុងពិនិត្យការបង់ប្រាក់...'],
        remaining: ['Bạn còn {0} giây nghe; có thể vào ứng dụng.', 'You have {0} seconds left. You can open the guide.', '您还有{0}秒可用，可以进入导览。', 'អ្នកនៅសល់ {0} វិនាទី។ អាចចូលមគ្គុទ្ទេសក៍បាន។'],
        empty: ['Bạn chưa có thời lượng nghe. Hãy tạo QR thanh toán mô phỏng.', 'You have no listening time. Create a demo payment QR.', '您没有剩余收听时长，请生成模拟支付二维码。', 'អ្នកមិនទាន់មានរយៈពេលស្តាប់ទេ។ សូមបង្កើត QR បង់ប្រាក់សាកល្បង។'],
        apiError: ['Không thể kết nối hệ thống. Vui lòng thử lại.', 'Unable to connect. Please try again.', '无法连接系统，请重试。', 'មិនអាចភ្ជាប់ប្រព័ន្ធបានទេ។ សូមព្យាយាមម្តងទៀត។'],
        createError: ['Không thể tạo đơn. Vui lòng thử lại.', 'Unable to create order. Please try again.', '无法创建订单，请重试。', 'មិនអាចបង្កើតការបញ្ជាទិញបានទេ។ សូមព្យាយាមម្តងទៀត។'],
        scanInstruction: ['Dùng điện thoại quét để xác nhận thanh toán mô phỏng. Thiết bị đang hiển thị QR sẽ nhận 900 giây.', 'Scan with a phone to confirm demo payment. The device displaying the QR receives 900 seconds.', '使用手机扫描以确认模拟付款。显示二维码的设备将获得900秒。', 'ស្កេនដោយទូរស័ព្ទដើម្បីបញ្ជាក់ការបង់ប្រាក់សាកល្បង។ ឧបករណ៍ដែលបង្ហាញ QR ទទួលបាន 900 វិនាទី។'],
        creditNote: ['900 giây được cộng cho thiết bị hiển thị QR; điện thoại quét chỉ xác nhận thanh toán.', '900 seconds are added to the device displaying the QR; the scanning phone only confirms payment.', '900秒将增加到显示二维码的设备；扫码手机仅用于确认付款。', '900 វិនាទីត្រូវបន្ថែមទៅឧបករណ៍បង្ហាញ QR។ ទូរស័ព្ទស្កេនគ្រាន់តែបញ្ជាក់ការបង់ប្រាក់។'],
        qrReady: ['QR đã tạo. Chờ điện thoại xác nhận; thiết bị này sẽ tự mở Narration sau khi thanh toán thành công.', 'QR created. Waiting for phone confirmation; this device will open the guide after payment.', '二维码已生成，等待手机确认；支付成功后本设备将自动打开导览。', 'បានបង្កើត QR។ កំពុងរង់ចាំការបញ្ជាក់ពីទូរស័ព្ទ។ ឧបករណ៍នេះនឹងបើកមគ្គុទ្ទេសក៍បន្ទាប់ពីបង់ប្រាក់ជោគជ័យ។'],
        qrError: ['Không tạo được QR. Vui lòng thử lại.', 'Unable to create QR. Please try again.', '无法生成二维码，请重试。', 'មិនអាចបង្កើត QR បានទេ។ សូមព្យាយាមម្តងទៀត។'],
        publicHostMissing: ['Chưa cấu hình địa chỉ QR mô phỏng.', 'Demo QR address is not configured.', '未配置模拟二维码地址。', 'មិនទាន់កំណត់អាសយដ្ឋាន QR សាកល្បង។'],
        publicHostInvalid: ['Tên miền QR không hợp lệ.', 'Invalid QR domain.', '二维码域名无效。', 'ដែន QR មិនត្រឹមត្រូវ។'],
        csrfMissing: ['Không tìm thấy mã xác thực. Vui lòng tải lại trang.', 'Verification token missing. Please reload the page.', '缺少验证令牌，请刷新页面。', 'បាត់លេខកូដផ្ទៀងផ្ទាត់។ សូមផ្ទុកទំព័រឡើងវិញ។'],
        qrLibraryMissing: ['Thư viện QR chưa tải được.', 'QR library could not load.', '二维码库加载失败。', 'មិនអាចផ្ទុកបណ្ណាល័យ QR បានទេ។']
    };
    let lang = 'vi', chosen = false;
    try {
        const saved = localStorage.getItem(key);
        if (codes.includes(saved)) { lang = saved; chosen = sessionStorage.getItem("tm_language_chosen_session") === "1"; }
    } catch { /* preference unavailable, use this page only */ }
    const bindings = new Map();
    const text = (id, ...args) => {
        const row = rows[id];
        return (row?.[codes.indexOf(lang)] ?? row?.[0] ?? id)
            .replace(/\{(\d+)\}/g, (_, n) => String(args[Number(n)] ?? ''));
    };
    function bind(el, id, ...args) {
        if (!el) return;
        bindings.set(el, { id, args });
        el.textContent = text(id, ...args);
    }
    const attrs = [];
    function bindAttr(el, attr, id) {
        if (!el) return;
        attrs.push({el, attr, id});
        el.setAttribute(attr, text(id));
    }
    function apply() {
        document.documentElement.lang = lang;
        for (const [el, item] of bindings) el.textContent = text(item.id, ...item.args);
        for (const {el, attr, id} of attrs) el.setAttribute(attr, text(id));
        document.dispatchEvent(new CustomEvent('tm:ui-language-changed', {detail: {language: lang}}));
    }
    let select, dialog;
    function setLanguage(code) {
        if (!codes.includes(code)) return;
        lang = code;
        chosen = true;
        try { localStorage.setItem(key, code); sessionStorage.setItem("tm_language_chosen_session", "1"); } catch { /* this page still works */ }
        if (select) select.value = code;
        if (dialog?.open) dialog.close();
        apply();
    }
    function mountPicker(host) {
        if (!host || select) return;
        select = document.createElement('select');
        select.className = 'tm-language-select';
        codes.forEach((code, i) => {
            const option = new Option(names[i], code);
            option.lang = code; select.add(option);
        });
        select.value = lang;
        bindAttr(select, 'aria-label', 'choose');
        select.addEventListener('change', () => setLanguage(select.value));
        host.append(select);
        if (chosen) return;
        dialog = document.createElement('dialog');
        dialog.className = 'tm-language-dialog';
        dialog.setAttribute('aria-labelledby', 'tm-language-title');
        const title = document.createElement('h2');
        title.id = 'tm-language-title'; title.textContent = 'Choose your language / Chọn ngôn ngữ';
        dialog.append(title);
        codes.forEach((code, i) => {
            const button = document.createElement('button');
            button.type = 'button'; button.lang = code; button.textContent = names[i];
            button.addEventListener('click', () => setLanguage(code));
            dialog.append(button);
        });
        dialog.addEventListener('cancel', e => e.preventDefault());
        document.body.append(dialog);
        if (typeof dialog.showModal === 'function') dialog.showModal();
        else { dialog.remove(); setLanguage(lang); }
    }
    function initPayment() {
        const q = s => document.querySelector(s);
        const staticLabels = {
            '.site-brand-name': 'brand', '.site-header-label': 'payment',
            '.payment-heading h1': 'payment', '.payment-heading p': 'paymentSub',
            '.back-link': 'back', '#package-heading': 'package', '.duration': 'duration',
            '.feature-list li:nth-child(1)': 'featureLanguage',
            '.feature-list li:nth-child(2)': 'featureGps',
            '.feature-list li:nth-child(3)': 'featurePause',
            '.note-box span:nth-child(2)': 'note', '#qr-heading': 'qr',
            '#payment-status': 'notCreated', '#order-status': 'notCreated',
            '.qr-placeholder strong': 'placeholder', '.qr-placeholder small': 'noBank',
            '.info-row:nth-child(1) > span': 'amount',
            '.info-row:nth-child(2) > span': 'received', '.info-row:nth-child(2) > strong': 'minutes',
            '.info-row:nth-child(3) > span': 'orderStatus', '#create-order-btn': 'create',
            '#enter-app-btn': 'enter', '#demo-qr-btn': 'issue', '.qr-note': 'simulation',
            '.payment-footer': 'footer'
        };
        for (const [selector, id] of Object.entries(staticLabels)) bind(q(selector), id);
        bindAttr(q('.site-brand'), 'aria-label', 'brand');
        bindAttr(q('#demo-qr-canvas'), 'aria-label', 'demoQr');
        bindAttr(q('.qr-placeholder'), 'aria-label', 'noQr');
        document.addEventListener('tm:ui-language-changed', () => { document.title = text('payment') + ' | ' + text('brand'); });
        mountPicker(q('.site-header-inner'));
        apply();
    }
    window.TMUI = { language: () => lang, text, bind, bindAttr, mountPicker, setLanguage, apply,
        register: more => Object.assign(rows, more), unbind: el => bindings.delete(el),
        hasChosen: () => chosen, whenChosen: () => chosen ? Promise.resolve() : new Promise(resolve =>
            document.addEventListener('tm:ui-language-changed', resolve, {once: true})) };
    if (document.querySelector('.payment-container')) initPayment();
})();
