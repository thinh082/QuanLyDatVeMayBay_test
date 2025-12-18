namespace QuanLyDatVeMayBay.Models.Model
{
    public class PhieuGiamGiaModel
    {
        public long Id { get; set; }

        public string MaGiamGia { get; set; } = null!;

        public decimal GiaTriGiam { get; set; }

        public DateOnly NgayKetThuc { get; set; }

        public string? NoiDung { get; set; }
        public bool? Active { get; set; }
    }
}
