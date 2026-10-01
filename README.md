# NHẬT KÝ LÀM VIỆC NHÓM

## Đề tài: Hệ thống thuyết minh tự động đa ngôn ngữ

**Hướng thực hiện:** Backend Development theo kiến trúc 3 lớp, kết hợp CI/CD.

---

# TUẦN 1 - TÌM HIỂU ĐỀ TÀI VÀ XÁC ĐỊNH HƯỚNG THỰC HIỆN

## Mục tiêu

Trong tuần đầu tiên, nhóm tập trung tìm hiểu nội dung của đề tài **Thuyết minh tự động đa ngôn ngữ**, xác định hệ thống cần giải quyết bài toán gì và lựa chọn hướng triển khai phù hợp với yêu cầu Backend Development theo kiến trúc 3 lớp.

Tuần này nhóm chủ yếu thực hiện quá trình tìm hiểu, chưa bắt đầu xây dựng chương trình.

## Công việc đã thực hiện

- Tìm hiểu yêu cầu chung của đề tài thuyết minh tự động đa ngôn ngữ.
- Thảo luận về cách người dùng có thể cung cấp dữ liệu đầu vào cho hệ thống.
- Xác định ba loại dữ liệu đầu vào dự kiến:
  - Văn bản.
  - Giọng nói.
  - Hình ảnh có chứa văn bản.
- Tìm hiểu quy trình xử lý chung của hệ thống.
- Tìm hiểu Speech To Text.
- Tìm hiểu OCR.
- Tìm hiểu Translation.
- Tìm hiểu Text To Speech.
- Tìm hiểu cách tổ chức Backend theo kiến trúc 3 lớp.
- Tìm hiểu ASP.NET Core Web API để sử dụng làm công nghệ Backend.

## Hướng xử lý được xác định

Nhóm bước đầu xác định hệ thống sẽ xử lý dữ liệu theo hướng:

```text
                    INPUT
                      |
       +--------------+--------------+
       |              |              |
    Văn bản        Giọng nói       Hình ảnh
       |              |              |
       |        Speech To Text        |
       |              |             OCR
       |              |              |
       +--------------+--------------+
                      |
                      v
                    TEXT
                      |
                      v
                 TRANSLATION
                      |
                      v
              TRANSLATED TEXT
                      |
               +------+------+
               |             |
               v             v
          Hiển thị      Text To Speech
                             |
                             v
                           Audio
```

Trong đó, dù dữ liệu đầu vào là văn bản, giọng nói hay hình ảnh thì cuối cùng hệ thống sẽ cố gắng chuyển dữ liệu về dạng văn bản trước khi thực hiện dịch.

## Kiến trúc Backend dự kiến

Nhóm xác định Backend sẽ được tổ chức theo kiến trúc:

```text
Presentation Layer
        |
        v
Business Logic Layer
        |
        v
Data Access Layer
        |
        v
Database
```

Tương ứng với:

```text
Controller
    |
    v
Service
    |
    v
Repository
    |
    v
Database
```

## Kết quả tuần 1

Sau tuần đầu tiên, nhóm đã:

- Hiểu được yêu cầu tổng quát của đề tài.
- Xác định hướng hệ thống hỗ trợ nhiều loại dữ liệu đầu vào.
- Xác định ba loại đầu vào chính gồm Text, Speech và Image.
- Hiểu được vai trò cơ bản của Speech To Text.
- Hiểu được vai trò của OCR.
- Hiểu được vai trò của Translation.
- Hiểu được vai trò của Text To Speech.
- Xác định hướng sử dụng ASP.NET Core Web API.
- Hiểu sơ bộ cách tổ chức Backend theo kiến trúc 3 lớp.

## Khó khăn

Một số vấn đề nhóm chưa xác định được trong tuần này:

- Chưa xác định đầy đủ chức năng của hệ thống.
- Chưa lựa chọn công nghệ cụ thể cho Speech To Text.
- Chưa lựa chọn công nghệ cụ thể cho OCR.
- Chưa lựa chọn Translation Service.
- Chưa lựa chọn Text To Speech Service.
- Chưa thiết kế Database.
- Chưa thiết kế API.

## Kế hoạch tuần 2

