# Calendar System - OOAD Project

Đây là dự án hệ thống Lịch (Calendar System) sử dụng kiến trúc Clean Architecture (N-Layer), frontend Blazor WebAssembly, và backend ASP.NET Core Web API. Cơ sở dữ liệu đang sử dụng là SQLite.

## Cấu trúc thư mục (Clean Architecture)

- `src/Calendar.API`: Backend API (ASP.NET Core Web API). Chứa các endpoint điều hướng cho hệ thống.
- `src/Calendar.Client`: Frontend App (Blazor WebAssembly). Giao diện người dùng.
- `src/Calendar.Core`: Tầng chứa Domain Entities (Các thực thể cốt lõi) và Interfaces.
- `src/Calendar.Infrastructure`: Tầng Data Access (Entity Framework Core, SQLite) và các dịch vụ giao tiếp bên ngoài (như EmailService).
- `src/Calendar.Service`: Tầng Business Logic chứa các Services xử lý nghiệp vụ của hệ thống (như AppointmentService).
- `src/Calendar.Shared`: Các DTOs (Data Transfer Objects) dùng chung cho cả API và Client.

## Yêu cầu hệ thống (Prerequisites)

Để chạy dự án, máy tính cần cài đặt:
- [.NET 8 SDK](https://dotnet.microsoft.com/download) (Hoặc phiên bản .NET mới nhất tương ứng với dự án).
- IDE khuyên dùng: Visual Studio 2022, JetBrains Rider, hoặc Visual Studio Code.
- Cài đặt công cụ Entity Framework Core: `dotnet tool install --global dotnet-ef`

## Hướng dẫn cài đặt và chạy ứng dụng

### 1. Phục hồi các packages
Mở terminal/command prompt tại thư mục gốc của dự án (`Calendar_OOAD`), chạy lệnh sau để tải các packages cần thiết:
```bash
dotnet restore Calendar_OOAD.sln
```

### 2. Cập nhật cơ sở dữ liệu (Database Migration)
Dự án sử dụng SQLite. Cần chạy lệnh Update Database để tạo các bảng trong cơ sở dữ liệu dựa trên file Migration.
Điều hướng vào thư mục API:
```bash
cd src/Calendar.API
```
Chạy lệnh update:
```bash
dotnet ef database update --project ../Calendar.Infrastructure
```

### 3. Cấu hình Email (Bắt buộc để gửi thông báo)
Mở file `src/Calendar.API/appsettings.json`, tìm đến phần `"SmtpSettings"` và thay thế `"SenderEmail"` và `"Password"` bằng tài khoản email và [App Password (Mật khẩu ứng dụng)](https://support.google.com/accounts/answer/185833?hl=vi):
```json
  "SmtpSettings": {
    "Server": "smtp.gmail.com",
    "Port": 587,
    "SenderName": "Calendar App",
    "SenderEmail": "your-email@gmail.com",
    "Password": "your-app-password"
  }
```

### 4. Chạy Backend API
Từ thư mục gốc hoặc thư mục `src/Calendar.API`, chạy Backend API:
```bash
cd src/Calendar.API
dotnet run
```
Sau khi chạy, API thường sẽ lắng nghe ở cổng `https://localhost:7000` hoặc `http://localhost:5000` (có thể kiểm tra ở terminal).
*Chú ý: Đảm bảo giữ terminal chạy API để Frontend có thể gọi dữ liệu.*

### 5. Chạy Frontend Client
Mở một terminal khác (terminal thứ hai), điều hướng tới thư mục `src/Calendar.Client` và chạy dự án Blazor WebAssembly:
```bash
cd src/Calendar.Client
dotnet run
```
Terminal sẽ hiển thị đường link để truy cập vào ứng dụng trên trình duyệt (thường là `https://localhost:7001` hoặc tương tự). 
Click vào link để mở và sử dụng hệ thống Lịch.

## Các chức năng chính (Các luồng công việc đã hoàn thiện)
- Quản lý cuộc hẹn và lịch trình (Appointments/Meetings).
- Cơ chế gửi thông báo Email tự động cho các thành viên tham gia hoặc khi bị hủy.
- Quản lý trạng thái và luồng Hủy lịch/Rời khỏi lịch.
- Giao diện người dùng Web App tích hợp API backend theo thời gian thực.
