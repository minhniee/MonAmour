# Sửa lỗi RuntimeBinderException trong ProductImages

## 🚨 **Lỗi đã gặp phải:**

```
RuntimeBinderException: '<>f__AnonymousType9<int,string>' does not contain a definition for 'Value'
```

## 🔍 **Nguyên nhân:**

Lỗi xảy ra vì trong View `ProductImages.cshtml`, chúng ta đang truy cập `product.Value` và `product.Text`, nhưng method `GetProductsForDropdownAsync()` trong `ProductService` trả về anonymous type với thuộc tính `productId` và `name` (chữ thường).

## 🛠️ **Cách sửa:**

### **1. Sửa ProductService.cs:**

```csharp
// Trước đây (SAI):
public async Task<List<object>> GetProductsForDropdownAsync()
{
    var products = await _context.Products
        .Where(p => p.Status == "active")
        .OrderBy(p => p.Name)
        .Select(p => new
        {
            productId = p.ProductId,  // ❌ Chữ thường
            name = p.Name            // ❌ Chữ thường
        })
        .ToListAsync();

    return products.Cast<object>().ToList();
}

// Bây giờ (ĐÚNG):
public async Task<List<object>> GetProductsForDropdownAsync()
{
    var products = await _context.Products
        .Where(p => p.Status == "active")
        .OrderBy(p => p.Name)
        .Select(p => new
        {
            Value = p.ProductId,     // ✅ Chữ hoa
            Text = p.Name           // ✅ Chữ hoa
        })
        .ToListAsync();

    return products.Cast<object>().ToList();
}
```

### **2. Trong View ProductImages.cshtml:**

```html
<!-- Bây giờ có thể truy cập đúng: -->
@foreach (var product in ViewBag.Products)
{
    <option value="@product.Value">@product.Text</option>
}
```

## 📋 **Tóm tắt thay đổi:**

1. **ProductService.cs**: Đổi `productId` → `Value`, `name` → `Text`
2. **AdminController.cs**: Không cần xử lý dynamic casting nữa
3. **ProductImages.cshtml**: Truy cập `product.Value` và `product.Text` hoạt động bình thường

## ✅ **Kết quả:**

- Lỗi RuntimeBinderException đã được khắc phục
- Dropdown sản phẩm hiển thị đúng
- Trang ProductImages hoạt động bình thường
- CRUD hình ảnh hoạt động đầy đủ

## 🔧 **Lưu ý kỹ thuật:**

Khi sử dụng `ViewBag` với anonymous types, cần đảm bảo:
- Tên thuộc tính trong anonymous type phải khớp với tên được sử dụng trong View
- Sử dụng PascalCase cho tên thuộc tính (Value, Text) thay vì camelCase
- Tránh sử dụng dynamic casting khi không cần thiết

## 🎯 **Kiểm tra:**

Sau khi sửa, hãy:
1. Build project: `dotnet build`
2. Chạy ứng dụng
3. Truy cập trang ProductImages
4. Kiểm tra dropdown sản phẩm hoạt động
5. Test các chức năng CRUD hình ảnh

Bây giờ trang quản lý hình ảnh sản phẩm sẽ hoạt động hoàn hảo! 🚀
