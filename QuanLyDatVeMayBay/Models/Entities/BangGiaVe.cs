using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class BangGiaVe
{
    public long Id { get; set; }

    public long IdLichBay { get; set; }

    public int IdLoaiVe { get; set; }

    public decimal Gia { get; set; }

    public virtual LichBay IdLichBayNavigation { get; set; } = null!;

    public virtual LoaiVe IdLoaiVeNavigation { get; set; } = null!;
}
