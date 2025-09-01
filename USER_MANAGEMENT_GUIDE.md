# Hướng dẫn sử dụng chức năng Quản lý User cho Admin

## Tổng quan

Chức năng quản lý user cho admin cung cấp đầy đủ các tính năng CRUD (Create, Read, Update, Delete) để quản lý người dùng trong hệ thống MonAmour.

## Các tính năng chính

### 1. Xem danh sách người dùng
- **URL**: `/Admin/Users`
- **Chức năng**:
  - Hiển thị danh sách tất cả người dùng với phân trang
  - Tìm kiếm theo email, tên, số điện thoại
  - Lọc theo trạng thái, giới tính, xác thực, vai trò
  - Thống kê tổng quan về người dùng

### 2. Tạo người dùng mới
- **URL**: `/Admin/CreateUser`
- **Chức năng**:
  - Tạo tài khoản người dùng mới
  - Gán vai trò cho người dùng
  - Thiết lập trạng thái xác thực
  - Validation đầy đủ thông tin

### 3. Chỉnh sửa thông tin người dùng
- **URL**: `/Admin/EditUser/{id}`
- **Chức năng**:
  - Cập nhật thông tin cá nhân
  - Thay đổi vai trò
  - Cập nhật trạng thái tài khoản

### 4. Xem chi tiết người dùng
- **URL**: `/Admin/UserDetail/{id}`
- **Chức năng**:
  - Hiển thị thông tin chi tiết
  - Xem vai trò và quyền hạn
  - Thống kê hoạt động (đơn hàng, booking, đánh giá)
  - Thao tác nhanh

### 5. Đổi mật khẩu người dùng
- **URL**: `/Admin/ChangeUserPassword/{id}`
- **Chức năng**:
  - Đổi mật khẩu cho người dùng
  - Kiểm tra độ mạnh mật khẩu
  - Validation mật khẩu

### 6. Quản lý trạng thái
- **Chức năng**:
  - Bật/tắt xác thực email
  - Thay đổi trạng thái tài khoản (active/inactive/suspended)
  - Xóa người dùng (có bảo vệ admin)

## Cấu trúc dữ liệu

### User Model
```csharp
public class User
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    public bool? Verified { get; set; }
    public string? Gender { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### UserRole Model
```csharp
public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public DateTime? AssignedAt { get; set; }
    public int? AssignedBy { get; set; }
}
```

## ViewModels

### AdminUserViewModel
- `UserListViewModel`: Hiển thị danh sách user
- `UserCreateViewModel`: Tạo user mới
- `UserEditViewModel`: Chỉnh sửa user
- `UserDetailViewModel`: Chi tiết user
- `UserChangePasswordViewModel`: Đổi mật khẩu
- `UserSearchViewModel`: Tìm kiếm và lọc

## Service Layer

### IUserManagementService
```csharp
public interface IUserManagementService
{
    Task<(List<UserListViewModel> Users, int TotalCount)> GetUsersAsync(UserSearchViewModel searchModel);
    Task<UserDetailViewModel?> GetUserByIdAsync(int userId);
    Task<bool> CreateUserAsync(UserCreateViewModel model, int adminUserId);
    Task<bool> UpdateUserAsync(UserEditViewModel model, int adminUserId);
    Task<bool> DeleteUserAsync(int userId, int adminUserId);
    Task<bool> ChangeUserPasswordAsync(UserChangePasswordViewModel model, int adminUserId);
    Task<bool> ToggleUserVerificationAsync(int userId, int adminUserId);
    Task<bool> ToggleUserStatusAsync(int userId, string status, int adminUserId);
    Task<List<RoleViewModel>> GetAllRolesAsync();
    Task<bool> IsEmailExistsAsync(string email, int? excludeUserId = null);
    Task<Dictionary<string, int>> GetUserStatisticsAsync();
}
```

## Controller Actions

### AdminController
- `Users()`: Hiển thị danh sách user
- `CreateUser()`: GET/POST tạo user mới
- `EditUser(int id)`: GET/POST chỉnh sửa user
- `UserDetail(int id)`: Xem chi tiết user
- `DeleteUser(int id)`: POST xóa user
- `ChangeUserPassword(int id)`: GET/POST đổi mật khẩu
- `ToggleUserVerification(int id)`: POST bật/tắt xác thực
- `ToggleUserStatus(int id, string status)`: POST thay đổi trạng thái

## Bảo mật

### Authorization
- Tất cả actions đều yêu cầu quyền admin
- Sử dụng `[AdminOnly]` attribute
- Kiểm tra quyền truy cập trong controller

### Validation
- Email phải là duy nhất
- Mật khẩu phải đủ mạnh (8+ ký tự, chữ hoa, chữ thường, số)
- Validation client-side và server-side
- CSRF protection

### Logging
- Ghi log tất cả thao tác CRUD
- Theo dõi admin thực hiện thao tác
- Log lỗi và exception

## Giao diện

### Features UI
- Responsive design với Bootstrap 5
- Font Awesome icons
- Modal confirmations
- Real-time validation
- Progress bars cho password strength
- Pagination
- Search và filter

### Components
- Statistics cards
- User table với actions
- Forms với validation
- Modal dialogs
- Breadcrumb navigation

## Cách sử dụng

### 1. Truy cập quản lý user
1. Đăng nhập với tài khoản admin
2. Vào Dashboard
3. Click "Quản lý người dùng"

### 2. Tạo user mới
1. Click "Thêm người dùng"
2. Điền thông tin bắt buộc
3. Chọn vai trò
4. Click "Tạo người dùng"

### 3. Chỉnh sửa user
1. Click icon "Chỉnh sửa" trong danh sách
2. Thay đổi thông tin cần thiết
3. Click "Cập nhật"

### 4. Đổi mật khẩu
1. Click icon "Đổi mật khẩu"
2. Nhập mật khẩu mới
3. Xác nhận mật khẩu
4. Click "Đổi mật khẩu"

### 5. Quản lý trạng thái
- Click icon tương ứng để bật/tắt xác thực
- Click icon để thay đổi trạng thái tài khoản

## Lưu ý quan trọng

1. **Bảo vệ admin**: Không thể xóa tài khoản admin
2. **Email unique**: Email phải là duy nhất trong hệ thống
3. **Password security**: Mật khẩu được hash bằng SHA256
4. **Audit trail**: Tất cả thao tác đều được ghi log
5. **Role management**: Có thể gán nhiều vai trò cho một user

## Troubleshooting

### Lỗi thường gặp
1. **Email đã tồn tại**: Kiểm tra email trong hệ thống
2. **Validation errors**: Kiểm tra thông tin bắt buộc
3. **Permission denied**: Đảm bảo có quyền admin
4. **Database errors**: Kiểm tra kết nối database

### Debug
- Kiểm tra logs trong console
- Xem exception details
- Verify database connections
- Check user permissions

## Tương lai

### Tính năng có thể thêm
1. Bulk operations (import/export users)
2. Advanced filtering
3. User activity logs
4. Email notifications
5. Two-factor authentication
6. User groups management
7. API endpoints
8. Mobile responsive improvements
