# Hướng dẫn Authentication - MonAmour

## Tổng quan
Hệ thống authentication đã được triển khai đầy đủ với các tính năng sau:

## Các tính năng đã triển khai

### 1. Đăng nhập (Login)
- **Endpoint**: `POST /Auth/Login`
- **Tính năng**:
  - Xác thực email và mật khẩu
  - Tạo session cho user
  - Hỗ trợ "Remember Me" với cookie 30 ngày
  - Kiểm tra trạng thái tài khoản (Active/Pending)

### 2. Đăng ký (Signup)
- **Endpoint**: `POST /Auth/Signup`
- **Tính năng**:
  - Tạo tài khoản mới
  - Hash mật khẩu với SHA256
  - Gửi email xác thực tự động
  - Kiểm tra email trùng lặp

### 3. Quên mật khẩu (Forgot Password)
- **Endpoint**: `POST /Auth/ForgotPassword`
- **Tính năng**:
  - Gửi email reset password
  - Token có thời hạn 1 giờ
  - Bảo mật với token ngẫu nhiên

### 4. Đặt lại mật khẩu (Reset Password)
- **Endpoint**: `GET/POST /Auth/ResetPassword`
- **Tính năng**:
  - Xác thực token reset password
  - Cập nhật mật khẩu mới
  - Vô hiệu hóa token sau khi sử dụng

### 5. Xác thực email (Email Verification)
- **Endpoint**: `GET /Auth/VerifyEmail`
- **Tính năng**:
  - Xác thực tài khoản qua email
  - Token có thời hạn 24 giờ
  - Kích hoạt tài khoản sau khi xác thực

### 6. Gửi lại email xác thực (Resend Verification)
- **Endpoint**: `POST /Auth/ResendVerification`
- **Tính năng**:
  - Gửi lại email xác thực
  - Vô hiệu hóa token cũ
  - Tạo token mới

### 7. Đăng xuất (Logout)
- **Endpoint**: `POST /Auth/Logout`
- **Tính năng**:
  - Xóa session
  - Xóa remember me cookie
  - Vô hiệu hóa remember me token

### 8. Remember Me
- **Tính năng**:
  - Cookie tự động đăng nhập 30 ngày
  - Middleware tự động kiểm tra và đăng nhập
  - Bảo mật với token database

## Cấu trúc Database

### Bảng Users
- `UserId`: ID người dùng
- `Email`: Email đăng nhập
- `Password`: Mật khẩu đã hash
- `Name`: Tên người dùng
- `Phone`: Số điện thoại
- `Verified`: Trạng thái xác thực email
- `Status`: Trạng thái tài khoản (Active/Pending)

### Bảng Tokens
- `TokenId`: ID token
- `UserId`: ID người dùng
- `TokenValue`: Giá trị token
- `TokenType`: Loại token (EmailVerification/PasswordReset/RememberMe)
- `ExpiresAt`: Thời gian hết hạn
- `IsActive`: Trạng thái hoạt động
- `UsedAt`: Thời gian sử dụng

## Cấu hình

### Program.cs
```csharp
// Đã cấu hình:
- Entity Framework với SQL Server
- Session management
- HttpContextAccessor
- Dependency Injection cho services
- Remember Me middleware
```

### Services
- `IAuthService`: Interface cho authentication
- `AuthService`: Implementation chính
- `IEmailService`: Interface cho email
- `EmailService`: Implementation email (cần cấu hình SMTP)

## Sử dụng

### 1. Kiểm tra đăng nhập
```csharp
// Trong controller
if (AuthHelper.IsLoggedIn(HttpContext))
{
    var userId = AuthHelper.GetUserId(HttpContext);
    var userEmail = AuthHelper.GetUserEmail(HttpContext);
}
```

### 2. Đăng xuất
```csharp
// Trong controller
await _authService.LogoutAsync();
```

### 3. Kiểm tra token
```csharp
// Kiểm tra token hợp lệ
var isValid = await _authService.IsTokenValidAsync(token, "EmailVerification");
```

## Bảo mật

### Mật khẩu
- Hash với SHA256
- Validation mạnh (chữ hoa, thường, số)
- Tối thiểu 8 ký tự

### Token
- Token ngẫu nhiên 32 bytes
- Thời hạn khác nhau cho từng loại
- Vô hiệu hóa sau khi sử dụng

### Session
- HttpOnly cookies
- Secure cookies
- Session timeout 30 phút

## Cần cấu hình thêm

### 1. Email Service
Hiện tại EmailService chỉ log ra console. Cần cấu hình:
- SendGrid
- SMTP
- Hoặc service email khác

### 2. Connection String
Cập nhật connection string trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-connection-string"
  }
}
```

### 3. Email Configuration
Cấu hình email trong `appsettings.json`:
```json
{
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "From": "your-email@gmail.com"
  }
}
```

## Testing

### Test Cases cần thực hiện:
1. Đăng ký tài khoản mới
2. Đăng nhập với thông tin đúng/sai
3. Quên mật khẩu và reset
4. Xác thực email
5. Remember me functionality
6. Đăng xuất
7. Session timeout

## Lưu ý
- Tất cả mật khẩu được hash trước khi lưu database
- Token được tạo ngẫu nhiên và có thời hạn
- Session được quản lý tự động
- Remember me cookie được bảo mật
- Email service cần được cấu hình để hoạt động thực tế
