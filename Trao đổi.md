# Phân công và quy ước triển khai backend

Đề tài: Thuyết minh tự động đa ngôn ngữ.

Định hướng: phát triển backend theo kiến trúc ba lớp, kết hợp CI/CD. Giao diện sử dụng các biểu mẫu và trình phát đơn giản để thao tác, kiểm thử và trình diễn chức năng.

## Phần Chúp

- Xây dựng chức năng đăng nhập và phân quyền cho chủ quán, quản trị viên.
- Quản lý thông tin quán/địa điểm: tên, địa chỉ, tọa độ, bán kính kích hoạt và nội dung thuyết minh gốc.
- Xây dựng luồng chủ quán gửi nội dung và quản trị viên kiểm duyệt.
- Kiểm tra chủ quán chỉ được thao tác dữ liệu thuộc quyền quản lý của mình.
- Làm giao diện đơn giản cho chủ quán và quản trị viên; thống nhất mẫu biểu và CSS dùng chung.
- Phụ trách chính các bảng: `roles`, `permissions`, `role_permissions`, `admin_users`, `poi`.
- Viết phần báo cáo tiếng Anh về tài khoản, phân quyền, quản lý địa điểm và kiểm duyệt; tổng hợp, chỉnh sửa báo cáo chung.

## Phần Nhong

- Xây dựng chức năng quản lý bản dịch và nội dung thuyết minh theo ngôn ngữ.
- Tích hợp chuyển văn bản thành giọng nói (Text-to-Speech — TTS).
- Quản lý trạng thái tạo âm thanh và đường dẫn tệp âm thanh.
- Xử lý lấy vị trí GPS trên trình duyệt và xác định địa điểm nằm trong bán kính kích hoạt.
- Làm giao diện chọn ngôn ngữ và trình phát thuyết minh.
- Tích hợp trình phát với API phiên nghe của Thư để báo trạng thái phát, tạm dừng và kết thúc.
- Phụ trách chính bảng `poi_localizations`.
- Thiết lập CI: tự động khôi phục thư viện, biên dịch và chạy kiểm thử khi có thay đổi mã nguồn.
- Viết phần báo cáo tiếng Anh về đa ngôn ngữ, âm thanh, GPS và CI.

## Phần Thư

- Xây dựng và duy trì bộ khung backend ba lớp.
- Xây dựng cơ chế nhận diện lượt truy cập bằng cookie cho người nghe không đăng nhập.
- Quản lý gói nghe, đơn thanh toán và mã QR thanh toán.
- Xác minh kết quả thanh toán trước khi cộng thời lượng; ngăn cộng trùng một giao dịch.
- Quản lý số giây nghe còn lại, phiên nghe và lịch sử cộng/trừ thời lượng.
- Xử lý yêu cầu gửi lặp và ngăn nhiều phiên nghe đồng thời trên cùng hồ sơ truy cập.
- Làm giao diện mua gói, thanh toán và xem thời lượng còn lại.
- Thống nhất cấu trúc dữ liệu, định dạng API và tích hợp chức năng của Chúp, Nhong.
- Phối hợp bảo đảm chỉ nội dung đủ điều kiện xuất bản mới được cung cấp cho người nghe.
- Thiết lập CD: triển khai bản đã vượt qua các bước kiểm tra lên môi trường chạy thử.
- Viết phần báo cáo tiếng Anh về kiến trúc, thanh toán, thời lượng nghe, tích hợp và CD.

## Quy ước công nghệ chung

- Công cụ lập trình: VS Code.
- Nền tảng: .NET 10 SDK.
- Backend: C# và ASP.NET Core Web API.
- Công cụ thao tác cơ sở dữ liệu: MySQL Workbench.
- Máy chủ cơ sở dữ liệu đã xác nhận trên máy Thư: MariaDB 10.4.32.

MySQL Workbench là công cụ quản lý, không phải máy chủ lưu trữ dữ liệu. Chúp và Nhong kiểm tra máy chủ đang sử dụng bằng:

```sql
SELECT VERSION();
```

Báo kết quả cho Thư để thống nhất môi trường trước khi triển khai tiếp.

## Kiến trúc ba lớp

Cả nhóm sử dụng chung solution `ThuyetMinh.sln`:

| Project | Thành phần chính | Trách nhiệm |
|---|---|---|
| `ThuyetMinh.Api` | Controller | Nhận yêu cầu HTTP và trả kết quả |
| `ThuyetMinh.Business` | Service | Xử lý quy tắc nghiệp vụ |
| `ThuyetMinh.Data` | Repository | Đọc, ghi cơ sở dữ liệu |

- Controller gọi Service; Service gọi Repository.
- Không viết SQL trực tiếp trong Controller.
- Mỗi người bổ sung Controller, Service và Repository cho chức năng mình phụ trách vào bộ khung chung.
- Không tạo backend riêng cho từng thành viên.

## Quy ước cơ sở dữ liệu

