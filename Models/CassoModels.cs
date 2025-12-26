using System.Text.Json.Serialization;

namespace MonAmour.Models
{
    public class CassoApiResponse<T>
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("desc")]
        public string Desc { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }

    public class CassoTransactionList
    {
        [JsonPropertyName("records")]
        public List<CassoTransaction> Records { get; set; } = new List<CassoTransaction>();

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class CassoWebhookData
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("tid")]
        public long Tid { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("when")]
        public DateTime When { get; set; }

        [JsonPropertyName("bank_sub_acc_id")]
        public string BankSubAccId { get; set; } = string.Empty;

        [JsonPropertyName("sub_acc_id")]
        public string SubAccId { get; set; } = string.Empty;

        [JsonPropertyName("virtual_account")]
        public string VirtualAccount { get; set; } = string.Empty;

        [JsonPropertyName("corresponding_account")]
        public string CorrespondingAccount { get; set; } = string.Empty;

        [JsonPropertyName("corresponding_account_name")]
        public string CorrespondingAccountName { get; set; } = string.Empty;

        [JsonPropertyName("corresponding_bank_id")]
        public string CorrespondingBankId { get; set; } = string.Empty;

        [JsonPropertyName("corresponding_bank_name")]
        public string CorrespondingBankName { get; set; } = string.Empty;

        [JsonPropertyName("reference")]
        public string Reference { get; set; } = string.Empty;

        [JsonPropertyName("ref")]
        public string Ref { get; set; } = string.Empty;
    }

    public class CassoWebhookRequest
    {
        [JsonPropertyName("data")]
        public CassoWebhookData Data { get; set; } = new CassoWebhookData();

        [JsonPropertyName("checksum")]
        public string Checksum { get; set; } = string.Empty;
    }
}
