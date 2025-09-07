# Product Image Upload Only Feature Guide

## Overview
This document describes the implementation of file upload-only functionality for product images, removing URL input and fixing the delete functionality.

## Changes Made

### 1. **Add Image Form - File Upload Only**
**File**: `Views/Admin/ProductImages.cshtml`

**Changes**:
- Removed URL input field
- Made file upload required
- Updated validation to only check for file selection

**Before**:
```html
<div class="form-group">
    <label for="addImageFile">Chọn hình ảnh từ máy tính</label>
    <input type="file" id="addImageFile" name="ImageFile" class="form-control" accept="image/*" onchange="previewImage(this)">
    <!-- URL field was here -->
</div>
```

**After**:
```html
<div class="form-group">
    <label for="addImageFile">Chọn hình ảnh từ máy tính <span class="text-danger">*</span></label>
    <input type="file" id="addImageFile" name="ImageFile" class="form-control" accept="image/*" onchange="previewImage(this)" required>
    <small class="form-text text-muted">Hỗ trợ: JPG, PNG, GIF. Kích thước tối đa: 5MB</small>
    <div id="imagePreview" class="mt-2" style="display: none;">
        <img id="previewImg" src="" alt="Preview" class="img-thumbnail" style="max-width: 200px; max-height: 200px;">
    </div>
</div>
```

### 2. **Edit Image Form - File Upload Only**
**File**: `Views/Admin/ProductImages.cshtml`

**Changes**:
- Added file upload field for updating images
- Removed URL field completely (no reference to current URL)
- Added preview functionality for new file selection
- Controller automatically handles current URL from database

**New Structure**:
```html
<div class="form-group">
    <label for="editImageFile">Chọn hình ảnh mới từ máy tính</label>
    <input type="file" id="editImageFile" name="ImageFile" class="form-control" accept="image/*" onchange="previewEditImage(this)">
    <small class="form-text text-muted">Hỗ trợ: JPG, PNG, GIF. Kích thước tối đa: 5MB. Để trống nếu không muốn thay đổi hình ảnh.</small>
    <div id="editImagePreview" class="mt-2" style="display: none;">
        <img id="editPreviewImg" src="" alt="Preview" class="img-thumbnail" style="max-width: 200px; max-height: 200px;">
    </div>
</div>
```

### 3. **JavaScript Updates**

#### Add Form Validation
**Before**:
```javascript
// Kiểm tra xem có file hoặc URL không
if (!imageFile && !imgUrl) {
    showAlert('danger', 'Vui lòng chọn file hình ảnh hoặc nhập URL!');
    return;
}
```

**After**:
```javascript
// Kiểm tra xem có file không
if (!imageFile) {
    showAlert('danger', 'Vui lòng chọn file hình ảnh!');
    $('#addImageFile').focus();
    return;
}
```

#### Add Form Data Submission
**Before**:
```javascript
if (imageFile) {
    formData.append('ImageFile', imageFile);
} else {
    formData.append('ImgUrl', imgUrl);
}
```

**After**:
```javascript
formData.append('ImageFile', imageFile);
```

#### Edit Form Data Submission
**Before**:
```javascript
const formData = {
    ImgId: $('#editImgId').val(),
    ImgUrl: $('#editImgUrl').val(),
    // ... other fields
};
$.ajax({
    contentType: 'application/json',
    data: JSON.stringify(formData),
```

**After**:
```javascript
const imageFile = $('#editImageFile')[0].files[0];
const formData = new FormData();
formData.append('ImgId', $('#editImgId').val());
formData.append('ProductId', $('#editProductId').val());
formData.append('ImgName', $('#editImgName').val());
formData.append('AltText', $('#editAltText').val());
formData.append('IsPrimary', $('#editIsPrimary').is(':checked'));
formData.append('DisplayOrder', $('#editDisplayOrder').val());
if (imageFile) {
    formData.append('ImageFile', imageFile);
}
$.ajax({
    data: formData,
    processData: false,
    contentType: false,
```

#### Delete Function Fix
**Before**:
```javascript
$.ajax({
    contentType: 'application/json',
    data: JSON.stringify({ imageId: currentImageId }),
```

**After**:
```javascript
$.ajax({
    data: { imageId: currentImageId },
```

