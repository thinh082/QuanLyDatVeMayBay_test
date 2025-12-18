using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class DieuKienGiamGium
{
    public int Id { get; set; }

    public string? DieuKien { get; set; }

    public virtual ICollection<PhieuGiamGium> PhieuGiamGia { get; set; } = new List<PhieuGiamGium>();
}
