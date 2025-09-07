using MonAmour.AuthViewModel;

namespace MonAmour.Services.Interfaces;

public interface IWishListService
{

    Task<UserWishListViewModel> GetUserWishListAsync(int userId);
    Task<WishListViewModel> AddToWishListAsync(AddToWishListViewModel dto);
    Task<bool> RemoveFromWishListAsync(RemoveFromWishListViewModel dto);
    Task<bool> IsInWishListAsync(int userId, int? productId, int? conceptId);
    Task<bool> ClearWishListAsync(int userId);
}