- Tên database: `thuyetminh`.
- Bộ ký tự: `utf8mb4`.
- Collation: `utf8mb4_unicode_ci`.
- Script khởi tạo: `ThuyetMinh.Data/Database/schema.sql`.
- File hỗ trợ kết nối: `ThuyetMinh.Data/Database/Dbconnection.cs`.
- Thư viện truy cập dữ liệu: `MySqlConnector`, đặt trong `ThuyetMinh.Data`.
- Phiên bản thư viện sử dụng theo khai báo trong `ThuyetMinh.Data.csproj`.

Mỗi người chạy script khởi tạo trên database của máy mình. Kéo code từ GitHub hoặc chạy `dotnet build` không tự tạo database và các bảng.

Không tạo thêm bản sao của script khởi tạo ở vị trí khác. Khi thay đổi cấu trúc bảng đã tồn tại, bổ sung script cập nhật riêng trong `ThuyetMinh.Data/Database` và ghi rõ thứ tự chạy.

`CREATE TABLE IF NOT EXISTS` chỉ tạo bảng khi chưa tồn tại; không cập nhật cấu trúc bảng cũ.

## Khôi phục thư viện và biên dịch

Sau khi kéo code, chạy tại thư mục chứa `ThuyetMinh.sln`:

```powershell
dotnet restore ThuyetMinh.sln
dotnet build ThuyetMinh.sln
```

- `restore`: tải các thư viện theo cấu hình dự án.
- `build`: biên dịch và phát hiện lỗi biên dịch.
- Build thành công chưa chứng minh các chức năng nghiệp vụ hoạt động đúng.
- Không tự đổi thư viện kết nối hoặc nâng phiên bản riêng khi chưa thống nhất.

## Cấu hình kết nối

- Mỗi máy cấu hình kết nối riêng bằng User Secrets trong môi trường phát triển.
- Khóa cấu hình dùng chung: `ConnectionStrings:DefaultConnection`.
- Máy chủ, cổng, tài khoản và mật khẩu có thể khác nhau giữa các máy.
- Không ghi mật khẩu thật vào `Dbconnection.cs`, `appsettings.json`, script SQL hoặc GitHub.
- Không gửi mật khẩu cho nhau để cấu hình dự án.
- Môi trường CI/CD sử dụng cấu hình bí mật riêng, không phụ thuộc User Secrets trên máy Thư.

## Quy ước nghiệp vụ dùng chung

- Người nghe không cần tài khoản; chủ quán và quản trị viên phải đăng nhập.
- Quyền nghe gắn với hồ sơ truy cập được nhận diện qua cookie của trình duyệt.
- Cơ chế này nhận diện trình duyệt, không bảo đảm nhận diện duy nhất thiết bị vật lý.
- Đổi trình duyệt hoặc xóa cookie có thể làm mất khả năng truy cập số dư đã mua nếu chưa có cơ chế khôi phục.
- Thời lượng mua và sử dụng được lưu bằng giây.
- Chỉ tính thời lượng phát thực tế theo cơ chế phiên nghe được backend kiểm tra; không tính thời gian tạm dừng, chờ hoặc tải âm thanh.
- Backend là nơi quyết định số dư; giao diện không tự quyết định việc cộng hoặc trừ thời lượng.
- Mỗi hồ sơ truy cập chỉ được có một phiên nghe đang hoạt động.
- Trạng thái kiểm duyệt nội dung và trạng thái tạo âm thanh phải được quy định riêng.
- Thông tin vị trí và thao tác phát âm thanh cần được kiểm thử trên trình duyệt điện thoại thực tế.

## Phối hợp và bàn giao

- Thư là đầu mối thống nhất dữ liệu, API và tích hợp.
- Chúp và Nhong phát triển theo hợp đồng API đã thống nhất qua Thư.
- Hợp đồng API gồm đường dẫn, phương thức HTTP, dữ liệu đầu vào, kết quả đầu ra và các trường hợp lỗi.
- Thay đổi bảng, tên trường hoặc định dạng API dùng chung phải báo Thư trước khi áp dụng.
- Mỗi người bàn giao code kèm cách chạy, kết quả kiểm thử và những phần chưa hoàn thành.
- Mỗi người cập nhật nhật ký làm việc và phần báo cáo tiếng Anh của mình.
- Chỉ ghi nhận công việc đã thực hiện; phân biệt rõ kế hoạch với kết quả đã kiểm chứng.

## Trạng thái đã xác nhận

- Đã tạo bộ khung ba project.
- Đã build thành công solution.
- Thư đã cấu hình User Secrets và báo kết nối C# đến database thành công.
- Đã soạn `schema.sql` với sáu bảng: `roles`, `permissions`, `role_permissions`, `admin_users`, `poi`, `poi_localizations`.
- Cần kiểm tra kết quả thực thi script để xác nhận các bảng thực tế.
- Chưa có kết quả xác nhận hoàn thành API nghiệp vụ và CI/CD trong bản cập nhật này.
