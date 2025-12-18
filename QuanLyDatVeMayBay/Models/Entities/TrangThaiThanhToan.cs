using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class TrangThaiThanhToan
{
    public int Id { get; set; }

    public string TenTrangThai { get; set; } = null!;

    public virtual ICollection<LichSuThanhToan> LichSuThanhToans { get; set; } = new List<LichSuThanhToan>();
}
