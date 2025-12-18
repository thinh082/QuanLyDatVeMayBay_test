using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class ChucNang
{
    public int Id { get; set; }

    public string MaChucNang { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<PhanQuyenChucNang> PhanQuyenChucNangs { get; set; } = new List<PhanQuyenChucNang>();
}
