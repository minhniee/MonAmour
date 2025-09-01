# Hướng dẫn Hiển thị Hình ảnh Nhóm theo Sản phẩm

## 🎯 **Tính năng mới đã được cải tiến**

### ✅ **Vấn đề đã được khắc phục:**
- **Trước đây:** Tất cả hình ảnh của nhiều sản phẩm hiển thị lộn xộn, khó quản lý
- **Bây giờ:** Hình ảnh được hiển thị theo từng sản phẩm cụ thể, mỗi sản phẩm có section riêng

### 🔧 **Cách hoạt động mới:**

#### **1. Hiển thị theo nhóm sản phẩm:**
- Mỗi sản phẩm có một **card header** riêng với tên sản phẩm và ID
- Hình ảnh của sản phẩm được hiển thị trong section riêng biệt
- Không còn bị trộn lẫn giữa các sản phẩm

#### **2. Cấu trúc dữ liệu mới:**
```csharp
// Trước đây: List<ProductImgViewModel>
// Bây giờ: List<object> với cấu trúc:
new {
    ProductId = p.ProductId,
    ProductName = p.Name,
    Images = List<ProductImgViewModel>
}
```

#### **3. Giao diện mới:**
- **Card header xanh dương** cho mỗi sản phẩm
- **Hình ảnh được sắp xếp** theo thứ tự ảnh chính → ảnh phụ → display order
- **Thống kê tổng hợp** hiển thị số lượng ảnh chính, ảnh phụ và số sản phẩm

## 📋 **Cách sử dụng**

### **1. Xem tất cả hình ảnh (nhóm theo sản phẩm):**
- Truy cập `/Admin/ProductImages` (không có productId)
- Hình ảnh sẽ được nhóm theo từng sản phẩm
- Mỗi sản phẩm có header riêng và danh sách hình ảnh

### **2. Xem hình ảnh của sản phẩm cụ thể:**
- Truy cập `/Admin/ProductImages?productId=1`
- Chỉ hiển thị hình ảnh của sản phẩm có ID = 1
- Vẫn giữ cấu trúc nhóm để nhất quán

### **3. Lọc theo sản phẩm:**
- Sử dụng dropdown "Chọn Sản phẩm" để lọc
- Chọn sản phẩm cụ thể để xem chỉ hình ảnh của sản phẩm đó
- Chọn "-- Tất cả sản phẩm --" để xem tất cả (nhóm theo sản phẩm)

## 🔧 **Thay đổi kỹ thuật**

### **1. AdminController.cs:**
```csharp
// Thay đổi method ProductImages
public async Task<IActionResult> ProductImages(int? productId = null)
{
    if (productId.HasValue)
    {
        // Lấy hình ảnh của sản phẩm cụ thể
        var images = await _productService.GetProductImagesAsync(productId.Value);
        var product = await _productService.GetProductByIdAsync(productId.Value);
        
        var groupedImages = new List<object>
        {
            new {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Images = images
            }
        };
        
        return View(groupedImages);
    }
    else
    {
        // Lấy tất cả sản phẩm có hình ảnh, nhóm theo sản phẩm
        var groupedImages = await _productService.GetProductImagesGroupedByProductAsync();
        return View(groupedImages);
    }
}
```

### **2. ProductService.cs:**
```csharp
// Thêm method mới
public async Task<List<object>> GetProductImagesGroupedByProductAsync()
{
    var groupedImages = await _context.Products
        .Where(p => p.Status == "active" && p.ProductImgs.Any())
        .OrderBy(p => p.Name)
        .Select(p => new
        {
            ProductId = p.ProductId,
            ProductName = p.Name,
            Images = p.ProductImgs
                .OrderBy(pi => pi.IsPrimary == true ? 0 : 1)
                .ThenBy(pi => pi.DisplayOrder)
                .Select(pi => new ProductImgViewModel { ... })
                .ToList()
        })
        .ToListAsync();

    return groupedImages.Cast<object>().ToList();
}
```

### **3. ProductImages.cshtml:**
```html
<!-- Thay đổi Model type -->
@model List<object>

<!-- Hiển thị theo nhóm sản phẩm -->
@foreach (dynamic productGroup in Model)
{
    <div class="product-group mb-5">
        <div class="card border-primary">
            <div class="card-header bg-primary text-white">
                <h5 class="mb-0">
                    <i class="fas fa-box mr-2"></i>
                    <strong>@productGroup.ProductName</strong>
                    <span class="badge badge-light ml-2">ID: @productGroup.ProductId</span>
                </h5>
            </div>
            <div class="card-body">
                <!-- Hiển thị hình ảnh của sản phẩm này -->
                <div class="row">
                    @foreach (var image in (List<ProductImgViewModel>)productGroup.Images)
                    {
                        <!-- Card hình ảnh -->
                    }
                </div>
            </div>
        </div>
    </div>
}
```

## ✅ **Kết quả đạt được**

### **1. Quản lý dễ dàng hơn:**
- Mỗi sản phẩm có section riêng biệt
- Không còn bị trộn lẫn hình ảnh
- Dễ dàng tìm kiếm và quản lý

### **2. Giao diện rõ ràng:**
- Header xanh dương cho mỗi sản phẩm
- Tên sản phẩm và ID hiển thị rõ ràng
- Hình ảnh được sắp xếp theo thứ tự logic

### **3. Tính năng CRUD đầy đủ:**
- Thêm, sửa, xóa hình ảnh
- Đặt ảnh chính
- Tải xuống hình ảnh
- Xem chi tiết hình ảnh

## 🎯 **Kiểm tra tính năng**

Sau khi triển khai, hãy kiểm tra:

1. **Build project:** `dotnet build`
2. **Truy cập trang:** `/Admin/ProductImages`
3. **Kiểm tra hiển thị:** Hình ảnh được nhóm theo sản phẩm
4. **Test lọc:** Sử dụng dropdown để lọc theo sản phẩm
5. **Test CRUD:** Thêm, sửa, xóa hình ảnh

## 🚀 **Lợi ích**

- **Quản lý hiệu quả:** Mỗi sản phẩm có section riêng
- **Giao diện rõ ràng:** Không còn bị trộn lẫn
- **Dễ tìm kiếm:** Nhanh chóng tìm hình ảnh của sản phẩm cụ thể
- **Trải nghiệm người dùng tốt hơn:** Giao diện trực quan, dễ sử dụng

Bây giờ trang quản lý hình ảnh sản phẩm sẽ hiển thị rõ ràng theo từng sản phẩm, giúp admin dễ dàng quản lý và không còn bị trộn lẫn! 🎉
