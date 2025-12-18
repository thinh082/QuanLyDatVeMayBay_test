using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class TaiKhoan
{
    public long Id { get; set; }

    public string Email { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public string? MatKhau { get; set; }

    public int? LoaiTaiKhoanId { get; set; }

    public int? SoLanNhapSaiMatKhau { get; set; }

    public DateTime? ThoiGianKhoaMatKhau { get; set; }

    public bool? IsEmail { get; set; }

    public string? Otp { get; set; }

    public string? HinhAnh { get; set; }

    public virtual ICollection<ChiTietDanhGium> ChiTietDanhGia { get; set; } = new List<ChiTietDanhGium>();

    public virtual ICollection<ChiTietPhieuGiamGium> ChiTietPhieuGiamGia { get; set; } = new List<ChiTietPhieuGiamGium>();

    public virtual ICollection<DatPhong> DatPhongs { get; set; } = new List<DatPhong>();

    public virtual ICollection<DatVe> DatVes { get; set; } = new List<DatVe>();

    public virtual KhachHang? KhachHang { get; set; }

    public virtual ICollection<LichSuThanhToan> LichSuThanhToans { get; set; } = new List<LichSuThanhToan>();

    public virtual LoaiTaiKhoan? LoaiTaiKhoan { get; set; }

    public virtual ICollection<PhanQuyenChucNang> PhanQuyenChucNangs { get; set; } = new List<PhanQuyenChucNang>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
