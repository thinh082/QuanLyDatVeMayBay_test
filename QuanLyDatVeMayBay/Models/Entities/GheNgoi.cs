using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class GheNgoi
{
    public long Id { get; set; }

    public long IdChuyenBay { get; set; }

    public string SoGhe { get; set; } = null!;

    public int IdLoaiVe { get; set; }

    public bool? Active { get; set; }

    public virtual ICollection<ChiTietDatVe> ChiTietDatVes { get; set; } = new List<ChiTietDatVe>();

    public virtual ICollection<GheNgoiLichBay> GheNgoiLichBays { get; set; } = new List<GheNgoiLichBay>();

    public virtual ChuyenBay IdChuyenBayNavigation { get; set; } = null!;

    public virtual LoaiVe IdLoaiVeNavigation { get; set; } = null!;
}
