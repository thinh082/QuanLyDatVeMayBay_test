using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class KhachHangCccd
{
    public long Id { get; set; }

    public long? IdKhachHang { get; set; }

    public string? NoiThuongTru { get; set; }

    public string? QueQuan { get; set; }

    public string? NoiCap { get; set; }

    public DateTime? NgayCap { get; set; }

    public string? SoCccd { get; set; }

    public string? TenTrenCccd { get; set; }

    public virtual KhachHang? IdKhachHangNavigation { get; set; }
}
