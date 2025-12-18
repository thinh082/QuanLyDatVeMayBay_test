using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class LogHoatDongNguoiDung
{
    public long Id { get; set; }

    public long? IdTaiKhoan { get; set; }

    public int? IdHoatDong { get; set; }

    public DateTime? CreatedAt { get; set; }
}
