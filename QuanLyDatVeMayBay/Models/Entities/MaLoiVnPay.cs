using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class MaLoiVnPay
{
    public int Code { get; set; }

    public string? MoTa { get; set; }

    public virtual ICollection<ThanhToanVnpay> ThanhToanVnpays { get; set; } = new List<ThanhToanVnpay>();
}
