# Hướng dẫn Gallery Hình ảnh Sản phẩm

## Tổng quan
Gallery hình ảnh sản phẩm đã được cải thiện để hiển thị rõ ràng ảnh chính và ảnh phụ của sản phẩm, với giao diện đẹp mắt và thân thiện với người dùng.

## Tính năng chính

### 1. Phân loại hình ảnh
- **Ảnh chính**: Hiển thị nổi bật với viền xanh lá, kích thước lớn hơn
- **Ảnh phụ**: Hiển thị trong grid với viền xanh dương, kích thước nhỏ hơn

### 2. Hiển thị thông tin chi tiết
- Tên hình ảnh
- Alt text (mô tả)
- Loại ảnh (chính/phụ)
- Thứ tự hiển thị
- Số lượng ảnh tổng cộng

### 3. Modal xem hình ảnh
- Xem ảnh kích thước lớn
- Hiển thị đầy đủ thông tin
- Nút tải xuống hình ảnh
- Nút mở trong tab mới

### 4. Thống kê hình ảnh
- Tổng số ảnh
- Số ảnh chính
- Số ảnh phụ
- Thứ tự cao nhất

## Cấu trúc dữ liệu

### ProductDetailViewModel
```csharp
public class ProductDetailViewModel
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public List<ProductImgViewModel> Images { get; set; }
    // ... các thuộc tính khác
}
```

### ProductImgViewModel
```csharp
public class ProductImgViewModel
{
    public int ImgId { get; set; }
    public string ImgUrl { get; set; }
    public string? ImgName { get; set; }
    public string? AltText { get; set; }
    public bool? IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}
```

## Cách sử dụng

### 1. Trong Controller
```csharp
public async Task<IActionResult> ProductDetail(int id)
{
    var product = await _productService.GetProductByIdAsync(id);
    return View(product);
}
```

### 2. Trong View
```html
@model MonAmour.ViewModels.ProductDetailViewModel

<!-- Gallery sẽ tự động hiển thị dựa trên Model.Images -->
<div class="product-image-gallery">
    <!-- Ảnh chính -->
    <!-- Ảnh phụ -->
    <!-- Thống kê -->
</div>
```

## CSS Classes

### Gallery chính
- `.product-image-gallery`: Container chính
- `.primary-image-section`: Phần ảnh chính
- `.secondary-images-section`: Phần ảnh phụ
- `.image-summary`: Phần thống kê

### Cards
- `.primary-image-card`: Card ảnh chính
- `.secondary-image-card`: Card ảnh phụ

### Modal
- `.modal-image`: Hình ảnh trong modal
- `.modal-image-info`: Thông tin hình ảnh

## Responsive Design

Gallery tự động điều chỉnh layout dựa trên kích thước màn hình:
- **Desktop**: Ảnh chính 6 cột, ảnh phụ 4 cột
- **Tablet**: Ảnh chính 6 cột, ảnh phụ 4 cột
- **Mobile**: Ảnh chính 12 cột, ảnh phụ 12 cột

## JavaScript Functions

### openImageModal(imageUrl, imageName, altText, isPrimary, displayOrder)
Mở modal xem hình ảnh với thông tin chi tiết

### downloadImage()
Tải xuống hình ảnh hiện tại

## Tùy chỉnh

### Thay đổi màu sắc
```css
.primary-image-card {
    border-color: #your-color !important;
}

.secondary-image-card {
    border-color: #your-color !important;
}
```

### Thay đổi kích thước
```css
.primary-image-card .card-img-top {
    height: 400px; /* Thay đổi chiều cao ảnh chính */
}

.secondary-image-card .card-img-top {
    height: 250px; /* Thay đổi chiều cao ảnh phụ */
}
```

## Lưu ý

1. **Ảnh chính**: Phải có `IsPrimary = true`
2. **Thứ tự hiển thị**: Sử dụng `DisplayOrder` để sắp xếp
3. **Alt text**: Nên có để tăng tính accessibility
4. **Kích thước ảnh**: Nên đồng nhất để giao diện đẹp

## Troubleshooting

### Vấn đề thường gặp

1. **Ảnh không hiển thị**: Kiểm tra `ImgUrl` có hợp lệ không
2. **Layout bị vỡ**: Kiểm tra CSS có được load đúng không
3. **Modal không hoạt động**: Kiểm tra jQuery và Bootstrap có được load không

### Debug
```javascript
console.log('Images:', @Html.Raw(Json.Serialize(Model.Images)));
console.log('Primary image:', @Html.Raw(Json.Serialize(Model.Images.FirstOrDefault(i => i.IsPrimary == true))));
```

## Kết luận

Gallery hình ảnh mới cung cấp trải nghiệm người dùng tốt hơn với:
- Giao diện đẹp mắt và chuyên nghiệp
- Phân loại rõ ràng ảnh chính/phụ
- Thông tin chi tiết đầy đủ
- Responsive design
- Tính năng xem và tải xuống ảnh
