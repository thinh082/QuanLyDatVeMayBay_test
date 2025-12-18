using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class ChuyenBay
{
    public long Id { get; set; }

    public int IdHangBay { get; set; }

    public string MaSanBayDi { get; set; } = null!;

    public string MaSanBayDen { get; set; } = null!;

    public virtual ICollection<DatVe> DatVes { get; set; } = new List<DatVe>();

    public virtual ICollection<GheNgoi> GheNgois { get; set; } = new List<GheNgoi>();

    public virtual HangBay IdHangBayNavigation { get; set; } = null!;

    public virtual ICollection<LichBay> LichBays { get; set; } = new List<LichBay>();

    public virtual SanBay MaSanBayDenNavigation { get; set; } = null!;

    public virtual SanBay MaSanBayDiNavigation { get; set; } = null!;

    public virtual ICollection<TienNghiChuyenMayBay> TienNghiChuyenMayBays { get; set; } = new List<TienNghiChuyenMayBay>();

    public virtual ICollection<VeMayBay> VeMayBays { get; set; } = new List<VeMayBay>();
}
