namespace QuanLyDatVeMayBay.Models.Model
{
    public class DatVeRequest
    {
        public List<long> IdGheNgois { get; set; } = new List<long>();
        public long IdChuyenBay { get; set; }
        public long IdTuyenBay { get; set; }
        public long IdLichBay { get; set; }
        public long? IdChiTietPhieuGiamGia { get; set; }
        public decimal Gia { get; set; }
        public int SelectedPayment { get; set; }
    }
}