- Xác định phạm vi hệ thống.
- Xác định các chức năng chính.
- Xác định Actor.
- Phân tích chi tiết luồng Text, Speech và Image.
- Tìm hiểu cách thiết kế RESTful API.
- Xác định các Entity cần quản lý.
- Xác định cấu trúc Backend 3 lớp chi tiết hơn.
- Tìm hiểu hướng triển khai CI/CD.

---

# TUẦN 2 - PHÂN TÍCH CHỨC NĂNG VÀ CÁCH TRIỂN KHAI HỆ THỐNG

## Mục tiêu

Trong tuần 2, nhóm tiếp tục quá trình tìm hiểu nhưng tập trung nhiều hơn vào cách triển khai thực tế của hệ thống.

Mục tiêu chính là xác định phạm vi của phiên bản đầu tiên, các chức năng cần có và cách tổ chức các thành phần trong Backend trước khi bắt đầu viết code.

## Công việc đã thực hiện

### 1. Xác định phạm vi hệ thống

Nhóm thống nhất phiên bản đầu tiên sẽ tập trung vào ba chức năng chính:

```text
Dịch văn bản
Dịch từ giọng nói
Dịch nội dung văn bản trong hình ảnh
```

Ngoài ra, hệ thống dự kiến có chức năng đọc văn bản đã dịch bằng giọng nói.

Các chức năng quá phức tạp chưa được đưa vào phiên bản đầu tiên như:

- Nhận diện vật thể trong ảnh.
- Nhận diện cảnh vật.
- Clone giọng nói.
- Đồng bộ khẩu hình.
- Dịch video hoàn chỉnh.
- Dịch hội thoại thời gian thực.

### 2. Xác định Actor

Nhóm bước đầu xác định:

```text
Guest
User
```

#### Guest

Có thể:

- Truy cập hệ thống.
- Đăng ký.
- Đăng nhập.

#### User

Có thể:

- Dịch văn bản.
- Dịch từ giọng nói.
- Dịch nội dung trong hình ảnh.
- Chọn ngôn ngữ nguồn.
- Chọn ngôn ngữ đích.
- Nghe nội dung đã dịch.
- Xem lịch sử dịch.

### 3. Xác định các chức năng chính

Các nhóm chức năng dự kiến:

```text
Authentication
Text Translation
Speech Translation
Image Translation
Text To Speech
Translation History
```

### 4. Phân tích luồng dịch văn bản

```text
Người dùng nhập văn bản
          |
          v
Chọn ngôn ngữ nguồn
          |
          v
Chọn ngôn ngữ đích
          |
          v
Translation
          |
          v
Văn bản kết quả
```

### 5. Phân tích luồng dịch giọng nói

```text
Người dùng nói / gửi Audio
          |
          v
Speech To Text
          |
          v
        Text
          |
          v
Translation
          |
          v
Translated Text
```

Nếu người dùng muốn nghe kết quả:

```text
Translated Text
      |
      v
Text To Speech
      |
      v
    Audio
```

### 6. Phân tích luồng dịch hình ảnh

```text
Người dùng tải hình ảnh
          |
          v
         OCR
          |
          v
Trích xuất văn bản
          |
          v
Translation
          |
          v
Translated Text
```

### 7. Xác định cách tái sử dụng chức năng Translation

Nhóm nhận thấy không cần xây dựng ba chức năng dịch hoàn toàn riêng biệt.

Thay vào đó:

```text
Text ---------------------------+
                                |
Speech -> Speech To Text -------+----> TranslationService
                                |
Image -> OCR -------------------+
```

Như vậy `TranslationService` có thể được sử dụng chung cho cả ba loại đầu vào.

### 8. Thiết kế API sơ bộ

Một số API dự kiến:

```text
POST /api/auth/register
POST /api/auth/login

POST /api/translate/text
POST /api/translate/speech
POST /api/translate/image

POST /api/speech/synthesize

GET /api/history
GET /api/history/{id}
```

### 9. Xác định Entity sơ bộ

Nhóm bước đầu xác định các Entity:

```text
User
TranslationHistory
MediaFile
```

Trong đó:

```text
User
- UserId
- FullName
- Email
- PasswordHash
- CreatedAt
```

```text
TranslationHistory
- HistoryId
- UserId
- InputType
- SourceLanguage
- TargetLanguage
- OriginalText
- TranslatedText
- CreatedAt
```

