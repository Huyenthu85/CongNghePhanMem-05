# Phân công và quy ước triển khai backend

Đề tài: Thuyết minh tự động đa ngôn ngữ.

Định hướng: phát triển backend theo kiến trúc ba lớp, kết hợp CI/CD. Giao diện sử dụng biểu mẫu và trình phát đơn giản để thao tác, kiểm thử và trình diễn chức năng.

## Phần Chúp

- Xây dựng đăng nhập và phân quyền cho chủ quán, quản trị viên.
- Quản lý thông tin quán/địa điểm: tên, địa chỉ, tọa độ, bán kính kích hoạt và nội dung thuyết minh gốc.
- Xây dựng luồng chủ quán gửi nội dung, quản trị viên duyệt hoặc từ chối.
- Kiểm tra chủ quán chỉ được thao tác dữ liệu thuộc quyền quản lý của mình.
- Quản lý bản gửi chỉnh sửa và phối hợp xác định phiên bản nội dung được xuất bản.
- Làm giao diện chủ quán, quản trị viên và mẫu giao diện dùng chung.
- Phụ trách dữ liệu tài khoản, phân quyền, địa điểm và bản gửi kiểm duyệt. Tên bảng cụ thể được chốt khi tích hợp.
- Viết phần báo cáo tiếng Anh tương ứng; tổng hợp và chỉnh sửa báo cáo chung.

## Phần Nhong

- Quản lý ngôn ngữ, bản dịch và nội dung thuyết minh theo ngôn ngữ.
- Tích hợp chuyển văn bản thành giọng nói (Text-to-Speech — TTS).
- Quản lý tác vụ tạo âm thanh, trạng thái xử lý và đường dẫn tệp.
- Liên kết bản dịch và âm thanh với đúng phiên bản nội dung nguồn.
- Xử lý GPS trên trình duyệt và xác định địa điểm trong bán kính kích hoạt.
- Làm giao diện chọn ngôn ngữ và trình phát.
- Tích hợp trình phát với API phiên nghe của Thư.
- Chuyển dữ liệu lưu tạm trong bộ nhớ sang database theo phương án chung.
- Thiết lập CI: tự động khôi phục thư viện, biên dịch và chạy kiểm thử.
- Viết phần báo cáo tiếng Anh về đa ngôn ngữ, âm thanh, GPS và CI.

## Phần Thư

- Duy trì bộ khung backend ba lớp và tích hợp mã nguồn.
- Xây dựng nhận diện người nghe không đăng nhập bằng cookie.
- Quản lý gói nghe, đơn thanh toán và mã QR.
- Xác minh thanh toán trước khi cộng thời lượng; ngăn cộng trùng giao dịch.
- Quản lý số giây còn lại, phiên nghe và lịch sử cộng/trừ thời lượng.
- Xử lý yêu cầu gửi lặp và ngăn nhiều phiên nghe đồng thời trên cùng hồ sơ truy cập.
- Làm giao diện mua gói, thanh toán và xem thời lượng.
- Thống nhất dữ liệu, hợp đồng API và liên kết chức năng của Chúp, Nhong.
- Phối hợp bảo đảm chỉ nội dung đủ điều kiện xuất bản mới được cung cấp cho người nghe.
- Thiết lập CD: triển khai bản đã vượt qua kiểm tra lên môi trường chạy thử.
- Viết phần báo cáo tiếng Anh về kiến trúc, thanh toán, thời lượng nghe, tích hợp và CD.

## Công nghệ và phương án đang đánh giá

### Các điểm đã thống nhất

- Công cụ lập trình: VS Code.
- Nền tảng hiện tại: .NET 10.
- Backend: C# và ASP.NET Core.
- Kiến trúc: ba lớp trong cùng một solution.
- Máy chủ database đã xác nhận trên máy Thư: MariaDB 10.4.32.
- MySQL Workbench là công cụ quản lý database, không phải máy chủ lưu dữ liệu.

### Hiện trạng các phần

| Thành viên | Hiện trạng đã kiểm tra |
|---|---|
| Chúp | Lớp Data dùng EF Core, có thư viện SQLite và `AppDbContext` |
| Nhong | `TranslationRepository` lưu bản dịch trong danh sách ở bộ nhớ |
| Thư | Đã thử kết nối MariaDB bằng MySqlConnector và soạn script cấu trúc ban đầu |

Chưa xác nhận các phần này hoạt động chung trên một database.

### Phương án đề xuất

- Thống nhất EF Core để tận dụng phần Chúp đã viết.
- Giữ MariaDB làm database chung.
- Kiểm tra thư viện kết nối EF Core–MariaDB tương thích trước khi chọn phiên bản.
- Nếu chốt EF Core, thống nhất dùng migration để quản lý thay đổi cấu trúc database.
- Không tự thay thư viện hoặc chuyển database riêng trong lúc đang đối chiếu.

