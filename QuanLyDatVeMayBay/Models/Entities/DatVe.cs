using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class DatVe
{
    public long Id { get; set; }

    public long IdTaiKhoan { get; set; }

    public DateTime? NgayDat { get; set; }

    public long? IdVe { get; set; }

    public long? IdChuyenBay { get; set; }

    public long? IdLichBay { get; set; }

    public long? LichBayId { get; set; }

    public string? TrangThai { get; set; }

    public string? LyDoHuy { get; set; }

    public DateTime? NgayHuy { get; set; }

    public decimal? Gia { get; set; }

    public virtual ICollection<ChiTietDatVe> ChiTietDatVes { get; set; } = new List<ChiTietDatVe>();

    public virtual ChuyenBay? IdChuyenBayNavigation { get; set; }

    public virtual TaiKhoan IdTaiKhoanNavigation { get; set; } = null!;

    public virtual VeMayBay? IdVeNavigation { get; set; }

    public virtual LichBay? LichBay { get; set; }
}
