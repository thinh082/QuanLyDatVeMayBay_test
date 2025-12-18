using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class LichSuThanhToan
{
    public long Id { get; set; }

    public string? MaThanhToan { get; set; }

    public long IdTaiKhoan { get; set; }

    public decimal SoTien { get; set; }

    public int IdPhuongThucThanhToan { get; set; }

    public int TrangThaiId { get; set; }

    public DateTime NgayThanhToan { get; set; }

    public string? LoaiDichVu { get; set; }

    public bool? VnPayId { get; set; }

    public bool? PayPalId { get; set; }

    public virtual LoaiPhuongThucThanhToan IdPhuongThucThanhToanNavigation { get; set; } = null!;

    public virtual TaiKhoan IdTaiKhoanNavigation { get; set; } = null!;

    public virtual TrangThaiThanhToan TrangThai { get; set; } = null!;
}
