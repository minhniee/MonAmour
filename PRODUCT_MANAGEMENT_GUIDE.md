# Hướng dẫn Quản lý Sản phẩm - MonAmour Admin

## Tổng quan

Hệ thống quản lý sản phẩm trong MonAmour Admin cung cấp đầy đủ các chức năng CRUD (Create, Read, Update, Delete) cho sản phẩm, danh mục sản phẩm và hình ảnh sản phẩm.

## Các tính năng chính

### 1. Quản lý Sản phẩm (Products)
- **Xem danh sách**: Hiển thị tất cả sản phẩm với phân trang và tìm kiếm
- **Tìm kiếm & Lọc**: Theo tên, danh mục, trạng thái, giá
- **Thêm mới**: Tạo sản phẩm mới với đầy đủ thông tin
- **Chỉnh sửa**: Cập nhật thông tin sản phẩm
- **Xem chi tiết**: Hiển thị đầy đủ thông tin và hình ảnh
- **Xóa**: Xóa sản phẩm (có xác nhận)
- **Thay đổi trạng thái**: Kích hoạt/tạm dừng/bản nháp

### 2. Quản lý Danh mục (Categories)
- **Xem danh sách**: Hiển thị tất cả danh mục với số lượng sản phẩm
- **Thêm mới**: Tạo danh mục mới
- **Chỉnh sửa**: Cập nhật tên danh mục
- **Xóa**: Chỉ xóa được danh mục không có sản phẩm

### 3. Quản lý Hình ảnh
- **Thêm hình ảnh**: Upload URL hình ảnh cho sản phẩm
- **Đặt hình chính**: Chọn hình ảnh chính cho sản phẩm
- **Sắp xếp**: Thay đổi thứ tự hiển thị hình ảnh
- **Xóa**: Xóa hình ảnh không cần thiết

## Cách sử dụng

### Truy cập hệ thống

1. **Đăng nhập admin**: Truy cập `/Admin/Login`
2. **Vào quản lý sản phẩm**: Menu `Admin` → `Products`
3. **Vào quản lý danh mục**: Menu `Admin` → `ProductCategories`

### Tạo sản phẩm mới

1. Từ trang danh sách sản phẩm, click `Thêm Sản phẩm`
2. Điền đầy đủ thông tin:
   - **Tên sản phẩm** (bắt buộc)
   - **Danh mục** (bắt buộc)
   - **Mô tả** (tùy chọn)
   - **Giá** (bắt buộc)
   - **Số lượng tồn kho** (bắt buộc)
   - **Chất liệu** (tùy chọn)
   - **Đối tượng khách hàng** (tùy chọn)
   - **Trạng thái** (bắt buộc)
3. Click `Lưu sản phẩm`

### Chỉnh sửa sản phẩm

1. Từ danh sách sản phẩm, click nút `Chỉnh sửa` (biểu tượng bút)
2. Thay đổi thông tin cần thiết
3. Click `Cập nhật sản phẩm`

### Quản lý hình ảnh

1. Từ trang chỉnh sửa sản phẩm, scroll xuống phần `Quản lý Hình ảnh`
2. Click `Thêm hình ảnh`
3. Điền thông tin:
   - **URL hình ảnh** (bắt buộc)
   - **Tên hình ảnh** (tùy chọn)
   - **Alt text** (tùy chọn)
   - **Đặt làm hình ảnh chính** (checkbox)
4. Click `Thêm hình ảnh`

### Quản lý danh mục

1. **Tạo danh mục mới**:
   - Click `Thêm Danh mục`
   - Nhập tên danh mục
   - Click `Lưu danh mục`

2. **Chỉnh sửa danh mục**:
   - Click nút `Chỉnh sửa` (biểu tượng bút)
   - Thay đổi tên danh mục
   - Click `Cập nhật`

3. **Xóa danh mục**:
   - Chỉ xóa được danh mục không có sản phẩm
   - Click nút `Xóa` (biểu tượng thùng rác)
   - Xác nhận xóa

## Cấu trúc dữ liệu