`InputType` dự kiến gồm:

```text
TEXT
SPEECH
IMAGE
```

### 10. Xác định cấu trúc Backend

Cấu trúc dự kiến:

```text
MultilingualTranslator
│
├── MultilingualTranslator.API
├── MultilingualTranslator.Business
├── MultilingualTranslator.Data
└── MultilingualTranslator.Tests
```

Trong đó:

```text
API
 |
 v
Business
 |
 v
Data
```

### 11. Tìm hiểu hướng triển khai CI/CD

Nhóm tìm hiểu quy trình CI:

```text
Push / Pull Request
        |
        v
GitHub Actions
        |
        v
Restore
        |
        v
Build
        |
        v
Test
```

Quy trình CD dự kiến:

```text
Merge main
    |
    v
Build
    |
    v
Test
    |
    v
Docker
    |
    v
Deploy
```

Trong tuần 2 nhóm mới dừng ở mức tìm hiểu cách triển khai, chưa xây dựng Pipeline thực tế.

## Kết quả tuần 2

Sau tuần 2, nhóm đã:

- Xác định phạm vi MVP.
- Xác định Actor.
- Xác định các chức năng chính.
- Phân tích luồng xử lý Text.
- Phân tích luồng xử lý Speech.
- Phân tích luồng xử lý Image.
- Xác định Translation là chức năng xử lý chung.
- Xây dựng danh sách API sơ bộ.
- Xác định Entity ban đầu.
- Xác định cấu trúc Backend theo kiến trúc 3 lớp.
- Có định hướng ban đầu cho CI/CD.

## Khó khăn

Một số vấn đề chưa hoàn thiện:

- API mới ở mức thiết kế sơ bộ.
- Database chưa được thiết kế đầy đủ.
- Chưa lựa chọn Translation Service cụ thể.
- Chưa lựa chọn Speech To Text cụ thể.
- Chưa lựa chọn OCR cụ thể.
- Chưa lựa chọn Text To Speech cụ thể.
- Chưa triển khai CI/CD thực tế.

## Kế hoạch tuần 3

- Bắt đầu tạo Backend.
- Khởi tạo ASP.NET Core Web API.
- Tạo Solution.
- Tạo cấu trúc 3 lớp.
- Cấu hình Swagger.
- Tạo API kiểm tra.
- Bắt đầu thử nghiệm chức năng Text Translation.
- Chuẩn bị cấu trúc Database.

---

# TUẦN 3 - BẮT ĐẦU XÂY DỰNG BACKEND CƠ BẢN

## Mục tiêu

Sau hai tuần tìm hiểu và phân tích, nhóm bắt đầu chuyển sang giai đoạn thực hiện.

Trong tuần này nhóm chưa thực hiện các chức năng lớn mà tập trung xây dựng nền tảng Backend để chuẩn bị cho việc phát triển các chức năng sau này.

## Công việc đã thực hiện

### 1. Khởi tạo Backend

Nhóm bắt đầu tạo project sử dụng:

```text
ASP.NET Core Web API
```

Solution dự kiến:

```text
MultilingualTranslator.sln
```

### 2. Tạo cấu trúc 3 lớp

Cấu trúc ban đầu:

```text
MultilingualTranslator
│
├── MultilingualTranslator.API
├── MultilingualTranslator.Business
└── MultilingualTranslator.Data
```

### 3. Tạo Presentation Layer

Project:

```text
MultilingualTranslator.API
```

Cấu trúc:

```text
Controllers
Program.cs
appsettings.json
```

Presentation Layer được sử dụng để tiếp nhận HTTP Request và trả Response cho Client.

### 4. Tạo Business Logic Layer

Project:

```text
MultilingualTranslator.Business
```

Cấu trúc:

```text
Interfaces
Services
DTOs
```

Business Layer sẽ chứa các xử lý nghiệp vụ chính.

### 5. Tạo Data Access Layer

Project:

```text
MultilingualTranslator.Data
```

Cấu trúc dự kiến:

```text
Entities
Interfaces
Repositories
AppDbContext
```

### 6. Xác định luồng xử lý giữa các Layer

```text
Controller
    |
    v
Service
    |
    v
Repository
    |
    v
Database
```

