using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class VeMayBay
{
    public long Id { get; set; }

    public string MaVe { get; set; } = null!;

    public long IdChuyenBay { get; set; }

    public int LoaiVeId { get; set; }

    public decimal Gia { get; set; }

    public virtual ICollection<DatVe> DatVes { get; set; } = new List<DatVe>();

    public virtual ChuyenBay IdChuyenBayNavigation { get; set; } = null!;

    public virtual LoaiVe LoaiVe { get; set; } = null!;
}
