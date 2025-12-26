# Hướng dẫn CRUD Hình ảnh Sản phẩm với Giới hạn 3 Ảnh

## 🎯 **Tính năng mới đã được hoàn thiện**

### ✅ **CRUD đầy đủ cho Hình ảnh:**
1. **Create** - Thêm hình ảnh mới với validation
2. **Read** - Xem danh sách hình ảnh theo sản phẩm
3. **Update** - Chỉnh sửa thông tin hình ảnh
4. **Delete** - Xóa hình ảnh với xác nhận

### 🔒 **Giới hạn 3 hình ảnh cho mỗi sản phẩm:**
- Mỗi sản phẩm chỉ được tối đa 3 hình ảnh
- Validation tự động kiểm tra trước khi thêm
- Hiển thị thông tin số lượng hình ảnh hiện tại
- Nút "Thêm ảnh" chỉ hiện khi còn thể thêm

## 🔧 **Cách hoạt động**

### **1. Kiểm tra giới hạn trước khi thêm:**
```csharp
// ProductService.cs
public async Task<bool> AddProductImageAsync(ProductImgViewModel model)
{
    // Kiểm tra giới hạn 3 hình ảnh cho mỗi sản phẩm
    var existingImageCount = await _context.ProductImgs
        .Where(pi => pi.ProductId == model.ProductId)
        .CountAsync();

    if (existingImageCount >= 3)
    {
        _logger.LogWarning("Product {ProductId} already has maximum 3 images", model.ProductId);
        return false;
    }
    
    // ... tiếp tục thêm hình ảnh
}
```

### **2. Validation trong Controller:**
```csharp
// AdminController.cs
[HttpPost]
public async Task<IActionResult> AddProductImage([FromBody] ProductImgViewModel model)
{
    // Kiểm tra giới hạn 3 hình ảnh
    var canAddMore = await _productService.CanProductAddMoreImagesAsync(model.ProductId);
    if (!canAddMore)
    {
        return Json(new { 
            success = false, 
            message = "Sản phẩm này đã đạt giới hạn 3 hình ảnh. Không thể thêm hình ảnh mới." 
        });
    }
    
    // ... tiếp tục xử lý
}
```

### **3. Kiểm tra real-time:**
```csharp
// Action mới để kiểm tra số lượng hình ảnh
[HttpGet]
public async Task<IActionResult> GetProductImageCount(int productId)
{
    var imageCount = await _productService.GetProductImageCountAsync(productId);
    var canAddMore = await _productService.CanProductAddMoreImagesAsync(productId);
    
    return Json(new { 
        success = true, 
        imageCount = imageCount, 
        canAddMore = canAddMore,
        maxImages = 3,
        message = canAddMore ? $"Có thể thêm {3 - imageCount} hình ảnh nữa" : "Đã đạt giới hạn 3 hình ảnh"
    });
}
```

## 📋 **Giao diện mới**

### **1. Header sản phẩm với thông tin hình ảnh:**
```html
<div class="card-header bg-primary text-white">
    <div class="d-flex justify-content-between align-items-center">
        <h5 class="mb-0">
            <i class="fas fa-box mr-2"></i>
            <strong>@productGroup.ProductName</strong>
            <span class="badge badge-light ml-2">ID: @productGroup.ProductId</span>
        </h5>
        <div class="image-count-info">
            <span class="badge @(canAddMore ? "badge-success" : "badge-warning")">
                <i class="fas fa-image mr-1"></i>
                @imageCount/3 hình ảnh
            </span>
            @if (canAddMore)
            {
                <button type="button" class="btn btn-sm btn-light ml-2" 
                        onclick="addImageForProduct(@productGroup.ProductId, '@productGroup.ProductName')">
                    <i class="fas fa-plus"></i> Thêm ảnh
                </button>
            }
            else
            {
                <span class="badge badge-warning ml-2">
                    <i class="fas fa-exclamation-triangle"></i> Đã đạt giới hạn
                </span>
            }
        </div>
    </div>
</div>
```

### **2. Nút "Thêm ảnh" thông minh:**
- **Hiển thị:** Chỉ khi sản phẩm còn thể thêm hình ảnh (< 3)
- **Vị trí:** Trong header của mỗi sản phẩm
- **Chức năng:** Tự động pre-fill sản phẩm đã chọn

## 🚀 **Tính năng CRUD đầy đủ**

### **1. Create - Thêm hình ảnh:**
- **Validation:** Kiểm tra giới hạn 3 hình ảnh
- **Pre-fill:** Tự động chọn sản phẩm khi click "Thêm ảnh"
- **Form fields:** URL, tên, alt text, ảnh chính, thứ tự hiển thị

### **2. Read - Xem hình ảnh:**
- **Nhóm theo sản phẩm:** Mỗi sản phẩm có section riêng
- **Thông tin chi tiết:** Tên, mô tả, thứ tự, trạng thái ảnh chính
- **Thống kê:** Số lượng ảnh chính, ảnh phụ, tổng sản phẩm

