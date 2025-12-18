namespace QuanLyDatVeMayBay.Models.Model
{
    public class ThemPhieuGiamGiaModel
    {
        public long Id { get; set; }
        public string MaGiamGia { get; set; }
        public decimal GiaTriGiam { get; set; }
        public DateOnly NgayKetThuc { get; set; }
        public string NoiDung { get; set; }
        public bool Active { get; set; }
        public int IdLoaiGiamGia { get; set; }
    }
}