EF Core là công cụ truy cập và ánh xạ dữ liệu. Sử dụng EF Core vẫn phải tuân theo kiến trúc ba lớp.

## Kiến trúc ba lớp

Solution trong repository nhóm: `Thuyetminh/ThuyetMinh.sln`.

| Project | Thành phần | Trách nhiệm |
|---|---|---|
| `ThuyetMinh.Api` | Controller, giao diện, cấu hình ứng dụng | Nhận yêu cầu, gọi nghiệp vụ và trả kết quả |
| `ThuyetMinh.Business` | Service | Xử lý quy tắc nghiệp vụ |
| `ThuyetMinh.Data` | Repository, model dữ liệu, cấu hình truy cập database | Đọc và ghi dữ liệu |

- Controller gọi Service; Service gọi Repository.
- Không truy vấn database trực tiếp trong Controller hoặc mã xử lý giao diện.
- Nếu dùng EF Core, Repository sử dụng `AppDbContext` để truy cập dữ liệu.
- Giao diện Razor Pages đặt trong `ThuyetMinh.Api/Pages`; tài nguyên CSS, JavaScript và ảnh đặt trong `wwwroot`.
- Mỗi người bổ sung chức năng vào ba project chung, không tạo backend riêng.
- Project kiểm thử là thành phần hỗ trợ, không phải lớp nghiệp vụ thứ tư.

## Thống nhất dữ liệu trước khi tích hợp

Tên bảng và model cuối cùng chưa được chốt. Thư phối hợp đối chiếu các điểm sau:

- Địa điểm dùng chung: quan hệ giữa `Shop` của Chúp và `PoiId` của Nhong.
- Phiên bản nội dung: `VersionId` của Nhong tham chiếu đến bản ghi nào.
- Bản gửi kiểm duyệt: cách sử dụng `ShopSubmission` và nội dung sau khi duyệt.
- Ngôn ngữ: dùng mã chuỗi hay khóa ngoại đến bảng ngôn ngữ.
- Bản dịch và âm thanh: phải xác định được nội dung nguồn tương ứng.
- Kiểu dữ liệu khóa chính và khóa ngoại phải thống nhất.
- Trạng thái kiểm duyệt, dịch và tạo âm thanh phải có ý nghĩa riêng.

Không mặc định `VersionId` chính là mã `ShopSubmission` khi chưa kiểm tra thiết kế.

## Database và các file hiện có của Thư

Bản làm việc cũ của Thư có:

- `ThuyetMinh.Data/Database/schema.sql`.
- `ThuyetMinh.Data/Database/Dbconnection.cs`.
- Database tên `thuyetminh`, script khai báo `utf8mb4` và `utf8mb4_unicode_ci`.

Script ban đầu gồm sáu bảng:

- `roles`
- `permissions`
- `role_permissions`
- `admin_users`
- `poi`
- `poi_localizations`

Đây là thiết kế ban đầu, chưa được xác nhận tương thích với code của Chúp và Nhong.

Trong giai đoạn đối chiếu:

- Không yêu cầu các thành viên chạy script này như cấu trúc cuối cùng.
- Không chép đè các file kết nối hoặc `Program.cs` giữa các bản dự án.
- Không xóa database hoặc dữ liệu hiện có để xử lý khác biệt.
- Nếu chuyển sang migration, Thư hướng dẫn cách chuyển từ trạng thái database thực tế.
- Không duy trì hai nguồn thay đổi cấu trúc độc lập bằng migration và script viết tay.

Kéo code hoặc chạy `dotnet build` không tự bảo đảm database đã được tạo hay cập nhật.

## Cấu hình kết nối

- Dùng User Secrets cho cấu hình bí mật trên máy phát triển.
- Khóa cấu hình dự kiến dùng chung: `ConnectionStrings:DefaultConnection`; cần đối chiếu với code tích hợp.
- Mỗi máy có thể dùng máy chủ, cổng và thông tin đăng nhập riêng.
- Không đưa mật khẩu database hoặc khóa dịch vụ thật lên GitHub.
- Khi chuyển sang bản clone mới, kiểm tra lại cấu hình; không mặc định User Secrets của bản cũ đã được dùng.
- CI/CD sử dụng cấu hình bí mật riêng.

## Khôi phục thư viện và biên dịch

Chạy trong thư mục `Thuyetminh` chứa solution:

```powershell
dotnet restore ThuyetMinh.sln
dotnet build ThuyetMinh.sln
```

- `restore`: khôi phục thư viện theo cấu hình.
- `build`: biên dịch và phát hiện lỗi.
- Khi project kiểm thử đã có trong solution, chạy:

```powershell
dotnet test ThuyetMinh.sln
```

Build thành công chưa chứng minh đăng nhập, kiểm duyệt, thanh toán hoặc trừ thời lượng hoạt động đúng. Cần kiểm thử các luồng tương ứng.

