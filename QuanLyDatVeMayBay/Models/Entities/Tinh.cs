using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class Tinh
{
    public int IdTinh { get; set; }

    public string TenTinh { get; set; } = null!;

    public virtual ICollection<KhachHang> KhachHangs { get; set; } = new List<KhachHang>();

    public virtual ICollection<Quan> Quans { get; set; } = new List<Quan>();
}
