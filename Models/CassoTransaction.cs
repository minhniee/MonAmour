using System;
using System.Collections.Generic;

namespace MonAmour.Models;

public partial class CassoTransaction
{
    public long Id { get; set; }

    public string? Tid { get; set; }

    public string? Description { get; set; }

    public long Amount { get; set; }

    public DateTime When { get; set; }

    public string? BankSubAccId { get; set; }

    public string? SubAccId { get; set; }

    public string? VirtualAccount { get; set; }

    public string? CorrespondingAccount { get; set; }

    public string? CorrespondingAccountName { get; set; }

    public string? CorrespondingBankId { get; set; }

    public string? CorrespondingBankName { get; set; }

    public string? Reference { get; set; }

    public string? Ref { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public int? UserId { get; set; }

    public int? OrderId { get; set; }

    public DateTime CreatedAt { get; set; }
}
