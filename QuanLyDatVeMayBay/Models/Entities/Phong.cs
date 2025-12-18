using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class Phong
{
    public long Id { get; set; }

    public string? TenPhong { get; set; }

    public int? SoGiuong { get; set; }

    public decimal? Gia { get; set; }

    public string? MoTa { get; set; }

    public string? Hinh { get; set; }

    public virtual ICollection<DatPhong> DatPhongs { get; set; } = new List<DatPhong>();
}
