## Phần Chúp

## Phần Nhong

## Phần Thư 
- Viết và chạy dự án bằng **VS Code**, sử dụng **.NET 10 SDK**.
- Backend dùng **C# và ASP.NET Core Web API**, tổ chức theo kiến trúc ba lớp.
- **MySQL Workbench** là công cụ thao tác dữ liệu. Máy t hiện kết nối đến **MariaDB 10.4.32**, không phải MySQL Server. 2 máy còn lại kiểm tra máy mình bằng `SELECT VERSION();` và báo lại để thống nhất môi trường.

Cả nhóm sử dụng solution **`ThuyetMinh.sln`** với ba project:

ThuyetMinh.Api (Controller): nhận yêu cầu và trả kết quả 
ThuyetMinh.Business( Service): xử lý nghiệp vụ 
ThuyetMinh.Data( Repository): đọc, ghi cơ sở dữ liệu 

- Mỗi người thêm chức năng vào bộ khung chung, không tạo backend riêng.
- Controller gọi Service; Service gọi Repository.
- Không viết SQL trực tiếp trong Controller.
- Bộ khung hiện đã build thành công cả ba project.

- Tên database thống nhất: **`thuyetminh`**, viết thường.
- Bộ mã ký tự: **`utf8mb4`**, hỗ trợ nội dung đa ngôn ngữ.
- Quy tắc so sánh văn bản mặc định: **`utf8mb4_unicode_ci`**.
- Mỗi người tạo database trên máy mình bằng cùng script:

CREATE DATABASE IF NOT EXISTS thuyetminh
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;
USE thuyetminh;
SELECT DATABASE();

- Script này được lưu chung tại **`database/001_create_database.sql`**.
- Kéo code từ GitHub không tự tạo database; mỗi người phải chạy script.
- Chưa tự tạo hoặc đổi tên bảng dùng chung. 2 máy gửi thiết kế dữ liệu phần mình cho t tổng hợp trước.
- `IF NOT EXISTS` không sửa bộ mã ký tự của database đã tồn tại; nếu đã có database cùng tên, báo t để kiểm tra.

- Dùng **`MySqlConnector`** để C# kết nối và thực thi SQL với MySQL/MariaDB.
- Thư viện được cài trong **`ThuyetMinh.Data`**.
- t đang thực hiện bước cài đặt và sẽ đưa phiên bản cụ thể vào tệp `ThuyetMinh.Data.csproj`.
- Sau khi kéo bản có cấu hình thư viện, 2 máy chạy tại thư mục chứa solution:

dotnet restore ThuyetMinh.sln
dotnet build ThuyetMinh.sln

- `restore` tải các thư viện theo cấu hình dự án; `build` biên dịch để kiểm tra lỗi.
- Không tự cài thêm thư viện kết nối khác hoặc đổi phiên bản riêng.

- Tên máy chủ, cổng, tài khoản và mật khẩu có thể khác nhau giữa các máy.
- t sẽ hướng dẫn cấu hình bằng **User Secrets** ở bước tiếp theo.
- Không đưa mật khẩu thật vào `appsettings.json`, script SQL hoặc GitHub.
- Không gửi mật khẩu cho nhau để cấu hình dự án.

- Chúp phụ trách tài khoản chủ quán/admin, quán và kiểm duyệt.
- Nhong phụ trách nội dung đa ngôn ngữ, âm thanh và GPS.
- T phụ trách thanh toán, thời lượng nghe, dữ liệu/API chung và tích hợp.
- Khi cần thay đổi tên trường, bảng hoặc định dạng API dùng chung, trao đổi với t trước.
- Mỗi người cập nhật kiểm thử, nhật ký và phần báo cáo tiếng Anh của chức năng mình làm.

**Trạng thái hiện tại:** bộ khung đã build thành công; đang thiết lập thư viện và database. Chưa xác nhận kết nối C# đến database, chưa triển khai API nghiệp vụ.
