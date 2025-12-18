using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class ThongBao
{
    public long Id { get; set; }

    public long? IdTaiKhoan { get; set; }

    public string? TieuDe { get; set; }

    public string? NoiDung { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? NgayTao { get; set; }

    public string? HinhAnh { get; set; }
}
