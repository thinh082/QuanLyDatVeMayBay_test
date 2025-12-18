namespace QuanLyDatVeMayBay.Models.Model
{
    public class CapNhatThongTinModel
    {
        public string Email { get; set; } = null!;

        public string? SoDienThoai { get; set; }
        public string TenKh { get; set; } = null!;

        public string? SoNha { get; set; }
        public int GioiTinh { get; set; }
        public int? IdPhuong { get; set; }

        public int? IdQuan { get; set; }

        public int? IdTinh { get; set; }
        public int? IdQuocTich { get; set; }
        }
}
