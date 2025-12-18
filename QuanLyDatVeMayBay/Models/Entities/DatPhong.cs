using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class DatPhong
{
    public long Id { get; set; }

    public long? IdTaiKhoan { get; set; }

    public long? IdPhong { get; set; }

    public DateTime? NgayDat { get; set; }

    public int? TrangThai { get; set; }

    public string? MaDatPhong { get; set; }

    public virtual Phong? IdPhongNavigation { get; set; }

    public virtual TaiKhoan? IdTaiKhoanNavigation { get; set; }
}
