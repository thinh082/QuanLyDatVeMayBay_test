using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class ChiTietDanhGium
{
    public long Id { get; set; }

    public long IdTaiKhoan { get; set; }

    public int SoSao { get; set; }

    public string? NoiDungDanhGia { get; set; }

    public DateTime? ThoiGianDanhGia { get; set; }

    public virtual TaiKhoan IdTaiKhoanNavigation { get; set; } = null!;
}
