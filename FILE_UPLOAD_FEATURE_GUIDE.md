# File Upload Feature for Product Images

## Overview
This document describes the implementation of file upload functionality for product images, allowing users to upload images from their local machine to the `Imagine/IMGProduct` directory.

## Features Implemented

### 1. **File Upload Support**
- Users can now upload image files directly from their computer
- Supported formats: JPG, JPEG, PNG, GIF
- Maximum file size: 5MB
- Files are stored in the `Imagine/IMGProduct` directory

### 2. **Dual Input Methods**
- **File Upload**: Choose image file from local machine
- **URL Input**: Enter image URL directly (existing functionality)
- At least one method must be provided

### 3. **Image Preview**
- Real-time preview of selected image files
- Preview disappears when modal is closed or form is reset

### 4. **File Validation**
- File size validation (max 5MB)
- File type validation (only image formats)
- Unique filename generation using GUID

## Technical Implementation

### 1. **Frontend Changes (ProductImages.cshtml)**

#### Modal Form Updates
```html
<!-- File upload field -->
<div class="form-group">
    <label for="addImageFile">Chọn hình ảnh từ máy tính</label>
    <input type="file" id="addImageFile" name="ImageFile" class="form-control" 
           accept="image/*" onchange="previewImage(this)">
    <small class="form-text text-muted">Hỗ trợ: JPG, PNG, GIF. Kích thước tối đa: 5MB</small>
    <div id="imagePreview" class="mt-2" style="display: none;">
        <img id="previewImg" src="" alt="Preview" class="img-thumbnail" 
             style="max-width: 200px; max-height: 200px;">
    </div>
</div>

<!-- URL field (optional) -->
<div class="form-group">
    <label for="addImgUrl">Hoặc URL Hình ảnh</label>
    <input type="url" id="addImgUrl" name="ImgUrl" class="form-control" 
           placeholder="https://example.com/image.jpg">
    <small class="form-text text-muted">Bạn phải chọn file hoặc nhập URL (ít nhất một trong hai)</small>
</div>
```

#### Form Configuration
```html
<form id="addImageForm" enctype="multipart/form-data">
```

