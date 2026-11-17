// UI-only translations. Approved descriptions/audio remain owned by the existing APIs.
(() => {
    const ui = window.TMUI;
    if (!ui) return;
    const rows = {
        discover: ['Khám phá','Explore','探索','រុករក'],
        places: ['Địa điểm','Destinations','目的地','គោលដៅ'],
        tagline: ['KHÁM PHÁ QUA ÂM THANH','EXPLORE THROUGH SOUND','通过声音探索','រុករកតាមសំឡេង'],
        heroEyebrow: ['KHÁM PHÁ SÀI GÒN THEO CÁCH RIÊNG','DISCOVER SAIGON YOUR WAY','用自己的方式探索西贡','រុករកសៃហ្គនតាមរបៀបរបស់អ្នក'],
        heroFirst: ['Mỗi địa điểm.','Every place.','每个地点。','គ្រប់ទីកន្លែង។'],
        heroSecond: ['Một câu chuyện.','A story.','一个故事。','រឿងរ៉ាវមួយ។'],
        heroDescription: ['Lắng nghe những câu chuyện thú vị đằng sau mỗi điểm đến. Khám phá ẩm thực và văn hóa địa phương bằng ngôn ngữ bạn yêu thích.','Listen to the stories behind each destination. Explore local food and culture in your preferred language.','聆听每个目的地背后的故事，以您喜欢的语言探索当地美食和文化。','ស្តាប់រឿងរ៉ាវនៃគោលដៅនីមួយៗ។ ស្វែងយល់ពីម្ហូប និងវប្បធម៌ក្នុងស្រុកជាភាសាដែលអ្នកចូលចិត្ត។'],
        browse: ['Khám phá địa điểm','Explore destinations','探索目的地','រុករកគោលដៅ'],
        placeCount: ['Địa điểm giới thiệu','Destinations','个目的地','គោលដៅ'],
        languageCount: ['Ngôn ngữ dự kiến','Languages','种语言','ភាសា'],
        local: ['TRẢI NGHIỆM ĐỊA PHƯƠNG','LOCAL EXPERIENCE','当地体验','បទពិសោធន៍ក្នុងស្រុក'],
        localSub: ['KHÁM PHÁ • LẮNG NGHE • CẢM NHẬN','EXPLORE • LISTEN • EXPERIENCE','探索 • 聆听 • 体验','រុករក • ស្តាប់ • ទទួលបទពិសោធន៍'],
        suggested: ['ĐỊA ĐIỂM ĐỀ XUẤT','SUGGESTED DESTINATIONS','推荐目的地','គោលដៅណែនាំ'],
        destinationsTitle: ['Điểm đến đang chờ bạn','Destinations await you','目的地等您探索','គោលដៅកំពុងរង់ចាំអ្នក'],
        destinationsSub: ['Chọn một địa điểm để tìm hiểu và nghe thuyết minh.','Choose a destination to learn more and listen to its guide.','选择目的地以了解更多并聆听导览。','ជ្រើសរើសគោលដៅដើម្បីស្វែងយល់ និងស្តាប់ការណែនាំ។'],
        search: ['Tìm theo tên địa điểm...','Search destinations...','搜索目的地…','ស្វែងរកគោលដៅ...'],
        noResults: ['Không tìm thấy địa điểm phù hợp.','No matching destinations.','未找到匹配的目的地。','រកមិនឃើញគោលដៅដែលត្រូវគ្នា។'],
        allPlaces: ['← Tất cả địa điểm','← All destinations','← 所有目的地','← គោលដៅទាំងអស់'],
        details: ['KHÁM PHÁ / CHI TIẾT ĐỊA ĐIỂM','EXPLORE / DESTINATION DETAILS','探索 / 目的地详情','រុករក / ព័ត៌មានគោលដៅ'],
        journey: ['HÀNH TRÌNH ẨM THỰC','CULINARY JOURNEY','美食之旅','ដំណើររសជាតិម្ហូប'],
        cuisine: ['🍽 Ẩm thực Việt Nam','🍽 Vietnamese cuisine','🍽 越南美食','🍽 ម្ហូបវៀតណាម'],
        multi: ['◎ Thuyết minh đa ngôn ngữ','◎ Multilingual audio guide','◎ 多语言导览','◎ មគ្គុទ្ទេសក៍សំឡេងពហុភាសា'],
        exploreTag: ['⌖ Khám phá địa điểm','⌖ Explore destinations','⌖ 探索目的地','⌖ រុករកគោលដៅ'],
        image: ['HÌNH ẢNH MINH HỌA','ILLUSTRATIVE IMAGE','示意图片','រូបភាពបង្ហាញ'],
        storyStep: ['01 / KHÁM PHÁ','01 / EXPLORE','01 / 探索','01 / រុករក'],
        story: ['Câu chuyện địa điểm','The story of this place','地点故事','រឿងរ៉ាវនៃទីកន្លែង'],
        quote: ['Mỗi điểm đến là một câu chuyện về văn hóa và ẩm thực địa phương.','Every destination tells a story of local culture and cuisine.','每个目的地都讲述当地文化和美食的故事。','គោលដៅនីមួយៗប្រាប់រឿងរ៉ាវអំពីវប្បធម៌ និងម្ហូបក្នុងស្រុក។'],
        audioLanguage: ['CHỌN NGÔN NGỮ THUYẾT MINH','AUDIO LANGUAGE','导览语言','ភាសាការណែនាំសំឡេង'],
        approvedNote: ['Nội dung và âm thanh phụ thuộc dữ liệu đã được duyệt.','Text and audio depend on approved content.','文字和音频取决于已审核内容。','អត្ថបទ និងសំឡេងអាស្រ័យលើខ្លឹមសារដែលបានអនុម័ត។'],
        transcript: ['▤ Nội dung thuyết minh','▤ Narration transcript','▤ 导览文字','▤ អត្ថបទការណែនាំ'],
        chosenLanguage: ['Theo ngôn ngữ đã chọn','In the selected language','按所选语言显示','តាមភាសាដែលបានជ្រើសរើស'],
        transcriptWaiting: ['Nội dung thuyết minh sẽ được hiển thị khi có bản dịch đã duyệt.','The transcript appears when an approved translation is available.','有已审核译文时显示导览文字。','អត្ថបទបង្ហាញនៅពេលមានការបកប្រែដែលបានអនុម័ត។'],
        mapStep: ['02 / VỊ TRÍ','02 / LOCATION','02 / 位置','02 / ទីតាំង'],
        mapTitle: ['⌖ Vị trí trên bản đồ','⌖ Location on the map','⌖ 地图位置','⌖ ទីតាំងលើផែនទី'],
        gpsUnknown: ['Chưa xác định vị trí','Location not determined','尚未定位','មិនទាន់កំណត់ទីតាំង'],
        enableGps: ['⌖ Bật định vị','⌖ Enable location','⌖ 启用定位','⌖ បើកទីតាំង'],
        mapNote: ['Vị trí chỉ mang tính tham khảo; quyền phát thuyết minh vẫn do máy chủ kiểm tra.','Location is for reference; the server verifies playback permission.','位置仅供参考，播放权限仍由服务器验证。','ទីតាំងសម្រាប់យោងប៉ុណ្ណោះ។ ម៉ាស៊ីនមេពិនិត្យសិទ្ធិចាក់សំឡេង។'],
        nowExploring: ['ĐANG KHÁM PHÁ','NOW EXPLORING','正在探索','កំពុងរុករក'],
        player: ['Trình phát thuyết minh','Audio guide player','语音导览播放器','កម្មវិធីចាក់សំឡេងណែនាំ'],
        play: ['Phát thuyết minh','Play audio guide','播放导览','ចាក់សំឡេងណែនាំ'],
        noAudio: ['Chưa có audio','Audio unavailable','暂无音频','មិនទាន់មានសំឡេង'],
        time: ['THỜI GIAN CÒN LẠI','TIME REMAINING','剩余时长','រយៈពេលនៅសល់'],
        cardLabel: ['ĐỊA ĐIỂM • TP.HCM','DESTINATION • HCMC','目的地 • 胡志明市','គោលដៅ • ហូជីមិញ'],
        cardLink: ['Xem chi tiết →','View details →','查看详情 →','មើលព័ត៌មាន →'],
        loading: ['Đang tải...','Loading...','加载中…','កំពុងផ្ទុក...'],
        checkingTranslation: ['Đang kiểm tra bản dịch...','Checking translation...','正在检查译文…','កំពុងពិនិត្យការបកប្រែ...'],
        loadingAudio: ['Đang tải audio...','Loading audio...','正在加载音频…','កំពុងផ្ទុកសំឡេង...'],
        ready: ['Sẵn sàng phát','Ready to play','可以播放','ត្រៀមចាក់សំឡេង'],
        finished: ['Đã phát xong','Playback finished','播放完成','ចាក់សំឡេងចប់'],
        audioError: ['Không tải được audio','Unable to load audio','无法加载音频','មិនអាចផ្ទុកសំឡេងបាន'],
        noTranslation: ['Quán chưa có bản dịch cho ngôn ngữ này.','This place has no translation for this language yet.','此地点暂无该语言的译文。','ទីកន្លែងនេះមិនទាន់មានការបកប្រែជាភាសានេះទេ។'],
        emptyTranslation: ['Có bản dịch nhưng chưa có nội dung văn bản để hiển thị.','The translation has no text to display.','该译文暂无可显示文字。','ការបកប្រែមិនទាន់មានអត្ថបទបង្ហាញ។'],
        translationError: ['Không tải được bản dịch hoặc thông tin audio.','Unable to load translation or audio information.','无法加载译文或音频信息。','មិនអាចផ្ទុកការបកប្រែ ឬព័ត៌មានសំឡេងបាន។'],
        noApproved: ['Địa điểm chưa có nội dung được duyệt.','This destination has no approved content yet.','此目的地暂无已审核内容。','គោលដៅនេះមិនទាន់មានខ្លឹមសារដែលបានអនុម័តទេ។'],
        approved: ['Nội dung đã được duyệt.','Content approved.','内容已审核。','ខ្លឹមសារត្រូវបានអនុម័ត។'],
        languageMissing: ['Ngôn ngữ này chưa khả dụng. Vui lòng chọn ngôn ngữ thuyết minh khác.','This language is unavailable. Please choose another audio language.','该语言暂不可用，请选择其他导览语言。','ភាសានេះមិនទាន់អាចប្រើបានទេ។ សូមជ្រើសភាសាសំឡេងផ្សេង។'],
        gpsUnsupported: ['Trình duyệt không hỗ trợ định vị','Location is not supported','浏览器不支持定位','កម្មវិធីរុករកមិនគាំទ្រទីតាំង'],
        gpsLooking: ['Đang xác định vị trí...','Determining location...','正在定位…','កំពុងកំណត់ទីតាំង...'],
        gpsDenied: ['Bạn chưa cấp quyền định vị','Location permission denied','未授权定位','មិនបានអនុញ្ញាតទីតាំង'],
        gpsFailed: ['Không lấy được vị trí','Unable to determine location','无法获取位置','មិនអាចកំណត់ទីតាំងបាន'],
        distance: ['Cách khoảng {0} {1}','About {0} {1} away','距离约 {0} {1}','ចម្ងាយប្រហែល {0} {1}'],
        exhausted: ['Đã hết thời lượng. Vui lòng mua thêm.','Listening time exhausted. Please purchase more.','收听时长已用完，请购买更多。','រយៈពេលស្តាប់អស់ហើយ។ សូមទិញបន្ថែម។'],
        playing: ['Đang phát · thời lượng được tính bởi backend.','Playing · listening time is tracked by the server.','正在播放 · 服务器计算收听时长。','កំពុងចាក់ · ម៉ាស៊ីនមេតាមដានរយៈពេលស្តាប់។'],
        autoplayBlocked: ['Trình duyệt chặn tự phát. Nhấn Phát thuyết minh để nghe.','Autoplay blocked. Press Play audio guide to listen.','浏览器阻止自动播放，请点击播放导览。','កម្មវិធីរុករករារាំងការចាក់ស្វ័យប្រវត្តិ។ ចុចចាក់សំឡេងណែនាំ។'],
        startFailed: ['Không thể bắt đầu nghe. Kiểm tra GPS và kết nối.','Unable to start. Check GPS and connection.','无法开始播放，请检查定位与网络。','មិនអាចចាប់ផ្ដើមបាន។ សូមពិនិត្យ GPS និងការតភ្ជាប់។'],
        stopped: ['Đã dừng nghe. Kiểm tra GPS và kết nối.','Playback stopped. Check GPS and connection.','播放已停止，请检查定位与网络。','បានបញ្ឈប់ការស្តាប់។ សូមពិនិត្យ GPS និងការតភ្ជាប់។'],
        resumeFailed: ['Không thể tiếp tục nghe.','Unable to resume playback.','无法继续播放。','មិនអាចបន្តចាក់បាន។'],
        offline: ['Mất kết nối. Đã dừng audio.','Connection lost. Audio stopped.','网络已断开，播放已停止。','បាត់ការតភ្ជាប់។ បានបញ្ឈប់សំឡេង។'],
        balanceError: ['Không thể cập nhật thời lượng nghe.','Unable to update listening time.','无法更新剩余时长。','មិនអាចធ្វើបច្ចុប្បន្នភាពរយៈពេលស្តាប់បាន។']
    };
    ui.register(rows);
    const q = sel => document.querySelector(sel);
    const label = (sel, key, index) => {
        const el = q(sel);
        if (index !== undefined) return ui.bind(el && [...el.childNodes].filter(n => n.nodeType === 3 && n.textContent.trim())[index], key);
        ui.bind(el, key);
    };
    const selectors = {
        '.tour-nav a:nth-child(1)':'discover','.tour-nav a:nth-child(2)':'places','.tour-logo small':'tagline',
        '.hero-content h1 em':'heroSecond','.hero-content p':'heroDescription',
        '.hero-photo-caption small':'localSub','.section-heading .eyebrow':'suggested','.section-heading h2':'destinationsTitle','.section-heading p':'destinationsSub',
        '#search-empty':'noResults','#back-to-list':'allPlaces','.detail-breadcrumb':'details','.detail-heading > .eyebrow':'journey',
        '.detail-tags span:nth-child(1)':'cuisine','.detail-tags span:nth-child(2)':'multi','.detail-tags span:nth-child(3)':'exploreTag',
        '.image-label':'image','.info-number':'storyStep','.detail-info h2':'story','#story-quote':'quote','.language-box label':'audioLanguage',
        '.language-note':'approvedNote','.narration-card-head strong':'transcript','#translation-label':'chosenLanguage','#narration-text-status':'transcriptWaiting',
        '.map-copy .eyebrow':'mapStep','.map-copy h2':'mapTitle','#gps-result':'gpsUnknown','#enable-location':'enableGps','.map-disclaimer':'mapNote',
        '.dock-kicker':'nowExploring','.dock-title > span:last-child':'player','#narration-start-audio':'play','#audio-status':'noAudio','.remaining-time small':'time','#header-balance-label':'time'
    };
    for (const [sel,key] of Object.entries(selectors)) label(sel,key);
    label('.tour-logo > span:last-child','brand',0);
    label('.hero-content > .eyebrow','heroEyebrow',0);
    label('.hero-content h1','heroFirst',0);
    label('.primary-action','browse',0);
    label('.hero-facts > span:first-child','placeCount',0);
    label('.hero-facts > span:last-child','languageCount',0);
    label('.hero-photo-caption > span:last-child','local',0);
    ui.bindAttr(q('#shop-search'),'placeholder','search');
    ui.bindAttr(q('#shop-search'),'aria-label','search');
    ui.bindAttr(q('#destinations'),'aria-label','places');
    ui.bindAttr(q('#shop-map'),'title','mapTitle');
    ui.bindAttr(q('#language'),'aria-label','audioLanguage');
    ui.bind(q('#language option'),'loading');
    document.addEventListener('tm:ui-language-changed', () => { document.title = ui.text('brand'); });
    ui.mountPicker(q('.header-inner'));
    ui.apply();
    function formatAddress(value, code = ui.language()) {
        const original = String(value ?? '');

        if (!['en','zh','km'].includes(code)) return original;

        const fold = v => v.normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .replace(/đ/g,'d').replace(/Đ/g,'D');

        const name = v => code === 'en' ? fold(v) : v;

        const city = {
            en:'Ho Chi Minh City',
            zh:'胡志明市',
            km:'ទីក្រុងហូជីមិញ'
        };

        const rules = [
            [/^(?:Phường\s+|P\.\s*)(.+)$/i, 'Ward','坊','សង្កាត់'],
            [/^(?:Quận\s+|Q\.\s*)(.+)$/i, 'District','郡','ខណ្ឌ'],
            [/^(?:Huyện\s+|H\.\s*)(.+)$/i, 'District','县','ស្រុក'],
            [/^(?:Xã\s+|X\.\s*)(.+)$/i, 'Commune','社','ឃុំ'],
            [/^Thị trấn\s+(.+)$/i, 'Town','市镇','ទីប្រជុំជន'],
            [/^Thị xã\s+(.+)$/i, 'Town','市社','ក្រុង'],
            [/^(?:Thành phố\s+|TP\.\s*)(.+)$/i, 'City','市','ទីក្រុង'],
            [/^Tỉnh\s+(.+)$/i, 'Province','省','ខេត្ត']
        ];

        return original.split(',').map(part => {
            const s = part.trim();
            const key = fold(s).toLowerCase().replace(/[\s.]/g,'');

            if ([
                'tphcm','tphochiminh','thanhphohochiminh',
                'hochiminh','hochiminhcity','hcmc'
            ].includes(key)) return city[code];

            if (key === 'vietnam') {
                return {
                    en:'Vietnam',
                    zh:'越南',
                    km:'វៀតណាម'
                }[code];
            }

            const street = s.match(
                /^(?:(\d+[\dA-Za-z/-]*)\s+)?(?:Đường\s+|Đ\.\s*)(.+)$/i
            );

            if (street) {
                const prefix = street[1] ? street[1] + ' ' : '';
                const v = name(street[2]);

                return prefix + ({
                    en:v+' Street',
                    zh:v+'路',
                    km:'ផ្លូវ '+v
                }[code]);
            }

            for (const [pattern,en,zh,km] of rules) {
                const match = s.match(pattern);

                if (!match) continue;

                const v = name(match[1]);

                if (code === 'en') {
                    return /^\d+$/.test(v)
                        ? en + ' ' + v
                        : v + ' ' + en;
                }

                if (code === 'zh') {
                    return (/^\d+$/.test(v) ? '第' : '') + v + zh;
                }

                return km + ' ' + v;
            }

            return name(s);
        }).join(', ');
    }

    function refreshAddresses() {
        document.querySelectorAll('[data-original-address]').forEach(el => {
            el.textContent = formatAddress(el.dataset.originalAddress);
            el.title = el.dataset.originalAddress;
        });
    }

    document.addEventListener(
        'tm:ui-language-changed',
        refreshAddresses
    );

    const descriptions = [
        [
            'Trường Đại học Sài Gòn - một trong những cơ sở giáo dục lâu đời và uy tín nhất khu vực phía Nam.',
            'Saigon University is one of the oldest and most respected educational institutions in southern Vietnam.',
            '西贡大学是越南南部历史最悠久、最负盛名的教育机构之一。',
            'សាកលវិទ្យាល័យសៃហ្គនជាគ្រឹះស្ថានអប់រំមួយក្នុងចំណោមគ្រឹះស្ថានដែលមានប្រវត្តិយូរអង្វែង និងមានកេរ្តិ៍ឈ្មោះល្បីល្បាញនៅភាគខាងត្បូងប្រទេសវៀតណាម។'
        ],
        [
            'Quán ăn gia đình với các món Việt truyền thống, không gian ấm cúng.',
            'A family-style restaurant serving traditional Vietnamese dishes in a warm, cozy setting.',
            '一家供应传统越南菜肴、环境温馨的家庭式餐厅。',
            'ភោជនីយដ្ឋានបែបគ្រួសារដែលមានម្ហូបវៀតណាមប្រពៃណី និងបរិយាកាសកក់ក្តៅ។'
        ],
        [
            'Hương vị bún chả chuẩn Hà Nội giữa lòng Sài Gòn, nướng than hoa.',
            'Authentic Hanoi-style bun cha, grilled over charcoal in the heart of Saigon.',
            '在西贡市中心品尝地道的河内烤肉米粉，采用炭火烤制。',
            'រសជាតិប៊ុនចាបែបហាណូយដើមនៅកណ្តាលទីក្រុងសៃហ្គន ដោយអាំងលើធ្យូង។'
        ],
        [
            'Phở bò nấu theo công thức gia truyền hơn 30 năm, nước dùng thanh ngọt.',
            'Beef pho made from a family recipe passed down for over 30 years, with a light, naturally sweet broth.',
            '采用传承逾30年的家传配方制作的牛肉河粉，汤底清甜。',
            'គុយទាវហ្វឺសាច់គោតាមរូបមន្តគ្រួសារជាង 30 ឆ្នាំ ជាមួយទឹកស៊ុបមានរសជាតិផ្អែមស្រាល។'
        ],
        [
            'Quán ốc đêm với đa dạng các món ốc, ghẹ và tôm hấp bia.',
            'A late-night seafood spot with a variety of snails, swimming crabs and beer-steamed shrimp.',
            '一家夜间营业的海鲜餐厅，供应多种螺类、梭子蟹和啤酒蒸虾。',
            'ហាងអាហារសមុទ្រពេលយប់ ដែលមានម្ហូបខ្យង ក្តាមសមុទ្រ និងបង្គាចំហុយជាមួយស្រាបៀរ។'
        ],
        [
            'Cơm tấm sườn nướng mật ong, ăn kèm chả trứng và nước mắm chua ngọt.',
            'Broken rice with honey-grilled pork ribs, egg meatloaf and sweet-and-sour fish sauce.',
            '碎米饭搭配蜂蜜烤猪排、蒸肉蛋糕和酸甜鱼露。',
            'បាយអង្ករបាក់ជាមួយឆ្អឹងជំនីជ្រូកអាំងទឹកឃ្មុំ សាច់ចំហុយជាមួយស៊ុត និងទឹកត្រីជូរផ្អែម។'
        ],
        [
            'Địa điểm đã được duyệt trong hệ thống.',
            'This destination has been approved in the system.',
            '该地点已通过系统审核。',
            'គោលដៅនេះត្រូវបានអនុម័តក្នុងប្រព័ន្ធរួចហើយ។'
        ]
    ];

    const descriptionKeys = new Map(
        descriptions.map((row,i) => [row[0], 'placeDescription'+i])
    );

    ui.register(
        Object.fromEntries(
            descriptions.map((row,i) => ['placeDescription'+i,row])
        )
    );

    const publishedDescriptionBindings = new Map();

    function refreshPublishedDescriptions() {
        for (const [el, texts] of publishedDescriptionBindings) {
            el.textContent =
                texts[ui.language()] || ui.text('noTranslation');
        }
    }

    document.addEventListener(
        'tm:ui-language-changed',
        refreshPublishedDescriptions
    );

    function setDescription(el,value,publishedTexts) {
        if (!el) return;

        if (publishedTexts && typeof publishedTexts === 'object') {
            ui.unbind(el);
            publishedDescriptionBindings.set(el,publishedTexts);

            el.textContent =
                publishedTexts[ui.language()] ||
                ui.text('noTranslation');

            return;
        }

        publishedDescriptionBindings.delete(el);

        const original = String(value ?? '');
        const key = descriptionKeys.get(original);

        if (key) {
            ui.bind(el,key);
        } else {
            ui.unbind(el);
            el.textContent = original;
        }
    }

    window.TMNarration = {
        setDescription,
        formatAddress,
        setAddress: (el,value) => {
            if (!el) return;
            el.dataset.originalAddress = String(value ?? '');
            el.title = el.dataset.originalAddress;
            el.textContent = formatAddress(value);
        },
        say: (el,key,...args) => ui.bind(el,key,...args),
        raw: (el,value) => { ui.unbind(el); el.textContent = value; },
        localizeCards: () => {
            document.querySelectorAll('.shop-card .section-label').forEach(e => ui.bind(e,'cardLabel'));
            document.querySelectorAll('.shop-card .card-arrow').forEach(e => ui.bind(e,'cardLink'));
            refreshAddresses();
            document.querySelectorAll('[data-original-description]').forEach(el => setDescription(el,el.dataset.originalDescription));
        },
        languageName: code => ({vi:'Tiếng Việt',en:'English',zh:'简体中文',km:'ភាសាខ្មែរ'})[code] || code
    };
})();

