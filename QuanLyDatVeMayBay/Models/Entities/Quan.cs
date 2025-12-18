using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class Quan
{
    public int IdQuan { get; set; }

    public string TenQuan { get; set; } = null!;

    public int IdTinh { get; set; }

    public virtual Tinh IdTinhNavigation { get; set; } = null!;

    public virtual ICollection<KhachHang> KhachHangs { get; set; } = new List<KhachHang>();

    public virtual ICollection<Phuong> Phuongs { get; set; } = new List<Phuong>();
}
