using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class TaiKhoanPhanQuyen
{
    public int Id { get; set; }

    public long IdTaiKhoan { get; set; }

    public int IdPhanQuyen { get; set; }

    public virtual PhanQuyen IdPhanQuyenNavigation { get; set; } = null!;

    public virtual TaiKhoan IdTaiKhoanNavigation { get; set; } = null!;
}