### Product Model
```csharp
public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public int CategoryId { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Material { get; set; }
    public string TargetAudience { get; set; }
    public int StockQuantity { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### ProductCategory Model
```csharp
public class ProductCategory
{
    public int CategoryId { get; set; }
    public string Name { get; set; }
}
```

### ProductImg Model
```csharp
public class ProductImg
{
    public int ImgId { get; set; }
    public int ProductId { get; set; }
    public string ImgUrl { get; set; }
    public string ImgName { get; set; }
    public string AltText { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

## API Endpoints

### Product Management
- `GET /Admin/Products` - Danh sách sản phẩm
- `GET /Admin/CreateProduct` - Form tạo sản phẩm
- `POST /Admin/CreateProduct` - Tạo sản phẩm mới
- `GET /Admin/EditProduct/{id}` - Form chỉnh sửa sản phẩm
- `POST /Admin/EditProduct` - Cập nhật sản phẩm
- `GET /Admin/ProductDetail/{id}` - Chi tiết sản phẩm
- `POST /Admin/DeleteProduct/{id}` - Xóa sản phẩm
- `POST /Admin/ToggleProductStatus` - Thay đổi trạng thái

### Category Management
- `GET /Admin/ProductCategories` - Danh sách danh mục
- `GET /Admin/CreateCategory` - Form tạo danh mục
- `POST /Admin/CreateCategory` - Tạo danh mục mới
- `GET /Admin/EditCategory/{id}` - Form chỉnh sửa danh mục
- `POST /Admin/EditCategory` - Cập nhật danh mục
- `POST /Admin/DeleteCategory/{id}` - Xóa danh mục

### Image Management
- `POST /Admin/AddProductImage` - Thêm hình ảnh
- `POST /Admin/UpdateProductImage` - Cập nhật hình ảnh
- `POST /Admin/DeleteProductImage` - Xóa hình ảnh
- `POST /Admin/SetPrimaryImage` - Đặt hình ảnh chính

## Tính năng nâng cao

### 1. Tìm kiếm và Lọc
- **Tìm kiếm theo từ khóa**: Tìm trong tên và mô tả sản phẩm
- **Lọc theo danh mục**: Chọn danh mục cụ thể
- **Lọc theo trạng thái**: Hoạt động/Không hoạt động
- **Lọc theo giá**: Khoảng giá từ - đến
- **Sắp xếp**: Theo tên, giá, tồn kho, ngày tạo

### 2. Phân trang
- Hiển thị 10 sản phẩm mỗi trang
- Điều hướng trang dễ dàng
- Hiển thị tổng số trang và sản phẩm

### 3. Thống kê
- Tổng số sản phẩm
- Số sản phẩm hoạt động
- Số sản phẩm hết hàng
- Số sản phẩm sắp hết hàng
- Tổng số danh mục

### 4. Validation
- Kiểm tra dữ liệu đầu vào
- Hiển thị thông báo lỗi rõ ràng
- Ngăn chặn dữ liệu không hợp lệ

### 5. Security
- Xác thực admin
- CSRF protection
- Authorization checks

## Lưu ý quan trọng

### 1. Quyền truy cập
- Chỉ admin mới có thể truy cập hệ thống quản lý sản phẩm
- Sử dụng attribute `[AdminOnly]` để bảo vệ

### 2. Xóa dữ liệu
- **Sản phẩm**: Có thể xóa bất kỳ lúc nào
- **Danh mục**: Chỉ xóa được khi không có sản phẩm nào
- **Hình ảnh**: Xóa được tự do, nhưng cần cẩn thận với hình ảnh chính

### 3. Trạng thái sản phẩm
- **active**: Sản phẩm hiển thị cho khách hàng
- **inactive**: Sản phẩm ẩn khỏi khách hàng
- **draft**: Sản phẩm đang soạn thảo

### 4. Hình ảnh sản phẩm
- Mỗi sản phẩm chỉ có 1 hình ảnh chính
- Hình ảnh chính sẽ hiển thị trong danh sách
- Có thể sắp xếp thứ tự hiển thị hình ảnh

## Troubleshooting

### Lỗi thường gặp

1. **Không thể xóa danh mục**
   - Kiểm tra xem danh mục có sản phẩm nào không
   - Chỉ xóa được danh mục trống

2. **Hình ảnh không hiển thị**
   - Kiểm tra URL hình ảnh có hợp lệ không
   - Đảm bảo hình ảnh có thể truy cập từ internet

3. **Lỗi validation**
   - Kiểm tra các trường bắt buộc
   - Đảm bảo định dạng dữ liệu đúng

### Hỗ trợ kỹ thuật

Nếu gặp vấn đề, vui lòng:
1. Kiểm tra log hệ thống
2. Xác nhận quyền admin
3. Kiểm tra kết nối database
4. Liên hệ team phát triển

## Kết luận

Hệ thống quản lý sản phẩm MonAmour Admin cung cấp giao diện thân thiện và đầy đủ tính năng để quản lý sản phẩm một cách hiệu quả. Với các chức năng CRUD đầy đủ, tìm kiếm nâng cao và quản lý hình ảnh, admin có thể dễ dàng quản lý catalog sản phẩm của website.