// TM-DESCRIPTION-TOGGLE
(() => {
    const p = document.getElementById('shop-description');
    if (!p) return;

    const labels = {
        vi: ['Xem thêm', 'Thu gọn'],
        en: ['Read more', 'Show less'],
        zh: ['查看更多', '收起'],
        km: ['មើលបន្ថែម', 'បង្រួម']
    };

    const button = document.createElement('button');

    button.type = 'button';
    button.className = 'tm-story-toggle';
    button.setAttribute('aria-controls', p.id);
    button.hidden = true;

    p.after(button);

    let expanded = false;

    function update() {
        const lang = window.TMUI?.language() || 'vi';
        const words = labels[lang] || labels.vi;

        p.classList.toggle('tm-story-collapsed', !expanded);

        button.textContent = words[expanded ? 1 : 0];
        button.setAttribute('aria-expanded', String(expanded));

        button.hidden = !expanded &&
            p.scrollHeight <= p.clientHeight + 2;
    }

    button.addEventListener('click', () => {
        expanded = !expanded;
        update();
    });

    const observer = new MutationObserver(() => {
        expanded = false;
        requestAnimationFrame(update);
    });

    observer.observe(p, {
        childList: true,
        characterData: true,
        subtree: true
    });

    document.addEventListener(
        'tm:ui-language-changed',
        () => requestAnimationFrame(update)
    );

    if (typeof ResizeObserver !== 'undefined') {
        new ResizeObserver(update).observe(p);
    }

    update();
})();
