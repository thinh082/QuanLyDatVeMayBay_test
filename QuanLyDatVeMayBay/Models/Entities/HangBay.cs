using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class HangBay
{
    public int Id { get; set; }

    public string TenHang { get; set; } = null!;

    public virtual ICollection<ChuyenBay> ChuyenBays { get; set; } = new List<ChuyenBay>();
}
