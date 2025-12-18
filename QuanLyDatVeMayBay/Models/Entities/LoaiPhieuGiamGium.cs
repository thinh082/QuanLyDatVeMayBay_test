using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class LoaiPhieuGiamGium
{
    public int Id { get; set; }

    public string? LoaiGiam { get; set; }

    public virtual ICollection<PhieuGiamGium> PhieuGiamGia { get; set; } = new List<PhieuGiamGium>();
}
