using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class ThanhToanCho
{
    public long Id { get; set; }

    public string? MaThanhToanCho { get; set; }

    public long IdTaiKhoan { get; set; }

    public decimal SoTien { get; set; }

    public int IdTrangThai { get; set; }

    public int IdLoaiDichVu { get; set; }

    public long IdDichVu { get; set; }

    public int? IdCongThanhToan { get; set; }

    public long? IdChiTietPhieuGiamGia { get; set; }
}
