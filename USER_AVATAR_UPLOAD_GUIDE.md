# User Avatar Upload Feature Guide

## Overview
This document describes the implementation of file upload functionality for user avatars in the user management system, replacing URL input with file upload from local machine.

## Changes Made

### 1. **CreateUser.cshtml - File Upload for Avatar**
**File**: `Views/Admin/CreateUser.cshtml`

**Changes**:
- Replaced URL input field with file upload
- Added real-time preview functionality
- Updated form to support multipart/form-data

**Before**:
```html
<div class="form-group mb-3">
    <label asp-for="Avatar" class="form-label">Avatar</label>
    <input asp-for="Avatar" class="form-control" placeholder="URL hình ảnh avatar" />
    <span asp-validation-for="Avatar" class="text-danger"></span>
    <small class="form-text text-muted">Nhập URL hình ảnh hoặc để trống</small>
</div>
```

**After**:
```html
<div class="form-group mb-3">
    <label for="avatarFile" class="form-label">Avatar</label>
    <input type="file" id="avatarFile" name="AvatarFile" class="form-control" accept="image/*" onchange="previewAvatar(this)">
    <span asp-validation-for="Avatar" class="text-danger"></span>
    <small class="form-text text-muted">Hỗ trợ: JPG, PNG, GIF. Kích thước tối đa: 5MB. Để trống nếu không muốn thêm avatar.</small>
    <div id="avatarPreview" class="mt-2" style="display: none;">
        <img id="previewAvatarImg" src="" alt="Avatar Preview" class="img-thumbnail" style="max-width: 150px; max-height: 150px;">
    </div>
</div>
```

### 2. **EditUser.cshtml - File Upload for Avatar**
**File**: `Views/Admin/EditUser.cshtml`

**Changes**:
- Replaced URL input field with file upload
- Added real-time preview functionality
- Added current avatar display
- Updated form to support multipart/form-data

**New Structure**:
```html
<div class="form-group mb-3">
    <label for="avatarFile" class="form-label">Avatar</label>
    <input type="file" id="avatarFile" name="AvatarFile" class="form-control" accept="image/*" onchange="previewAvatar(this)">
    <span asp-validation-for="Avatar" class="text-danger"></span>
    <small class="form-text text-muted">Hỗ trợ: JPG, PNG, GIF. Kích thước tối đa: 5MB. Để trống nếu không muốn thay đổi avatar.</small>
    <div id="avatarPreview" class="mt-2" style="display: none;">
        <img id="previewAvatarImg" src="" alt="Avatar Preview" class="img-thumbnail" style="max-width: 150px; max-height: 150px;">
    </div>
    @if (!string.IsNullOrEmpty(Model.Avatar))
    {
        <div class="mt-2">
            <small class="text-muted">Avatar hiện tại:</small>
            <img src="@Model.Avatar" alt="Current Avatar" class="img-thumbnail ms-2" style="max-width: 100px; max-height: 100px;">
        </div>
    }
</div>
```

### 3. **Form Updates**
Both forms were updated to support file upload:
```html
<form asp-action="CreateUser" method="post" enctype="multipart/form-data">
<!-- and -->
<form asp-action="EditUser" method="post" enctype="multipart/form-data">
```

### 4. **JavaScript Functions**

#### Avatar Preview Function
```javascript
function previewAvatar(input) {
    const preview = document.getElementById('avatarPreview');
    const previewImg = document.getElementById('previewAvatarImg');
    
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

### 5. **Controller Updates**

#### CreateUser Action
**File**: `Controllers/AdminController.cs`

**Changes**:
- Added `IFormFile? AvatarFile` parameter
- Added file upload processing logic
- Added validation for file size and type

**New Signature**:
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateUser(IFormFile? AvatarFile, AdminUserViewModel.UserCreateViewModel model)
```

