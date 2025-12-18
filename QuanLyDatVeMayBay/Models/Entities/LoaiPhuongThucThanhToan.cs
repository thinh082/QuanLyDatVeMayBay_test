using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class LoaiPhuongThucThanhToan
{
    public int Id { get; set; }

    public string TenPhuongThuc { get; set; } = null!;

    public virtual ICollection<LichSuThanhToan> LichSuThanhToans { get; set; } = new List<LichSuThanhToan>();
}
