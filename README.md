# NHẬT KÝ LÀM VIỆC NHÓM — 3 TUẦN ĐẦU

**Đề tài:** Hệ thống thuyết minh tự động đa ngôn ngữ  
**Hướng triển khai:** Backend theo kiến trúc 3 lớp, tích hợp CI/CD  
**Nhóm:** 05 
**Thành viên:** 
Lưu Huyền Thư / 
Nguyễn Ngọc Xuân Trúc / 
Huỳnh Thị Mỹ Tiên
## Tuần 1: Tìm hiểu đề tài và lựa chọn hướng triển khai
### Mục tiêu

Hiểu yêu cầu của đề tài thuyết minh tự động đa ngôn ngữ và xác định hướng thực hiện phù hợp với nhóm.

### Nội dung công việc

- Tìm hiểu các hướng triển khai: frontend, backend theo kiến trúc 3 lớp và backend gồm các dịch vụ giao tiếp với nhau.
- Lựa chọn hướng phát triển backend theo kiến trúc 3 lớp, có tích hợp CI/CD.
- Xem xét tài liệu tham khảo về hệ thống du lịch ẩm thực Quận 4.
- Tìm hiểu khái quát các thành phần: địa điểm, nội dung thuyết minh, ngôn ngữ, âm thanh và vị trí GPS.
- Ghi nhận các vấn đề chưa rõ về phạm vi chức năng, sản phẩm bàn giao và trách nhiệm frontend–backend.

### Kết quả

- Xác định đề tài và hướng kỹ thuật của nhóm.
- Nhận diện được luồng tổng quát: người dùng đến địa điểm và nghe nội dung giới thiệu bằng ngôn ngữ lựa chọn.
- Xác định cần làm rõ yêu cầu trước khi thiết kế dữ liệu và lập trình.

### Khó khăn, tồn đọng

- Tài liệu tham khảo mô tả nhiều chức năng, nhóm chưa phân biệt rõ phần cốt lõi và phần mở rộng.
- Chưa xác định đầy đủ các đối tượng sử dụng và quy tắc nghiệp vụ.

### Công việc tiếp theo

Phân tích hành trình người dùng, xác định các đối tượng tham gia và tổng hợp câu hỏi để làm rõ phạm vi đề án.

## Tuần 2: Phân tích đối tượng sử dụng và luồng nghiệp vụ
### Mục tiêu

Xác định hệ thống phục vụ những đối tượng nào và mỗi đối tượng thực hiện những công việc gì.

### Nội dung công việc

- Tổng hợp câu hỏi về chức năng bắt buộc, cách kích hoạt thuyết minh, nội dung đa ngôn ngữ và yêu cầu CI/CD.
- Xác định ba đối tượng chính: người dùng, chủ quán và quản trị viên.
- Làm rõ vai trò của nhiều chủ quán: mỗi chủ quán cung cấp dữ liệu của quán mình cho quản trị viên xử lý.
- Phác thảo quy trình gửi, kiểm tra, duyệt và công bố nội dung.
- Xác định luồng sử dụng có thu phí: người dùng thanh toán qua QR trước khi sử dụng thuyết minh.
- Xác định GPS là cơ sở để kích hoạt bài thuyết minh khi người dùng đến gần địa điểm.

### Kết quả

| Đối tượng | Vai trò sơ bộ |
|---|---|
| Người dùng | Thanh toán, chọn ngôn ngữ và nghe thuyết minh theo vị trí |
| Chủ quán | Cung cấp và đề nghị cập nhật thông tin của quán mình |
| Quản trị viên | Xử lý, duyệt và công bố nội dung; quản lý hoạt động hệ thống |

Luồng nghiệp vụ sơ bộ: **chủ quán gửi dữ liệu → quản trị viên xử lý và công bố → người dùng thanh toán → sử dụng thuyết minh theo GPS**.

### Khó khăn, tồn đọng

- Chưa phân biệt rõ thời gian truy cập ứng dụng và thời gian thực tế phát thuyết minh.
- Cần xác định cách nhận diện người đã thanh toán và lưu quyền sử dụng.
- Chưa chốt mức độ đầu tư giao diện.

