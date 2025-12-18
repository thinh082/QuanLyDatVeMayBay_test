namespace QuanLyDatVeMayBay.Models.Model
{
    public class DatVeRedisModel
    {
        public long IdTaiKhoan { get; set; }
        public long IdChuyenBay { get; set; }
        public long IdTuyenBay { get; set; }
        public long IdLichBay { get; set; }
        public List<long> IdGheNgois { get; set; } = new List<long>();
        public long? IdChiTietPhieuGiamGia { get; set; }
        public decimal Gia { get; set; }
        public int SelectedPayment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string MaThanhToanCho { get; set; } = string.Empty;
    }
}
