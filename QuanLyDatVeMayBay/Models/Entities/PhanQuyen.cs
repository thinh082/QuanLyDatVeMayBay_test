using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class PhanQuyen
{
    public int Id { get; set; }

    public string Tenquyen { get; set; } = null!;

    public virtual ICollection<PhanQuyenChucNang> PhanQuyenChucNangs { get; set; } = new List<PhanQuyenChucNang>();

    public virtual ICollection<TaiKhoanPhanQuyen> TaiKhoanPhanQuyens { get; set; } = new List<TaiKhoanPhanQuyen>();
}