### Công việc tiếp theo

Làm rõ cách tính phí theo thời lượng, yêu cầu đăng nhập và phạm vi giao diện phục vụ backend.

## Tuần 3: Làm rõ thời lượng nghe và phạm vi backend

### Mục tiêu

Hoàn thiện định hướng nghiệp vụ cốt lõi và xác định các phần backend cần triển khai.

### Nội dung công việc

- Xác định người dùng mua **số phút nghe thực tế**, không phải thời gian truy cập liên tục.
- Xác định nguyên tắc: chỉ trừ thời lượng khi âm thanh đang phát; không trừ khi tạm dừng hoặc chưa phát bài.
- Xác định người dùng nghe thuyết minh không cần đăng nhập.
- Đề xuất cơ chế vé truy cập ẩn danh để liên kết giao dịch thanh toán với thời lượng còn lại.
- Xác định giao diện sử dụng các trang và form đơn giản để thao tác, tập trung phát triển backend.
- Phân chia trách nhiệm ba lớp: tiếp nhận yêu cầu, xử lý nghiệp vụ và truy cập dữ liệu.
- Xác định nhu cầu kiểm thử tự động và tích hợp CI/CD trong quá trình phát triển.

### Kết quả

Các yêu cầu đã xác định:

- Hệ thống có ba đối tượng: người dùng, chủ quán và quản trị viên.
- Người dùng không cần đăng nhập để mua và sử dụng phút nghe.
- Người dùng thanh toán qua QR để nhận thời lượng nghe.
- Thời lượng được tính theo thời gian âm thanh thực sự phát.
- GPS được sử dụng để kích hoạt nội dung tại địa điểm.
- Chủ quán cung cấp dữ liệu cho quản trị viên xử lý.
- Giao diện đơn giản; trọng tâm là backend 3 lớp và CI/CD.

Các nhóm nghiệp vụ backend dự kiến:

| Nhóm nghiệp vụ | Nội dung |
|---|---|
| Thanh toán | Tạo đơn, xác nhận kết quả và tránh cộng thời lượng trùng |
| Quyền nghe | Quản lý vé truy cập và số giây còn lại |
| Phiên nghe | Ghi nhận thời gian phát, trừ thời lượng và xử lý hết phút |
| Nội dung | Tiếp nhận dữ liệu chủ quán, duyệt và công bố |
| Đa ngôn ngữ | Quản lý bản dịch và âm thanh theo ngôn ngữ |
| Địa điểm | Quản lý tọa độ và thông tin phục vụ kích hoạt theo GPS |

### Vấn đề cần tiếp tục thống nhất

- Giá và thời lượng của từng gói nghe.
- Cách khôi phục quyền nghe khi đổi thiết bị hoặc mất dữ liệu trình duyệt.
- Cách xử lý khi mất mạng, đóng ứng dụng hoặc mở nhiều phiên nghe.
- Ngôn ngữ hỗ trợ và quy trình tạo bản dịch, âm thanh.
- Hình thức tích hợp thanh toán và môi trường triển khai.
- Công nghệ sử dụng và phân công công việc cụ thể.

### Kế hoạch tuần tiếp theo

- Hoàn thiện danh sách yêu cầu và tiêu chí nghiệm thu.
- Thiết kế cơ sở dữ liệu và các API chính.
- Tạo cấu trúc dự án backend theo ba lớp.
- Phân công người phụ trách, người kiểm tra và thời hạn cho từng nhiệm vụ.
- Thiết lập CI ban đầu để kiểm tra mã nguồn.
- Bắt đầu triển khai quản lý địa điểm và quy trình gửi, duyệt nội dung.

## Đánh giá sau ba tuần

Nhóm đã làm rõ định hướng sản phẩm: hệ thống thuyết minh đa ngôn ngữ theo GPS, có thu phí dựa trên số phút nghe thực tế và không yêu cầu người nghe đăng nhập. Giai đoạn tiếp theo tập trung chuyển các yêu cầu đã xác định thành thiết kế dữ liệu, API và chức năng backend có thể kiểm thử.
