using MonAmour.Models;

namespace MonAmour.Services.Interfaces
{
    public interface ICassoService
    {
        Task<CassoApiResponse<CassoTransactionList>> GetTransactionsAsync(DateTime from, DateTime to, int page = 1, int pageSize = 100);
        Task<bool> ProcessPaymentAsync(CassoTransaction transaction, int userId);
    }
}