## Quy ước nghiệp vụ

- Người nghe không cần tài khoản; chủ quán và quản trị viên phải đăng nhập.
- Quyền nghe gắn với hồ sơ truy cập được nhận diện bằng cookie.
- Cookie nhận diện trình duyệt, không bảo đảm nhận diện duy nhất thiết bị vật lý.
- Đổi trình duyệt hoặc xóa cookie có thể làm mất khả năng truy cập số dư nếu chưa có cơ chế khôi phục.
- Thời lượng mua và sử dụng được lưu bằng giây.
- Backend kiểm tra và quyết định số dư.
- Không tính thời gian tạm dừng, chờ hoặc tải âm thanh.
- Thông tin phát do trình duyệt gửi phải được backend kiểm tra; không coi đó là bằng chứng tuyệt đối người dùng đang nghe.
- Mỗi hồ sơ truy cập chỉ có một phiên nghe đang hoạt động.
- Thanh toán phải được xác minh; mỗi giao dịch hợp lệ chỉ được cộng thời lượng một lần.
- Nội dung cung cấp cho người nghe phải đủ điều kiện xuất bản.
- Luồng GPS và phát âm thanh phải được kiểm thử trên trình duyệt điện thoại thực tế.

## Phối hợp và bàn giao

- Thư là đầu mối trao đổi, chốt dữ liệu và tích hợp.
- Chúp và Nhong bàn giao theo hợp đồng API thống nhất qua Thư.
- Hợp đồng API gồm đường dẫn, phương thức HTTP, dữ liệu đầu vào, đầu ra và lỗi.
- Thay đổi bảng, trường hoặc API dùng chung phải được trao đổi trước.
- Mỗi người bàn giao cách chạy, cấu hình, cập nhật database, kết quả kiểm thử và phần còn thiếu.
- Mỗi người cập nhật nhật ký và phần báo cáo tiếng Anh.
- Phân biệt rõ kế hoạch, code đã viết và chức năng đã kiểm thử thành công.

### Nhánh làm việc

- Chúp: `feature/truc-module-a`.
- Nhong: `feature/nhong`.
- Thư tích hợp: `feature/thu-integration`.
- `main`: nhận kết quả đã tích hợp và kiểm tra.

Code có trên nhánh riêng chưa đồng nghĩa đã có trong `main`.

Trước khi đồng bộ, kiểm tra:

```powershell
git branch --show-current
git status
```

Lưu công việc bằng commit hoặc stash phù hợp. Khi có xung đột ở file dùng chung, kiểm tra và ghép nội dung cần thiết; không chọn ghi đè toàn bộ theo một phía.

## Việc cần làm tiếp

### Chúp

- [ ] Xác nhận bản hiện tại restore, build và chạy được những chức năng nào.
- [ ] Cung cấp cách khởi tạo database hiện tại.
- [ ] Giải thích luồng `ShopSubmission` và cách lưu nội dung đã duyệt.
- [ ] Phối hợp chuyển cấu hình dữ liệu nếu nhóm chốt EF Core + MariaDB.

### Nhong

- [ ] Giải thích nguồn của `PoiId`, `VersionId`, `LanguageId`.
- [ ] Xác định ý nghĩa các trạng thái bản dịch và âm thanh.
- [ ] Xác nhận cách lưu dữ liệu của các Repository còn lại.
- [ ] Chuyển phần lưu tạm sang database sau khi chốt cấu trúc.

### Thư

- [ ] Kiểm tra khả năng tương thích của phương án EF Core + MariaDB.
- [ ] Đối chiếu phần phiên bản nội dung giữa Chúp và Nhong.
- [ ] Chốt công nghệ, model chung và cách cập nhật database.
- [ ] Tích hợp từng phần, build và kiểm thử.
- [ ] Bổ sung hồ sơ truy cập, thanh toán và phiên nghe trên nền đã thống nhất.

## Trạng thái đã xác nhận

- Bộ khung cũ của Thư đã build thành công.
- Thư đã cấu hình User Secrets và báo kết nối C# đến MariaDB thành công trên bản cũ.
- Thư đã soạn script sáu bảng; lần kiểm tra database được gửi gần nhất cho thấy chưa có bảng. Chưa có kết quả xác nhận sau khi chạy lại script.
- Đã clone repository nhóm vào `D:\Thuyetminh-Nhom`.
- Đã tạo nhánh `feature/thu-integration` trên máy Thư.
- Công việc chưa commit của bản cũ được giữ trong stash tại repository `D:\Thuyetminh`.
- Đã đọc một phần code truy cập dữ liệu của Chúp và Nhong.
- Chưa xác nhận build và chạy thành công bản tích hợp.
- Chưa có kết quả xác nhận hoàn thành CI/CD.
