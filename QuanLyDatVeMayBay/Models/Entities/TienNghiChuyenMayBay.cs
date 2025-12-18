using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class TienNghiChuyenMayBay
{
    public long Id { get; set; }

    public long IdChuyenBay { get; set; }

    public int IdTienNghi { get; set; }

    public virtual ChuyenBay IdChuyenBayNavigation { get; set; } = null!;

    public virtual TienNghi IdTienNghiNavigation { get; set; } = null!;
}
