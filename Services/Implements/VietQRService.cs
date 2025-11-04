using MonAmour.Services.Interfaces;
using System.Text.Json;
using System.Text;

namespace MonAmour.Services.Implements
{
    public class VietQRService : IVietQRService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public VietQRService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> GenerateQRCodeAsync(decimal amount, string content, string template = "compact")
        {
            try
            {
                var clientId = _config["VietQR:ClientId"];
                var apiKey = _config["VietQR:ApiKey"];
                
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey))
                {
                    // Fallback to URL method if API keys not configured
                    return "";
                }

                var requestData = new
                {
                    accountNo = _config["VietQR:AccountNo"],
                    accountName = _config["VietQR:AccountName"],
                    acqId = _config["VietQR:AcqId"],
                    addInfo = content,
                    amount = (int)(amount * 100), // VietQR expects amount in cents
                    template = template
                };

                var json = JsonSerializer.Serialize(requestData);
                var content_data = new StringContent(json, Encoding.UTF8, "application/json");

                // Add headers for VietQR API
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-client-id", clientId);
                _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

                var response = await _httpClient.PostAsync("/v2/generate", content_data);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"VietQR Response: {responseContent}");
                    var result = JsonSerializer.Deserialize<VietQRResponse>(responseContent);
                    var qrCode = result?.Data?.QrCode ?? "";
                    
                    // Convert base64 to data URL if needed
                    if (!string.IsNullOrEmpty(qrCode) && !qrCode.StartsWith("data:image"))
                    {
                        qrCode = $"data:image/png;base64,{qrCode}";
                    }
                    
                    return qrCode;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"VietQR Error: {response.StatusCode} - {errorContent}");
                }

                return "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VietQR Error: {ex.Message}");
                return "";
            }
        }

        public string GetQRCodeUrl(decimal amount, string content, string template = "compact")
        {
            var baseUrl = _config["VietQR:ApiBase"];
            var accountNo = _config["VietQR:AccountNo"];
            var accountName = _config["VietQR:AccountName"];
            var acqId = _config["VietQR:AcqId"];
            
            // URL encode the content
            var encodedContent = Uri.EscapeDataString(content);
            
            // Build VietQR URL
            var qrUrl = $"{baseUrl}/v2/generate?accountNo={accountNo}&accountName={Uri.EscapeDataString(accountName)}&acqId={acqId}&addInfo={encodedContent}&amount={(int)(amount * 100)}&template={template}";
            
            return qrUrl;
        }
    }

    public class VietQRResponse
    {
        public string Code { get; set; } = "";
        public string Desc { get; set; } = "";
        public VietQRData? Data { get; set; }
    }

    public class VietQRData
    {
        public string QrCode { get; set; } = "";
    }
}
