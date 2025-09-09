using Microsoft.AspNetCore.Mvc;
using MonAmour.Attributes;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace MonAmour.Controllers
{
    [AdminOnly]
    public class BannerController : Controller
    {
        private readonly IBannerService _bannerService;
        private readonly ILogger<BannerController> _logger;

        public BannerController(IBannerService bannerService, ILogger<BannerController> logger)
        {
            _bannerService = bannerService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Upload()
        {
            ViewBag.BannerTypes = BannerTypeOption.GetOptions();
            // Don't pass model on initial GET request
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(BannerUploadViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.BannerTypes = BannerTypeOption.GetOptions();
                    return View(model);
                }

                if (model.ImageFile == null || model.ImageFile.Length == 0)
                {
                    ModelState.AddModelError("ImageFile", "Vui lòng chọn file ảnh");
                    ViewBag.BannerTypes = BannerTypeOption.GetOptions();
                    return View(model);
                }

                // Process image if resize is enabled
                if (model.EnableResize && model.MaxWidth.HasValue && model.MaxHeight.HasValue)
                {
                    using var image = await Image.LoadAsync(model.ImageFile.OpenReadStream());
                    
                    if (model.MaintainAspectRatio)
                    {
                        // Calculate new dimensions maintaining aspect ratio
                        double ratio = Math.Min((double)model.MaxWidth.Value / image.Width, (double)model.MaxHeight.Value / image.Height);
                        int newWidth = (int)(image.Width * ratio);
                        int newHeight = (int)(image.Height * ratio);
                        
                        image.Mutate(x => x.Resize(newWidth, newHeight));
                    }
                    else
                    {
                        image.Mutate(x => x.Resize(model.MaxWidth.Value, model.MaxHeight.Value));
                    }

                    // Save resized image to memory stream
                    using var ms = new MemoryStream();
                    await image.SaveAsJpegAsync(ms);
                    
                    // Create new IFormFile from memory stream
                    var fileName = Path.GetFileName(model.ImageFile.FileName);
                    model.ImageFile = new FormFile(ms, 0, ms.Length, "ImageFile", fileName);
                }

                bool result = false;
                switch (model.BannerType.ToLower())
                {
                    case "homepage":
                        result = await _bannerService.CreateBannerHomepageAsync(new BannerHomepageCreateViewModel
                        {
                            ImageFile = model.ImageFile,
                            IsPrimary = model.IsPrimary,
                            DisplayOrder = model.DisplayOrder,
                            Description = model.Description,
                            IsActive = model.IsActive
                        });
                        break;

                    case "service":
                        result = await _bannerService.CreateBannerServiceAsync(new BannerServiceCreateViewModel
                        {
                            ImageFile = model.ImageFile,
                            IsPrimary = model.IsPrimary,
                            DisplayOrder = model.DisplayOrder,
                            Description = model.Description,
                            IsActive = model.IsActive
                        });
                        break;

                    case "product":
                        result = await _bannerService.CreateBannerProductAsync(new BannerProductCreateViewModel
                        {
                            ImageFile = model.ImageFile,
                            IsPrimary = model.IsPrimary,
                            DisplayOrder = model.DisplayOrder,
                            Description = model.Description,
                            IsActive = model.IsActive
                        });
                        break;
                }

                if (result)
                {
                    TempData["Success"] = "Upload banner thành công";
                    return RedirectToAction("Upload");
                }
                else
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi upload banner");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading banner");
                ModelState.AddModelError("", "Có lỗi xảy ra khi upload banner");
            }

            ViewBag.BannerTypes = BannerTypeOption.GetOptions();
            return View(model);
        }
    }
}
