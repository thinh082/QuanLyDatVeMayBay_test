using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class PhanQuyenChucNang
{
    public int Id { get; set; }

    public int IdChucNang { get; set; }

    public bool? QuyenXem { get; set; }

    public bool? QuyenThem { get; set; }

    public bool? QuyenSua { get; set; }

    public bool? QuyenXoa { get; set; }

    public long? IdTaiKhoan { get; set; }

    public virtual ChucNang IdChucNangNavigation { get; set; } = null!;

    public virtual TaiKhoan? IdTaiKhoanNavigation { get; set; }
}
