using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class PhieuGiamGium
{
    public long Id { get; set; }

    public string MaGiamGia { get; set; } = null!;

    public decimal GiaTriGiam { get; set; }

    public DateOnly NgayKetThuc { get; set; }

    public string? NoiDung { get; set; }

    public int? IdLoaiGiamGia { get; set; }

    public int? IdDieuKienGiam { get; set; }

    public bool? Active { get; set; }

    public virtual ICollection<ChiTietPhieuGiamGium> ChiTietPhieuGiamGia { get; set; } = new List<ChiTietPhieuGiamGium>();

    public virtual DieuKienGiamGium? IdDieuKienGiamNavigation { get; set; }

    public virtual LoaiPhieuGiamGium? IdLoaiGiamGiaNavigation { get; set; }
}
