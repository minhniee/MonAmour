# Hướng dẫn Sửa lỗi Hiển thị Badge

## Vấn đề đã được khắc phục

### 🔍 **Mô tả vấn đề:**
- Các badge hiển thị tồn kho và trạng thái sản phẩm có font chữ trắng trên nền trắng
- Text không thể đọc được do thiếu contrast
- Một số file sử dụng class `bg-*` thay vì `badge-*` gây không nhất quán

### ✅ **Giải pháp đã áp dụng:**

#### 1. **CSS cho Badge cơ bản**
```css
.badge {
    font-weight: 600;
    text-shadow: 0 1px 2px rgba(0, 0, 0, 0.1);
}
```

#### 2. **CSS cho từng loại Badge**
- **`.badge-success`**: Nền xanh lá, chữ trắng
- **`.badge-danger`**: Nền đỏ, chữ trắng  
- **`.badge-warning`**: Nền vàng, chữ đen
- **`.badge-secondary`**: Nền xám, chữ trắng
- **`.badge-info`**: Nền xanh dương, chữ trắng
- **`.badge-primary`**: Nền xanh dương đậm, chữ trắng

#### 3. **CSS cho Badge lớn (badge-lg)**
```css
.badge-lg.badge-success,
.badge-lg.badge-danger,
.badge-lg.badge-warning,
.badge-lg.badge-secondary,
.badge-lg.badge-info,
.badge-lg.badge-primary {
    font-size: 0.875rem;
    padding: 0.5rem 0.75rem;
    font-weight: 600;
}
```

#### 4. **CSS cho Badge trong Table và Card**
```css
.table .badge {
    font-size: 0.75rem;
    padding: 0.375rem 0.5rem;
}

.card .badge {
    font-size: 0.75rem;
    padding: 0.375rem 0.5rem;
}
```

#### 5. **Hỗ trợ Bootstrap 5 bg-* classes**
```css
.bg-success, .bg-danger, .bg-warning, .bg-secondary, .bg-info, .bg-primary, .bg-pink {
    /* Đảm bảo text hiển thị rõ ràng */
}
```

## Các file đã được cập nhật

### 1. **ProductDetail.cshtml**
- Badge tồn kho: `badge-danger`, `badge-warning`, `badge-success`
- Badge trạng thái: `badge-success`, `badge-secondary`, `badge-warning`
- Badge hình ảnh: `badge-success`, `badge-primary`

### 2. **Products.cshtml**
- Badge tồn kho: `badge-danger`, `badge-warning`, `badge-success`
- Badge trạng thái: `badge-success`, `badge-secondary`

### 3. **EditProduct.cshtml**
- Badge trạng thái: `badge-success`, `badge-secondary`, `badge-warning`

## Màu sắc và Ý nghĩa

### 🟢 **Success (Thành công)**
- **Màu**: Xanh lá (#28a745)
- **Sử dụng**: Hoạt động, có hàng, thành công

### 🔴 **Danger (Nguy hiểm)**
- **Màu**: Đỏ (#dc3545)
- **Sử dụng**: Hết hàng, lỗi, xóa

### 🟡 **Warning (Cảnh báo)**
- **Màu**: Vàng (#ffc107)
- **Sử dụng**: Sắp hết hàng, bản nháp, cảnh báo

### ⚫ **Secondary (Phụ)**
- **Màu**: Xám (#6c757d)
- **Sử dụng**: Không hoạt động, trạng thái phụ

### 🔵 **Info (Thông tin)**
- **Màu**: Xanh dương nhạt (#17a2b8)
- **Sử dụng**: Thông tin, chi tiết

### 🔵 **Primary (Chính)**
- **Màu**: Xanh dương đậm (#007bff)
- **Sử dụng**: Hình ảnh phụ, thông tin chính

## Responsive Design

### 📱 **Mobile**
- Badge tự động điều chỉnh kích thước
- Text vẫn rõ ràng trên mọi thiết bị

### 💻 **Desktop**
- Badge có kích thước chuẩn
- Hover effects cho trải nghiệm tốt hơn

## Cách sử dụng

### 1. **Badge cơ bản**
```html
<span class="badge badge-success">Hoạt động</span>
<span class="badge badge-danger">Hết hàng</span>
<span class="badge badge-warning">Sắp hết</span>
```

### 2. **Badge lớn**
```html
<span class="badge badge-success badge-lg">Hoạt động</span>
<span class="badge badge-danger badge-lg">Hết hàng</span>
```

### 3. **Badge trong Table**
```html
<td>
    <span class="badge badge-success">@product.StockQuantity</span>
</td>
```

### 4. **Badge trong Card**
```html
<div class="card-body">
    <span class="badge badge-primary">@Model.Images.Count</span>
</div>
```

## Lưu ý quan trọng

### ⚠️ **Đảm bảo contrast**
- Badge warning sử dụng chữ đen trên nền vàng
- Các badge khác sử dụng chữ trắng trên nền màu đậm

### 🔧 **Maintenance**
- CSS sử dụng `!important` để override Bootstrap mặc định
- Dễ dàng thay đổi màu sắc trong tương lai

### 📱 **Testing**
- Test trên nhiều thiết bị khác nhau
- Đảm bảo text luôn đọc được

## Kết quả

✅ **Đã khắc phục hoàn toàn** vấn đề font chữ trắng trên nền trắng
✅ **Text hiển thị rõ ràng** trên mọi background
✅ **Giao diện nhất quán** giữa các file
✅ **Responsive design** cho mọi thiết bị
✅ **Hover effects** tăng trải nghiệm người dùng

## Troubleshooting

### Vấn đề thường gặp:

1. **Badge không hiển thị đúng màu**
   - Kiểm tra CSS có được load đúng không
   - Clear browser cache

2. **Text vẫn không đọc được**
   - Kiểm tra class name có đúng không
   - Đảm bảo sử dụng `badge-*` thay vì `bg-*`

3. **Layout bị vỡ**
   - Kiểm tra Bootstrap CSS có được load không
   - Đảm bảo thứ tự load CSS đúng
