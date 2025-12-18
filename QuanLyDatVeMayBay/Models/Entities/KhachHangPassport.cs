using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class KhachHangPassport
{
    public long Id { get; set; }

    public long IdKhachHang { get; set; }

    public string SoPassport { get; set; } = null!;

    public string? TenTrenPassport { get; set; }

    public DateOnly? NgayCap { get; set; }

    public DateOnly? NgayHetHan { get; set; }

    public string? NoiCap { get; set; }

    public string? QuocTich { get; set; }

    public string? LoaiPassport { get; set; }

    public string? GhiChu { get; set; }

    public virtual KhachHang IdKhachHangNavigation { get; set; } = null!;
}
