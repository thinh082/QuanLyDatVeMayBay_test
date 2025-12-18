namespace QuanLyDatVeMayBay.Models.Model
{
    public class TimChuyenBayRequest
    {
        public int IdLoaiVe { get; set; }
        public int IdHangBay { get; set; }
        public string MaSanBayDi { get; set; }
        public string MaSanBayDen { get; set; }
        public string NgayDi { get; set; }
        public decimal GiaMin { get; set; }
        public decimal GiaMax { get; set; }
        public string idTienNghi { get; set; }

    }
}
