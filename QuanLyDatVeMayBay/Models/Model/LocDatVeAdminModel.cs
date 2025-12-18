namespace QuanLyDatVeMayBay.Models.Model
{
    public class LocDatVeAdminModel
    {
        public long? MaDatVe { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public long? IdChuyenBay { get; set; }
        public long? IdLichBay { get; set; }
        public string? MaSanBayDi { get; set; }
        public string? MaSanBayDen { get; set; }
        public string? TrangThai { get; set; }
        public DateTime? NgayDatFrom { get; set; }
        public DateTime? NgayDatTo { get; set; }
    }
}

