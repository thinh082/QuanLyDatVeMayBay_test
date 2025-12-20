namespace QuanLyDatVeMayBay.Models.Model
{
        public class LocLichSuThanhToanModel
        {
            public long? Id { get; set; }
            public string? MaThanhToan { get; set; }
            public string? Email { get; set; }
            public string? SoDienThoai { get; set; }
            public int? IdPhuongThucThanhToan { get; set; }
            public int? TrangThaiId { get; set; }
            public string? LoaiDichVu { get; set; }
            public DateTime? NgayThanhToanFrom { get; set; }
            public DateTime? NgayThanhToanTo { get; set; }
            public decimal? SoTienMin { get; set; }
            public decimal? SoTienMax { get; set; }
        }
}

