using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class LichBay
{
    public long Id { get; set; }

    public long IdTuyenBay { get; set; }

    public DateTime ThoiGianOsanBayDiUtc { get; set; }

    public DateTime ThoiGianOsanBayDenUtc { get; set; }

    public int? ThoiGianBay { get; set; }

    public virtual ICollection<BangGiaVe> BangGiaVes { get; set; } = new List<BangGiaVe>();

    public virtual ICollection<DatVe> DatVes { get; set; } = new List<DatVe>();

    public virtual ICollection<GheNgoiLichBay> GheNgoiLichBays { get; set; } = new List<GheNgoiLichBay>();

    public virtual ChuyenBay IdTuyenBayNavigation { get; set; } = null!;
}
