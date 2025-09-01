# Hệ thống Authentication MonAmour - Hướng dẫn sử dụng

## Tổng quan

Hệ thống authentication đã được triển khai lại hoàn toàn với các tính năng:

### ✨ Tính năng chính
- **Role-based Authentication**: Hỗ trợ 2 role User và Admin
- **Email Verification**: Xác thực email khi đăng ký
- **Password Reset**: Đặt lại mật khẩu qua email
- **Remember Me**: Tự động đăng nhập
- **Comprehensive Logging**: Ghi log đầy đủ cho tất cả hoạt động
- **Role-based Routing**: Admin tự động redirect tới dashboard
- **Enhanced Email Templates**: Email templates đẹp và chuyên nghiệp

## 🚀 Cách sử dụng

### 1. Khởi tạo hệ thống lần đầu

1. **Chạy ứng dụng**
   ```bash
   dotnet run
   ```

2. **Truy cập trang setup**
   ```
   https://localhost:7239/Setup/CreateFirstAdmin
   ```

3. **Tạo admin user đầu tiên**
   - Điền email, mật khẩu, và họ tên
   - Admin user sẽ được tạo và tự động xác thực
   - Sau đó có thể đăng nhập ngay

4. **Tắt chế độ setup**
   - Trong `appsettings.json`, đặt `"AllowSetup": false`

### 2. Đăng nhập

- **User thường**: Đăng nhập → redirect về `/Home/Index`
- **Admin**: Đăng nhập → redirect về `/Admin/Dashboard`

### 3. Quy trình đăng ký User mới

1. User đăng ký tại `/Auth/Signup`
2. Hệ thống gửi email xác thực
3. User click link trong email để xác thực
4. Sau xác thực, user có thể đăng nhập

## 🔧 Cấu trúc Code

### Models đã cập nhật
- **User**: Thêm `LastLoginAt`, `LastLoginIp`
- **Role**: Thêm `Description`, `IsActive`, `CreatedAt`
- **UserRole**: Thêm `UserRoleId`, `IsActive`

### Services
- **AuthService**: Triển khai lại với logging và role management
- **EmailService**: Cải thiện templates và error handling
- **RoleHelper**: Helper class quản lý roles

### Controllers
- **AuthController**: Cập nhật với role-based routing
- **AdminController**: Controller mới cho admin functions
- **SetupController**: Controller setup hệ thống

### Middleware & Helpers
- **RememberMeMiddleware**: Hỗ trợ roles
- **AuthHelper**: Thêm role management functions
- **Authorization Attributes**: `[AdminOnly]`, `[UserOnly]`

## 🛡️ Security Features

### Role-based Authorization
```csharp
[AdminOnly]
public class AdminController : Controller
{
    // Chỉ admin mới truy cập được
}

[Authorize(Role.Names.User)]
public IActionResult UserOnlyAction()
{
    // Chỉ user thường mới truy cập được
}
```

### Session Management
- Lưu trữ UserId, UserEmail, UserName, UserRoles
- Tự động clear khi logout
- Hỗ trợ Remember Me với token

### Email Security
- Token có thời hạn (24h cho verification, 1h cho reset password)
- Tự động vô hiệu hóa token đã sử dụng
- HTML templates với anti-phishing features

## 📊 Logging

Hệ thống ghi log đầy đủ cho:
- Login/Logout attempts
- Email sending
- Role assignments
- Authentication failures
- System errors

### Log Levels
- **Information**: Successful operations
- **Warning**: Security warnings, invalid attempts
- **Error**: System errors, email failures
- **Debug**: Detailed service operations

## 🔄 API Changes

### AuthService Methods (Breaking Changes)
```csharp
// Old
Task<bool> LoginAsync(LoginViewModel model)

// New
Task<(bool Success, string? ErrorMessage)> LoginAsync(LoginViewModel model)
```

Tất cả auth methods giờ trả về tuple với success flag và error message.

### New Role Methods
```csharp
Task<List<string>> GetUserRolesAsync(int userId)
Task<bool> HasRoleAsync(int userId, string roleName)
Task<bool> IsAdminAsync(int userId)
Task<bool> AssignRoleToUserAsync(int userId, string roleName)
```

## 🎨 UI Updates

### Admin Dashboard
- Responsive dashboard tại `/Admin/Dashboard`
- Quick stats và action buttons
- Modern card-based layout

### Email Templates
- Professional HTML templates
- Responsive design
- Brand consistent styling
- Security warnings và instructions

## ⚙️ Configuration

### appsettings.json
```json
{
  "AllowSetup": true,  // Tắt sau khi setup xong
  "Logging": {
    "LogLevel": {
      "MonAmour.Services": "Debug",
      "MonAmour.Controllers": "Information"
    }
  },
  "Email": {
    // Email configuration
  },
  "AppSettings": {
    "BaseUrl": "https://localhost:7239"
  }
}
```

## 🚨 Important Notes

### Security
1. **Luôn tắt AllowSetup sau khi setup**: `"AllowSetup": false`
2. **Sử dụng HTTPS trong production**
3. **Cấu hình email server đúng cách**
4. **Regularly monitor logs cho security issues**

### Performance
1. **Role checks được cache trong session**
2. **Email sending là async operations**
3. **Database queries được optimize với Include()**

### Maintenance
1. **Regularly clean up expired tokens**
2. **Monitor email delivery rates**
3. **Review security logs**
4. **Update email templates khi cần**

## 🔍 Troubleshooting

### Common Issues

1. **Admin không thể truy cập dashboard**
   - Kiểm tra user có role Admin không
   - Kiểm tra session có UserRoles không
   - Check logs cho authorization errors

2. **Email không được gửi**
   - Kiểm tra SMTP configuration
   - Check email service logs
   - Verify email credentials

3. **Remember Me không hoạt động**
   - Kiểm tra cookie settings
   - Verify token trong database
   - Check middleware order

### Debug Commands
```bash
# Check logs
tail -f logs/app.log

# Database queries
# Kiểm tra roles trong database
SELECT * FROM Roles
SELECT * FROM UserRoles WHERE UserId = [user_id]
```

## 📝 Testing

### Manual Testing Steps

1. **User Registration Flow**
   - Đăng ký user mới
   - Check email verification
   - Xác thực và đăng nhập

2. **Admin Functions**
   - Tạo admin user
   - Đăng nhập admin
   - Truy cập admin dashboard

3. **Password Reset**
   - Request password reset
   - Check email
   - Reset password thành công

4. **Remember Me**
   - Đăng nhập với Remember Me
   - Close browser
   - Mở lại, kiểm tra auto-login

---

## 🎯 Next Steps

1. **Implement user management interface** trong admin dashboard
2. **Add audit logging** cho admin actions
3. **Implement 2FA** cho admin accounts
4. **Add rate limiting** cho login attempts
5. **Create admin user management** features

---

*Tài liệu này được tạo cùng với việc triển khai lại hệ thống authentication. Vui lòng cập nhật khi có thay đổi.*
