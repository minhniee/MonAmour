using Microsoft.EntityFrameworkCore;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using System.Text.Json;

namespace MonAmour.Services.Implements
{
    public class CassoService : ICassoService
    {
        private readonly HttpClient _httpClient;
        private readonly MonAmourDbContext _dbContext;
        private readonly IConfiguration _config;
        private readonly string _apiKey;
        private readonly string _apiBase;

        public CassoService(HttpClient httpClient, MonAmourDbContext dbContext, IConfiguration config)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _config = config;
            _apiKey = _config["Casso:ApiKey"] ?? throw new InvalidOperationException("Casso API key not configured");
            _apiBase = _config["Casso:ApiBase"] ?? "https://oauth.casso.vn";

            _httpClient.BaseAddress = new Uri(_apiBase);

            // Casso API requires "Apikey" format
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Apikey {_apiKey}");

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"CassoService initialized:");
            System.Diagnostics.Debug.WriteLine($"- API Base: {_apiBase}");
            System.Diagnostics.Debug.WriteLine($"- API Key: {_apiKey.Substring(0, Math.Min(20, _apiKey.Length))}...");
            System.Diagnostics.Debug.WriteLine($"- Authorization Header: Apikey {_apiKey.Substring(0, Math.Min(20, _apiKey.Length))}...");
        }

        public async Task<CassoApiResponse<CassoTransactionList>> GetTransactionsAsync(DateTime from, DateTime to, int page = 1, int pageSize = 100)
        {
            try
            {
                var fromStr = from.ToString("yyyy-MM-dd");
                var toStr = to.ToString("yyyy-MM-dd");
                var url = $"/v2/transactions?from={fromStr}&to={toStr}&page={page}&pageSize={pageSize}";

                // Debug logging
                System.Diagnostics.Debug.WriteLine($"Making Casso API request:");
                System.Diagnostics.Debug.WriteLine($"- URL: {_apiBase}{url}");
                System.Diagnostics.Debug.WriteLine($"- Headers: {string.Join(", ", _httpClient.DefaultRequestHeaders.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))}");

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Casso API Error: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"Error Content: {content}");
                    System.Diagnostics.Debug.WriteLine($"Response Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))}");

                    return new CassoApiResponse<CassoTransactionList>
                    {
                        Code = (int)response.StatusCode,
                        Desc = $"API Error: {response.StatusCode} - {content}",
                        Data = null
                    };
                }

                // Log successful response for debugging
                System.Diagnostics.Debug.WriteLine($"Casso API Success: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response Content: {content}");

                try
                {
                    var result = JsonSerializer.Deserialize<CassoApiResponse<CassoTransactionList>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Parsed successfully - Code: {result.Code}, Desc: {result.Desc}");
                        return result;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Deserialization returned null");
                        return new CassoApiResponse<CassoTransactionList>
                        {
                            Code = -1,
                            Desc = "Deserialization returned null",
                            Data = null
                        };
                    }
                }
                catch (JsonException jsonEx)
                {
                    System.Diagnostics.Debug.WriteLine($"JSON Parsing Error: {jsonEx.Message}");
                    return new CassoApiResponse<CassoTransactionList>
                    {
                        Code = -1,
                        Desc = $"JSON Parsing Error: {jsonEx.Message}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new CassoApiResponse<CassoTransactionList>
                {
                    Code = -1,
                    Desc = ex.Message,
                    Data = null
                };
            }
        }


        public async Task<bool> ProcessPaymentAsync(CassoTransaction transaction, int userId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Processing payment for UserID: {userId}, Transaction: {transaction.Id}, Amount: {transaction.Amount}");

                // Find the user's cart order
                var cartOrder = await _dbContext.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "cart");

                if (cartOrder == null)
                {
                    System.Diagnostics.Debug.WriteLine($"No cart order found for UserID: {userId}");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Found cart order: {cartOrder.OrderId}, Total: {cartOrder.TotalPrice}");

                // Check if amount matches (with some tolerance for rounding)
                var expectedAmount = (long)Math.Round((cartOrder.TotalPrice)); // Casso trả về VND, không phải cents
                var amountDifference = Math.Abs(transaction.Amount - expectedAmount);

                System.Diagnostics.Debug.WriteLine($"Amount check: Expected={expectedAmount}, Received={transaction.Amount}, Difference={amountDifference}");

                // Allow 1000 VND difference for rounding
                if (amountDifference > 1000)
                {
                    System.Diagnostics.Debug.WriteLine($"Amount mismatch: Difference {amountDifference} > 1000");
                    return false;
                }

                // Check if transaction description contains user ID (hỗ trợ cả "UserID1" và "1")
                var userIdStr = userId.ToString();
                var hasUserId = transaction.Description?.Contains($"UserID{userIdStr}") == true ||
                               transaction.Description?.Contains(userIdStr) == true ||
                               transaction.Reference?.Contains($"UserID{userIdStr}") == true ||
                               transaction.Reference?.Contains(userIdStr) == true ||
                               transaction.Ref?.Contains($"UserID{userIdStr}") == true ||
                               transaction.Ref?.Contains(userIdStr) == true;

                System.Diagnostics.Debug.WriteLine($"UserID check: Description='{transaction.Description}', Reference='{transaction.Reference}', Ref='{transaction.Ref}', HasUserId={hasUserId}");

                if (!hasUserId)
                {
                    System.Diagnostics.Debug.WriteLine("UserID not found in transaction details");
                    return false;
                }

                // Check if payment already exists for this transaction
                var existingPayment = await _dbContext.Payments
                    .FirstOrDefaultAsync(p => p.TransactionId == transaction.Id);

                if (existingPayment != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Payment already exists for transaction: {transaction.Id}");
                    return true; // Already processed
                }

                // Create payment record
                var payment = new Payment
                {
                    Amount = transaction.Amount, // Casso trả về VND, không cần chia 100
                    Status = "completed",
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow,
                    UserId = userId,
                    PaymentReference = transaction.Reference,
                    TransactionId = transaction.Id,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Payments.Add(payment);
                await _dbContext.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"Created payment: {payment.PaymentId}");

                // Create payment detail
                var paymentDetail = new PaymentDetail
                {
                    PaymentId = payment.PaymentId,
                    OrderId = cartOrder.OrderId,
                    Amount = payment.Amount
                };

                _dbContext.PaymentDetails.Add(paymentDetail);

                // Update order status
                cartOrder.Status = "confirmed";
                cartOrder.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"Payment processed successfully for UserID: {userId}, Order: {cartOrder.OrderId}");
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error processing Casso payment: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

    }
}

