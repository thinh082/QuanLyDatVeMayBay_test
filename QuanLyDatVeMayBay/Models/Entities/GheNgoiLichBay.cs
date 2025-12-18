using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class GheNgoiLichBay
{
    public long Id { get; set; }

    public long? IdGheNgoi { get; set; }

    public long? IdLichBay { get; set; }

    public int? TrangThai { get; set; }

    public virtual GheNgoi? IdGheNgoiNavigation { get; set; }

    public virtual LichBay? IdLichBayNavigation { get; set; }
}
