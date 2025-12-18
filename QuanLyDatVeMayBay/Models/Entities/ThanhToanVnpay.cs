using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class ThanhToanVnpay
{
    public long Id { get; set; }

    public string? TransactionNo { get; set; }

    public string? BankCode { get; set; }

    public string? BankTranNo { get; set; }

    public string? CardType { get; set; }

    public int? ResponseCode { get; set; }

    public DateTime? PayDate { get; set; }

    public string? SecureHash { get; set; }

    public virtual MaLoiVnPay? ResponseCodeNavigation { get; set; }
}
