using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class LoaiGiayTo
{
    public int Id { get; set; }

    public string? LoaiGiayTo1 { get; set; }

    public virtual ICollection<KhachHangGiayTo> KhachHangGiayTos { get; set; } = new List<KhachHangGiayTo>();
}
