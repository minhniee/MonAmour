using System;
using System.Collections.Generic;

namespace MonAmour.Models;

public partial class PaymentDetail
{
    public int PaymentDetailId { get; set; }

    public int? PaymentId { get; set; }

    public int? OrderId { get; set; }

    public decimal? Amount { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Payment? Payment { get; set; }
}
