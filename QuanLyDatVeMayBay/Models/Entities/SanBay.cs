using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class SanBay
{
    public string MaIata { get; set; } = null!;

    public string Ten { get; set; } = null!;

    public string? ThanhPho { get; set; }

    public string? QuocGia { get; set; }

    public string TimeZoneId { get; set; } = null!;

    public string? IanaTimeZoneId { get; set; }

    public virtual ICollection<ChuyenBay> ChuyenBayMaSanBayDenNavigations { get; set; } = new List<ChuyenBay>();

    public virtual ICollection<ChuyenBay> ChuyenBayMaSanBayDiNavigations { get; set; } = new List<ChuyenBay>();
}
