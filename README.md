### Cài đặt cơ sở dữ liệu chính
Cài đặt cơ sở dữ liệu từ file database.sql 
Chỉnh đường dẫn đến cơ sở dữ liệu trong file: appsettings.json và file OnlineShopContext.cs
### Cài đặt kho lưu trữ hình ảnh
Tải minio qua link sau: https://dl.min.io/server/minio/release/windows-amd64/minio.exe
Lệnh chạy minio: .\minio.exe server D:\minio --console-address :9001
Đăng nhập vào kho lưu trữ ảnh này bằng user và pass minioadmin
Tạo 1 bucket mới tên là shopdogiadung và đẩy các hỉnh ảnh troing thư mục image vào bucket
Lấy accesskey và SecretKey thay vào file appsettings.json
### Tải và cài đặt mongoDB 
Đã cài đặt tự động thêm dữ liệu vào mongoDB chỉ cần cài đặt là tự động thêm vào máy local
### Khác:
Nếu bất kì vấn đề gì xảy ra hãy liên hệ qua email: hongquanlienxo04@gmail.com hoặc zalo: 0981293743