Nhóm thống nhất:

- Controller không truy cập trực tiếp Database.
- Controller chủ yếu nhận Request và trả Response.
- Service xử lý nghiệp vụ.
- Repository làm việc với dữ liệu.

### 7. Kiểm tra project

Nhóm tiến hành Build và chạy thử Backend.

```bash
dotnet build
```

Sau đó:

```bash
dotnet run
```

Mục tiêu là đảm bảo cấu trúc project có thể chạy trước khi xây dựng các chức năng nghiệp vụ.

### 8. Cấu hình Swagger

Swagger được sử dụng để:

- Xem các Endpoint.
- Gửi Request.
- Kiểm tra Response.
- Test Backend trong thời gian chưa có Frontend hoàn chỉnh.

### 9. Tạo API kiểm tra

Nhóm bắt đầu bằng một API đơn giản:

```http
GET /api/health
```

Response dự kiến:

```json
{
  "status": "OK"
}
```

API này được sử dụng để kiểm tra:

- Controller hoạt động.
- Routing hoạt động.
- Swagger hoạt động.
- Backend trả Response bình thường.

### 10. Bắt đầu chuẩn bị chức năng Text Translation

Nhóm lựa chọn Text Translation làm chức năng đầu tiên vì đây là luồng đơn giản nhất và sẽ được tái sử dụng cho Speech và Image sau này.

Luồng dự kiến:

```text
Text Input
    |
    v
TranslationController
    |
    v
TranslationService
    |
    v
Translation Provider
    |
    v
Translated Text
```

API dự kiến:

```http
POST /api/translate/text
```

## Kết quả tuần 3

Sau tuần 3, nhóm đã:

- Bắt đầu xây dựng Backend.
- Tạo Solution.
- Tạo các Project cho kiến trúc 3 lớp.
- Xác định rõ Controller, Service và Repository.
- Kiểm tra Backend có thể Build.
- Kiểm tra Backend có thể chạy.
- Cấu hình Swagger.
- Chuẩn bị API kiểm tra.
- Bắt đầu xây dựng cấu trúc cho Text Translation.

## Khó khăn

- Cần thống nhất cách tổ chức Interface và Service.
- Database vẫn chưa hoàn thiện.
- Chưa lựa chọn Translation Provider chính thức.
- Chưa tích hợp các dịch vụ bên ngoài.
- Cần tìm hiểu thêm Dependency Injection trong ASP.NET Core.

## Kế hoạch tuần 4

- Hoàn thiện cấu trúc Text Translation.
- Thiết kế Database cơ bản.
- Tạo các Entity đầu tiên.
- Tạo `AppDbContext`.
- Kết nối SQL Server.
- Tạo Migration.
- Thử nghiệm lưu lịch sử dịch.
- Chuẩn bị CI cơ bản.

---

# TUẦN 4 - TRIỂN KHAI CHỨC NĂNG DỊCH VĂN BẢN VÀ DATABASE

## Mục tiêu

Trong tuần 4, nhóm bắt đầu triển khai chức năng nghiệp vụ đầu tiên của hệ thống là **dịch văn bản**, đồng thời xây dựng Database cơ bản để phục vụ việc lưu thông tin người dùng và lịch sử dịch.

## Công việc đã thực hiện

### 1. Hoàn thiện cấu trúc Text Translation

Nhóm tiếp tục xây dựng luồng:

```text
Client
  |
  v
TranslationController
  |
  v
TranslationService
  |
  v
Translation Provider
  |
  v
Translated Text
  |
  v
Response
```

API:

```http
POST /api/translate/text
```

Request dự kiến:

```json
{
  "text": "Xin chào",
  "sourceLanguage": "vi",
  "targetLanguage": "en"
}
```

Response dự kiến:

```json
{
  "originalText": "Xin chào",
  "translatedText": "Hello",
  "sourceLanguage": "vi",
  "targetLanguage": "en"
}
```

### 2. Tạo DTO cho chức năng dịch

Nhóm bắt đầu tách dữ liệu Request và Response khỏi Entity.

Các DTO dự kiến:

```text
TranslateTextRequest
TranslateTextResponse
```

Mục tiêu là tránh sử dụng trực tiếp Entity làm dữ liệu truyền qua API.

### 3. Thiết kế Database cơ bản

