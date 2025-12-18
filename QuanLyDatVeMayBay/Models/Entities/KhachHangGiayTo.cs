using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class KhachHangGiayTo
{
    public long Id { get; set; }

    public long IdKhachHang { get; set; }

    public string SoGiayTo { get; set; } = null!;

    public DateOnly? NgayCap { get; set; }

    public DateOnly? NgayHetHan { get; set; }

    public string? NoiCap { get; set; }

    public int? IdLoaiGiayTo { get; set; }

    public int? IdQuocTich { get; set; }

    public string? NoiThuongTru { get; set; }

    public string? QueQuan { get; set; }

    public virtual KhachHang IdKhachHangNavigation { get; set; } = null!;

    public virtual LoaiGiayTo? IdLoaiGiayToNavigation { get; set; }

    public virtual QuocTich? IdQuocTichNavigation { get; set; }
}
