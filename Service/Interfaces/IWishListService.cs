using MonAmourDb_BE.DTOs;

namespace MonAmourDb_BE.Service.Interfaces;

public interface IWishListService
{
    Task<ApiResponse<UserWishListDto>> GetUserWishListAsync(int userId);
    Task<ApiResponse<WishListDto>> AddToWishListAsync(AddToWishListDto dto);
    Task<ApiResponse<bool>> RemoveFromWishListAsync(RemoveFromWishListDto dto);
    Task<ApiResponse<bool>> IsInWishListAsync(int userId, int? productId, int? conceptId);
    Task<ApiResponse<bool>> ClearWishListAsync(int userId);
}