**File Processing Logic**:
```csharp
// Xử lý file upload avatar nếu có
if (AvatarFile != null && AvatarFile.Length > 0)
{
    // Kiểm tra kích thước file (5MB)
    if (AvatarFile.Length > 5 * 1024 * 1024)
    {
        TempData["Error"] = "File avatar quá lớn. Kích thước tối đa là 5MB.";
        // ... error handling
    }

    // Kiểm tra loại file
    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
    var fileExtension = Path.GetExtension(AvatarFile.FileName).ToLowerInvariant();
    if (!allowedExtensions.Contains(fileExtension))
    {
        TempData["Error"] = "Chỉ hỗ trợ file JPG, PNG, GIF cho avatar.";
        // ... error handling
    }

    // Tạo tên file duy nhất và lưu file
    var fileName = $"{Guid.NewGuid()}{fileExtension}";
    var uploadPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Imagine", "Avatars");
    
    if (!Directory.Exists(uploadPath))
    {
        Directory.CreateDirectory(uploadPath);
    }

    var filePath = Path.Combine(uploadPath, fileName);
    
    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await AvatarFile.CopyToAsync(stream);
    }

    // Cập nhật Avatar URL trong model
    model.Avatar = $"/Imagine/Avatars/{fileName}";
}
```

#### EditUser Action
**File**: `Controllers/AdminController.cs`

**Changes**:
- Added `IFormFile? AvatarFile` parameter
- Added file upload processing logic
- Added logic to preserve existing avatar if no new file uploaded

**New Signature**:
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> EditUser(IFormFile? AvatarFile, AdminUserViewModel.UserEditViewModel model)
```

**File Processing Logic**:
```csharp
// Lấy thông tin user hiện tại để giữ avatar cũ nếu không upload mới
var existingUser = await _userManagementService.GetUserByIdAsync(model.UserId);

// Xử lý file upload avatar nếu có
if (AvatarFile != null && AvatarFile.Length > 0)
{
    // ... same file processing logic as CreateUser
    model.Avatar = $"/Imagine/Avatars/{fileName}";
}
else
{
    // Nếu không có file mới, giữ nguyên avatar hiện tại
    model.Avatar = existingUser.Avatar;
}
```

## Features

### ✅ **Create User**
- File upload for avatar (optional)
- Real-time preview
- File validation (size, type)
- Automatic file naming with GUID
- Files stored in `Imagine/Avatars/`

### ✅ **Edit User**
- Optional file upload for updating avatar
- Keep existing avatar if no new file uploaded
- Real-time preview for new file
- Display current avatar for reference
- Automatic URL management

### ✅ **File Management**
- Files stored in `Imagine/Avatars/`
- Unique file names using GUID
- Support for JPG, PNG, GIF
- Maximum file size: 5MB
- Automatic directory creation

## Benefits

1. **Simplified User Experience**: File upload instead of URL input
2. **Better File Management**: All avatars stored locally with consistent naming
3. **Real-time Preview**: Users can see selected avatar before upload
4. **Consistent with Product Images**: Same upload pattern as product image management
5. **Automatic URL Management**: Controller handles URL generation and storage

## Usage

### Creating New User
1. Fill in user information
2. Optionally choose avatar file from computer
3. Preview appears automatically
4. Submit form

### Editing Existing User
1. Update user information as needed
2. Optionally select new avatar file
3. Preview appears automatically
4. Current avatar is displayed for reference
5. Submit form (controller automatically handles avatar management)

## Technical Notes

- Uses `IWebHostEnvironment` for proper path resolution
- FormData for file uploads
- Proper error handling and user feedback
- File validation on both client and server side
- Automatic directory creation for `Imagine/Avatars/`
- Static file serving configured in `Program.cs`

## File Structure

```
Imagine/
├── Avatars/          # User avatar files
│   ├── {guid}.jpg
│   ├── {guid}.png
│   └── ...
└── IMGProduct/       # Product image files
    ├── {guid}.jpg
    ├── {guid}.png
    └── ...
```

## URL Structure

- Avatar URLs: `/Imagine/Avatars/{filename}`
- Product Image URLs: `/Imagine/IMGProduct/{filename}`

Both are served through the same static file configuration in `Program.cs`.
