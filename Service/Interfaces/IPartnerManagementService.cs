using MonAmourDb_BE.DTOs;

namespace MonAmourDb_BE.Service.Interfaces
{
    public interface IPartnerManagementService
    {
        Task<PaginatedResult<PartnerListDto>> GetPartnersAsync(PartnerFilterDto filter);
        Task<PartnerDetailDto?> GetPartnerByIdAsync(int partnerId);
        Task<PartnerDetailDto> CreatePartnerAsync(CreatePartnerDto createPartnerDto);
        Task<PartnerDetailDto?> UpdatePartnerAsync(int partnerId, UpdatePartnerDto updatePartnerDto);
        Task<bool> DeletePartnerAsync(int partnerId);
        Task<bool> TogglePartnerStatusAsync(int partnerId);
        Task<bool> ApprovePartnerAsync(int partnerId);
        Task<bool> SuspendPartnerAsync(int partnerId);
        Task<PartnerStatsDto> GetPartnerStatsAsync();
        Task<List<UserBasicDto>> GetAvailableUsersAsync();
    }
}
