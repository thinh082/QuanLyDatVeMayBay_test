namespace QuanLyDatVeMayBay.Models.Model
{
    public class GheNgoiDto
    {
        public long Id { get; set; }
        public long IdChuyenBay { get; set; }
        public int IdLoaiVe { get; set; }
        public string SoGhe { get; set; }
        public int? IdTrangThai { get; set; }
        public decimal? Gia { get; set; }
    }

}
