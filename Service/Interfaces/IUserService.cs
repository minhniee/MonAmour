using System.Threading.Tasks;
using MonAmourDb_BE.DTOs;

namespace MonAmourDb_BE.Service.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetProfileAsync(int userId);
        Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    }
}