#### JavaScript Updates
```javascript
// File upload handling
function submitAddImageForm() {
    const imageFile = $('#addImageFile')[0].files[0];
    const imgUrl = $('#addImgUrl').val();
    
    // Validation: require either file or URL
    if (!imageFile && !imgUrl) {
        showAlert('danger', 'Vui lòng chọn file hình ảnh hoặc nhập URL!');
        return;
    }
    
    // Create FormData for file upload
    const formData = new FormData();
    formData.append('ProductId', $('#addProductId').val());
    formData.append('ImgName', $('#addImgName').val());
    formData.append('AltText', $('#addAltText').val());
    formData.append('IsPrimary', $('#addIsPrimary').is(':checked'));
    formData.append('DisplayOrder', $('#addDisplayOrder').val());
    
    if (imageFile) {
        formData.append('ImageFile', imageFile);
    } else {
        formData.append('ImgUrl', imgUrl);
    }
    
    // AJAX call with FormData
    $.ajax({
        url: '@Url.Action("AddProductImage", "Admin")',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        // ... success/error handling
    });
}

// Image preview functionality
function previewImage(input) {
    const preview = document.getElementById('imagePreview');
    const previewImg = document.getElementById('previewImg');
    
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

### 2. **Backend Changes (AdminController.cs)**

#### Action Method Update
```csharp
[HttpPost]
public async Task<IActionResult> AddProductImage(IFormFile? ImageFile, ProductImgViewModel model)
{
    try
    {
        // Validation
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
        }

        // Check 3-image limit
        var canAddMore = await _productService.CanProductAddMoreImagesAsync(model.ProductId);
        if (!canAddMore)
        {
            return Json(new { success = false, message = "Sản phẩm này đã đạt giới hạn 3 hình ảnh." });
        }

        // Handle file upload
        if (ImageFile != null && ImageFile.Length > 0)
        {
            // File size validation (5MB)
            if (ImageFile.Length > 5 * 1024 * 1024)
            {
                return Json(new { success = false, message = "File quá lớn. Kích thước tối đa là 5MB." });
            }

            // File type validation
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return Json(new { success = false, message = "Chỉ hỗ trợ file JPG, PNG, GIF." });
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Imagine", "IMGProduct");
            
            // Create directory if not exists
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePath = Path.Combine(uploadPath, fileName);
            
            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ImageFile.CopyToAsync(stream);
            }

            // Update model with file path
            model.ImgUrl = $"/Imagine/IMGProduct/{fileName}";
        }
        else if (string.IsNullOrEmpty(model.ImgUrl))
        {
            return Json(new { success = false, message = "Vui lòng chọn file hình ảnh hoặc nhập URL!" });
        }

        // Save to database
        var result = await _productService.AddProductImageAsync(model);
        return Json(new { success = result, message = result ? "Thêm hình ảnh thành công" : "Có lỗi xảy ra" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in AddProductImage action");
        return Json(new { success = false, message = "Có lỗi xảy ra khi thêm hình ảnh" });
    }
}
```

### 3. **Static File Configuration (Program.cs)**

#### Static File Middleware
```csharp
// Configure static files for Imagine directory
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Imagine")),
    RequestPath = "/Imagine"
});
```

#### Required Using Statement
```csharp
using Microsoft.Extensions.FileProviders;
```

## File Storage Structure

```
MonAmour/
├── Imagine/
│   ├── Avatar/           # User avatar images
│   ├── IMGConcept/       # Concept images
│   └── IMGProduct/       # Product images (NEW)
│       ├── {guid1}.jpg
│       ├── {guid2}.png
│       └── {guid3}.gif
```

## File Naming Convention

- **Format**: `{GUID}.{extension}`
- **Example**: `550e8400-e29b-41d4-a716-446655440000.jpg`
- **Benefits**: 
  - Unique filenames prevent conflicts
  - GUID ensures no predictable patterns
  - Maintains original file extension

## Security Features

### 1. **File Type Validation**
- Only allows image files (JPG, PNG, GIF)
- Prevents upload of executable files or scripts

### 2. **File Size Limitation**
- Maximum 5MB per file
- Prevents server storage abuse

### 3. **Path Traversal Protection**
- Files are saved to specific directory only
- Uses `Path.Combine` for safe path construction

### 4. **Unique Filename Generation**
- GUID-based naming prevents filename conflicts
- No user-controlled filename input

## User Experience Improvements

### 1. **Visual Feedback**
- Real-time image preview
- Clear validation messages
- Loading states during upload

### 2. **Flexible Input**
- Can use file upload OR URL input
- Both methods supported simultaneously
- Clear instructions for users

### 3. **Error Handling**
- Specific error messages for different issues
- File size and type validation feedback
- Network error handling

## Usage Instructions

### For Users

1. **Upload File Method**:
   - Click "Chọn hình ảnh từ máy tính"
   - Select image file (JPG, PNG, GIF, max 5MB)
   - Preview will appear automatically
   - Fill in other details (name, alt text, etc.)

2. **URL Method**:
   - Enter image URL in "Hoặc URL Hình ảnh" field
   - Fill in other details
   - No file selection needed

3. **Combined Method**:
   - Can provide both file and URL
   - File takes precedence if both are provided

### For Developers

1. **Adding New File Types**:
   - Update `allowedExtensions` array in controller
   - Update frontend `accept` attribute
   - Update validation messages

2. **Changing File Size Limit**:
   - Update size check in controller (currently 5MB)
   - Update frontend validation message
   - Consider server configuration limits

3. **Changing Storage Location**:
   - Update `uploadPath` in controller
   - Update static file configuration in Program.cs
   - Ensure directory permissions

## Testing

### Test Cases

1. **Valid File Upload**:
   - Upload JPG/PNG/GIF file under 5MB
   - Verify file saved to `Imagine/IMGProduct`
   - Verify database record created
   - Verify image displays correctly

2. **File Validation**:
   - Try uploading file over 5MB
   - Try uploading non-image file
   - Verify appropriate error messages

3. **URL Input**:
   - Enter valid image URL
   - Verify database record created
   - Verify image displays correctly

4. **Mixed Input**:
   - Provide both file and URL
   - Verify file takes precedence
   - Verify URL is ignored

## Troubleshooting

### Common Issues

1. **File Not Uploading**:
   - Check file size (must be under 5MB)
   - Check file type (must be JPG, PNG, or GIF)
   - Check directory permissions for `Imagine/IMGProduct`

2. **Image Not Displaying**:
   - Verify static file configuration in Program.cs
   - Check file path in database
   - Verify file exists in `Imagine/IMGProduct` directory

3. **Permission Errors**:
   - Ensure `Imagine/IMGProduct` directory is writable
   - Check application pool permissions
   - Verify file system access rights

## Future Enhancements

### Potential Improvements

1. **Image Processing**:
   - Automatic image resizing
   - Thumbnail generation
   - Image compression

2. **Multiple File Upload**:
   - Drag and drop support
   - Batch upload functionality
   - Progress indicators

3. **Cloud Storage**:
   - Azure Blob Storage integration
   - CDN support
   - Backup and redundancy

4. **Advanced Validation**:
   - Image dimension validation
   - EXIF data extraction
   - Malware scanning

## Conclusion

The file upload feature provides a modern, user-friendly way to add product images while maintaining security and performance. The implementation supports both local file uploads and URL inputs, giving users flexibility in how they add images to their products.

The feature is fully integrated with the existing 3-image limit system and maintains all current functionality while adding new capabilities for local file management.