Nhóm bắt đầu xây dựng Database cho hệ thống.

Các Entity đầu tiên:

```text
User
TranslationHistory
```

### 4. Xây dựng Entity User

Thông tin dự kiến:

```text
User
----------------------
UserId
FullName
Email
PasswordHash
CreatedAt
```

### 5. Xây dựng Entity TranslationHistory

Thông tin dự kiến:

```text
TranslationHistory
----------------------
HistoryId
UserId
InputType
SourceLanguage
TargetLanguage
OriginalText
TranslatedText
CreatedAt
```

### 6. Xác định quan hệ dữ liệu

Một User có thể có nhiều TranslationHistory.

```text
USER
  1
  |
  |
  N
TRANSLATION_HISTORY
```

### 7. Chuẩn bị AppDbContext

Nhóm bắt đầu tạo:

```text
AppDbContext
```

để Entity Framework Core làm việc với Database.

Dự kiến:

```text
AppDbContext
│
├── Users
└── TranslationHistories
```

### 8. Cấu hình kết nối Database

Nhóm chuẩn bị cấu hình:

```text
Connection String
```

trong:

```text
appsettings.json
```

Backend dự kiến kết nối với:

```text
SQL Server
```

### 9. Tìm hiểu và chuẩn bị Migration

Nhóm bắt đầu tìm hiểu và sử dụng Migration để tạo Database từ các Entity.

Quy trình:

```text
Entity
   |
   v
Migration
   |
   v
Database
```

### 10. Chuẩn bị lưu lịch sử dịch

Sau khi Translation hoàn thành, kết quả dự kiến được lưu:

```text
TranslationService
        |
        v
TranslationHistoryRepository
        |
        v
Database
```

Thông tin lưu bao gồm:

- Loại dữ liệu đầu vào.
- Ngôn ngữ nguồn.
- Ngôn ngữ đích.
- Văn bản ban đầu.
- Văn bản đã dịch.
- Thời gian thực hiện.

### 11. Chuẩn bị Repository

Nhóm bắt đầu xác định:

```text
ITranslationHistoryRepository
TranslationHistoryRepository
```

để tách phần truy cập Database khỏi Business Logic Layer.

### 12. Chuẩn bị CI cơ bản

Nhóm bắt đầu chuẩn bị workflow CI với mục tiêu đầu tiên là tự động kiểm tra Backend có Build được hay không.

Quy trình dự kiến:

```text
Push Code
    |
    v
GitHub Actions
    |
    v
dotnet restore
    |
    v
dotnet build
```

Sau khi nhóm xây dựng Unit Test, bước:

```text
dotnet test
```

sẽ được bổ sung.

## Kết quả tuần 4

Sau tuần 4, nhóm bước đầu:

- Hoàn thiện hơn cấu trúc Text Translation.
- Xác định Request và Response của API dịch văn bản.
- Bắt đầu sử dụng DTO.
- Thiết kế Database cơ bản.
- Xác định Entity User.
- Xác định Entity TranslationHistory.
- Xác định quan hệ giữa User và TranslationHistory.
- Chuẩn bị AppDbContext.
- Chuẩn bị kết nối SQL Server.
- Tìm hiểu và chuẩn bị Migration.
- Xác định cách lưu lịch sử dịch.
- Chuẩn bị Repository cho lịch sử dịch.
- Bắt đầu chuẩn bị CI cơ bản.

## Khó khăn

Một số vấn đề cần tiếp tục xử lý:

- Translation Provider chưa được lựa chọn chính thức.
- Cần xử lý trường hợp Translation Service bị lỗi.
- Cần hoàn thiện Validation cho dữ liệu đầu vào.
- Cần hoàn thiện cấu hình Entity Framework Core.
- Cần kiểm tra lại thiết kế Database trước khi mở rộng.
- CI mới chỉ ở giai đoạn cơ bản.

## Kế hoạch tuần 5

- Hoàn thiện Text Translation.
- Hoàn thiện kết nối Database.
- Hoàn thiện lưu lịch sử dịch.
- Xây dựng API xem lịch sử.
- Bắt đầu xây dựng chức năng đăng ký và đăng nhập.
- Chuẩn bị JWT Authentication.
- Tiếp tục hoàn thiện CI.