### 4. **Controller Updates**

#### UpdateProductImage Action
**File**: `Controllers/AdminController.cs`

**Changes**:
- Added `IFormFile? ImageFile` parameter
- Added file upload processing logic
- Updated validation to handle file uploads

**New Signature**:
```csharp
[HttpPost]
public async Task<IActionResult> UpdateProductImage(IFormFile? ImageFile, ProductImgViewModel model)
```

**File Processing Logic**:
```csharp
// Lấy thông tin hình ảnh hiện tại từ database
var existingImage = await _productService.GetProductImageByIdAsync(model.ImgId);
if (existingImage == null)
{
    return Json(new { success = false, message = "Không tìm thấy hình ảnh!" });
}

// Xử lý file upload nếu có
if (ImageFile != null && ImageFile.Length > 0)
{
    // Kiểm tra kích thước file (5MB)
    if (ImageFile.Length > 5 * 1024 * 1024)
    {
        return Json(new { success = false, message = "File quá lớn. Kích thước tối đa là 5MB." });
    }

    // Kiểm tra loại file
    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
    var fileExtension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
    if (!allowedExtensions.Contains(fileExtension))
    {
        return Json(new { success = false, message = "Chỉ hỗ trợ file JPG, PNG, GIF." });
    }

    // Tạo tên file duy nhất và lưu file
    var fileName = $"{Guid.NewGuid()}{fileExtension}";
    var uploadPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Imagine", "IMGProduct");
    
    if (!Directory.Exists(uploadPath))
    {
        Directory.CreateDirectory(uploadPath);
    }

    var filePath = Path.Combine(uploadPath, fileName);
    
    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await ImageFile.CopyToAsync(stream);
    }

    // Cập nhật URL trong model
    model.ImgUrl = $"/Imagine/IMGProduct/{fileName}";
}
else
{
    // Nếu không có file mới, giữ nguyên URL hiện tại
    model.ImgUrl = existingImage.ImgUrl;
}
```

### 5. **New JavaScript Functions**

#### Preview Edit Image Function
```javascript
function previewEditImage(input) {
    const preview = document.getElementById('editImagePreview');
    const previewImg = document.getElementById('editPreviewImg');
    
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        
        reader.onload = function(e) {
            previewImg.src = e.target.result;
            preview.style.display = 'block';
        };
        
        reader.readAsDataURL(input.files[0]);
    } else {
        preview.style.display = 'none';
    }
}
```

## Features

### ✅ **Add Image**
- File upload only (no URL input)
- Required file selection
- Real-time preview
- File validation (size, type)
- Automatic file naming with GUID

### ✅ **Edit Image**
- Optional file upload for updating
- No URL field in form (controller handles current URL automatically)
- Real-time preview for new file
- Update only metadata if no new file
- Automatic URL management from database

### ✅ **Delete Image**
- Fixed AJAX call format
- Proper modal handling
- Success/error feedback

### ✅ **File Management**
- Files stored in `Imagine/IMGProduct/`
- Unique file names using GUID
- Support for JPG, PNG, GIF
- Maximum file size: 5MB
- Automatic directory creation

## Benefits

1. **Simplified User Experience**: Only file upload, no URL confusion
2. **Better File Management**: All images stored locally with consistent naming
3. **Fixed Delete Functionality**: Proper AJAX calls and error handling
4. **Enhanced Edit Capability**: Can update both file and metadata
5. **Consistent Validation**: Same validation rules for add and edit
6. **Real-time Preview**: Users can see selected images before upload

## Usage

### Adding New Image
1. Select product from dropdown
2. Choose image file from computer
3. Preview appears automatically
4. Fill optional metadata (name, alt text, etc.)
5. Submit form

### Editing Existing Image
1. Click edit button on image
2. Optionally select new image file (preview appears automatically)
3. Update metadata as needed (name, alt text, primary status, display order)
4. Submit form (controller automatically handles URL management)

### Deleting Image
1. Click delete button on image
2. Confirm deletion in modal
3. Image is removed from database and file system

## Technical Notes

- Uses `IWebHostEnvironment` for proper path resolution
- FormData for file uploads instead of JSON
- Bootstrap 5 modal compatibility
- Proper error handling and user feedback
- File validation on both client and server side
