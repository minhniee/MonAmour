# Hướng dẫn Quản lý Hình ảnh Sản phẩm

## 🎯 **Tính năng mới đã được cải tiến**

### ✅ **Vấn đề đã được khắc phục:**
- **Trước đây:** Tất cả hình ảnh của nhiều sản phẩm hiển thị lộn xộn, khó quản lý
- **Bây giờ:** Hình ảnh được hiển thị theo từng sản phẩm cụ thể, dễ quản lý và tìm kiếm

### 🔧 **Tính năng CRUD đầy đủ:**
1. **Create** - Thêm hình ảnh mới
2. **Read** - Xem danh sách hình ảnh theo sản phẩm
3. **Update** - Chỉnh sửa thông tin hình ảnh
4. **Delete** - Xóa hình ảnh

## 📋 **Cách sử dụng**

### 1. **Lọc theo Sản phẩm**
- Sử dụng dropdown "Chọn Sản phẩm" để lọc hình ảnh
- Chọn sản phẩm cụ thể để xem chỉ hình ảnh của sản phẩm đó
- Chọn "-- Tất cả sản phẩm --" để xem tất cả hình ảnh
- Sử dụng nút "Xóa bộ lọc" để reset bộ lọc

### 2. **Thêm Hình ảnh Mới**
- Click nút "Thêm Hình ảnh" (màu xanh)
- Điền thông tin:
  - **Sản phẩm** (bắt buộc): Chọn sản phẩm từ dropdown
  - **URL Hình ảnh** (bắt buộc): Link đến hình ảnh
  - **Tên hình ảnh**: Tên mô tả cho hình ảnh
  - **Alt Text**: Mô tả cho SEO
  - **Thứ tự hiển thị**: Số thứ tự để sắp xếp
  - **Đặt làm ảnh chính**: Checkbox để đặt làm ảnh chính

### 3. **Chỉnh sửa Hình ảnh**
- Hover vào hình ảnh để hiện các nút thao tác
- Click nút **✏️** (màu trắng) để chỉnh sửa
- Cập nhật thông tin trong modal
- Click "Cập nhật" để lưu thay đổi

### 4. **Đặt Ảnh Chính**
- Hover vào hình ảnh phụ
- Click nút **⭐** (màu xanh) để đặt làm ảnh chính
- Xác nhận thao tác

### 5. **Xóa Hình ảnh**
- Hover vào hình ảnh
- Click nút **🗑️** (màu đỏ)
- Xác nhận xóa trong modal

### 6. **Xem Chi tiết Hình ảnh**
- Click vào hình ảnh để xem chi tiết
- Modal hiển thị:
  - Hình ảnh kích thước lớn
  - Thông tin chi tiết (tên, mô tả, loại, thứ tự)
  - Nút "Mở trong tab mới"
  - Nút "Tải xuống"

## 🎨 **Giao diện mới**

### **Card Hình ảnh:**
- **Ảnh chính**: Viền xanh lá, badge "Ảnh chính" với icon ⭐
- **Ảnh phụ**: Viền xanh dương, badge "Ảnh phụ" với icon 🖼️
- **Hover effect**: Hiện overlay với các nút thao tác
- **Responsive**: Tự động điều chỉnh theo kích thước màn hình

### **Thống kê:**
- Hiển thị số lượng ảnh chính và ảnh phụ
- Badge "Đã lọc" khi đang lọc theo sản phẩm

### **Sắp xếp:**
- Ảnh chính hiển thị trước
- Ảnh phụ sắp xếp theo thứ tự hiển thị

## 🔧 **Cấu trúc Kỹ thuật**

### **Controller Actions:**
```csharp
// Lấy hình ảnh theo sản phẩm
public async Task<IActionResult> ProductImages(int? productId = null)

// Thêm hình ảnh mới
[HttpPost] public async Task<IActionResult> AddProductImage([FromBody] ProductImgViewModel model)

// Cập nhật hình ảnh
[HttpPost] public async Task<IActionResult> UpdateProductImage([FromBody] ProductImgViewModel model)

// Đặt ảnh chính
[HttpPost] public async Task<IActionResult> SetPrimaryImage(int productId, int imageId)

// Xóa hình ảnh
[HttpPost] public async Task<IActionResult> DeleteProductImage(int imageId)
```

### **ViewBag Data:**
- `ViewBag.Products`: Danh sách sản phẩm cho dropdown
- `ViewBag.SelectedProductId`: ID sản phẩm đang được lọc

### **JavaScript Functions:**
- `addProductImage()`: Thêm hình ảnh mới
- `editImage(imageId)`: Mở modal chỉnh sửa
- `updateProductImage()`: Cập nhật hình ảnh
- `setPrimaryImage(productId, imageId)`: Đặt ảnh chính
- `deleteImage(imageId)`: Xóa hình ảnh
- `openImageModal()`: Xem chi tiết hình ảnh

## 🚀 **Lợi ích của Tính năng mới**

### **Quản lý hiệu quả:**
- ✅ Hình ảnh được nhóm theo sản phẩm
- ✅ Dễ dàng tìm kiếm và quản lý
- ✅ Giao diện trực quan, dễ sử dụng

### **Tính năng đầy đủ:**
- ✅ CRUD hoàn chỉnh cho hình ảnh
- ✅ Quản lý ảnh chính/ảnh phụ
- ✅ Sắp xếp theo thứ tự hiển thị
- ✅ Preview và download hình ảnh

### **Trải nghiệm người dùng:**
- ✅ Hover effects đẹp mắt
- ✅ Modal responsive
- ✅ Thông báo rõ ràng
- ✅ Xác nhận trước khi xóa

## 📱 **Responsive Design**

- **Desktop**: Hiển thị 3 cột hình ảnh
- **Tablet**: Hiển thị 2 cột hình ảnh  
- **Mobile**: Hiển thị 1 cột hình ảnh
- **Modal**: Tự động điều chỉnh kích thước

## 🔍 **Troubleshooting**

### **Lỗi thường gặp:**
1. **Hình ảnh không hiển thị**: Kiểm tra URL hình ảnh có hợp lệ không
2. **Không thể thêm hình ảnh**: Kiểm tra đã chọn sản phẩm chưa
3. **Lỗi khi cập nhật**: Kiểm tra dữ liệu đầu vào có hợp lệ không

### **Giải pháp:**
- Refresh trang nếu gặp lỗi JavaScript
- Kiểm tra console browser để xem lỗi chi tiết
- Đảm bảo tất cả trường bắt buộc đã được điền

## 🎉 **Kết luận**

Tính năng quản lý hình ảnh sản phẩm đã được cải tiến hoàn toàn:
- **Giao diện đẹp mắt** với hover effects và responsive design
- **Quản lý hiệu quả** theo từng sản phẩm cụ thể
- **CRUD đầy đủ** cho tất cả thao tác cần thiết
- **Trải nghiệm người dùng tốt** với các modal và thông báo rõ ràng

Bây giờ admin có thể quản lý hình ảnh sản phẩm một cách dễ dàng và hiệu quả! 🚀
