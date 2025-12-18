using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class ChiTietDatVe
{
    public long Id { get; set; }

    public long IdDatVe { get; set; }

    public long IdGheNgoi { get; set; }

    public virtual DatVe IdDatVeNavigation { get; set; } = null!;

    public virtual GheNgoi IdGheNgoiNavigation { get; set; } = null!;
}
