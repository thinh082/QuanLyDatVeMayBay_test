using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class TienNghi
{
    public int Id { get; set; }

    public string MaTienIch { get; set; } = null!;

    public string TenTienIch { get; set; } = null!;

    public virtual ICollection<TienNghiChuyenMayBay> TienNghiChuyenMayBays { get; set; } = new List<TienNghiChuyenMayBay>();
}
