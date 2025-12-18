using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class ChiTietPhieuGiamGium
{
    public int Id { get; set; }

    public long IdTaiKhoan { get; set; }

    public DateOnly NgaySuDung { get; set; }

    public long? IdMaGiamGia { get; set; }

    public bool? Active { get; set; }

    public virtual PhieuGiamGium? IdMaGiamGiaNavigation { get; set; }

    public virtual TaiKhoan IdTaiKhoanNavigation { get; set; } = null!;
}