### **3. Update - Chỉnh sửa hình ảnh:**
- **Modal form:** Chỉnh sửa tất cả thông tin
- **Validation:** Kiểm tra dữ liệu trước khi cập nhật
- **Real-time:** Cập nhật ngay lập tức không cần reload

### **4. Delete - Xóa hình ảnh:**
- **Xác nhận:** Modal xác nhận trước khi xóa
- **Auto-primary:** Tự động đặt ảnh khác làm ảnh chính nếu xóa ảnh chính
- **Cập nhật:** Reload trang sau khi xóa thành công

## 🎨 **Cải tiến giao diện**

### **1. Badge thông tin:**
- **Xanh lá:** Còn thể thêm hình ảnh
- **Vàng:** Đã đạt giới hạn 3 hình ảnh
- **Hiển thị:** Số lượng hiện tại / tổng số cho phép

### **2. Nút thao tác:**
- **Hover effects:** Overlay hiện các nút khi hover
- **Icon rõ ràng:** Edit, Star (đặt ảnh chính), Delete
- **Responsive:** Tự động điều chỉnh theo kích thước màn hình

### **3. Modal forms:**
- **Add Image:** Form thêm hình ảnh mới
- **Edit Image:** Form chỉnh sửa hình ảnh
- **Preview Image:** Xem chi tiết và tải xuống
- **Delete Confirm:** Xác nhận xóa

## 🔍 **JavaScript Functions**

### **1. Kiểm tra giới hạn:**
```javascript
function addImageForProduct(productId, productName) {
    // Kiểm tra giới hạn hình ảnh
    $.get('@Url.Action("GetProductImageCount", "Admin")', { productId: productId }, function(result) {
        if (result.success && result.canAddMore) {
            // Pre-fill form với sản phẩm đã chọn
            $('#addProductId').val(productId);
            $('#addProductId').prop('disabled', true);
            $('#addImageModalLabel').html('<i class="fas fa-plus mr-2"></i>Thêm Hình ảnh cho "' + productName + '"');
            $('#addImageModal').modal('show');
        } else {
            showAlert('warning', result.message || 'Sản phẩm này đã đạt giới hạn 3 hình ảnh!');
        }
    });
}
```

### **2. Validation trước khi thêm:**
```javascript
$('#addImageForm').on('submit', function(e) {
    e.preventDefault();
    
    const productId = $('#addProductId').val();
    if (!productId) {
        showAlert('danger', 'Vui lòng chọn sản phẩm!');
        return;
    }
    
    // Kiểm tra giới hạn hình ảnh trước khi thêm
    $.get('@Url.Action("GetProductImageCount", "Admin")', { productId: productId }, function(result) {
        if (result.success && result.canAddMore) {
            submitAddImageForm();
        } else {
            showAlert('danger', result.message || 'Sản phẩm này đã đạt giới hạn 3 hình ảnh!');
        }
    });
});
```

## ✅ **Kết quả đạt được**

### **1. Quản lý hiệu quả:**
- **Giới hạn rõ ràng:** Mỗi sản phẩm tối đa 3 hình ảnh
- **Validation tự động:** Kiểm tra trước khi thêm
- **Thông tin trực quan:** Hiển thị số lượng hình ảnh hiện tại

### **2. CRUD hoàn chỉnh:**
- **Thêm:** Với validation và pre-fill
- **Sửa:** Tất cả thông tin có thể chỉnh sửa
- **Xóa:** Với xác nhận và auto-primary
- **Xem:** Chi tiết và preview

### **3. Giao diện thân thiện:**
- **Responsive design:** Hoạt động tốt trên mọi thiết bị
- **Visual feedback:** Badge màu sắc và icon rõ ràng
- **User experience:** Thao tác đơn giản, trực quan

## 🎯 **Kiểm tra tính năng**

Sau khi triển khai, hãy kiểm tra:

1. **Build project:** `dotnet build`
2. **Truy cập trang:** `/Admin/ProductImages`
3. **Test giới hạn:** Thử thêm hình ảnh cho sản phẩm đã có 3 ảnh
4. **Test CRUD:** Thêm, sửa, xóa hình ảnh
5. **Test validation:** Kiểm tra các trường bắt buộc

## 🚀 **Lợi ích**

- **Quản lý hiệu quả:** Giới hạn rõ ràng, không bị spam hình ảnh
- **Validation tự động:** Ngăn chặn lỗi người dùng
- **CRUD hoàn chỉnh:** Tất cả thao tác cần thiết
- **Giao diện thân thiện:** Dễ sử dụng, trực quan
- **Performance tốt:** Kiểm tra giới hạn trước khi thêm

Bây giờ hệ thống quản lý hình ảnh sản phẩm đã hoàn thiện với CRUD đầy đủ và giới hạn 3 hình ảnh cho mỗi sản phẩm! 🎉
