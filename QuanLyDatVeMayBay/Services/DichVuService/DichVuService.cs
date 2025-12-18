using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Models.Entities;

namespace QuanLyDatVeMayBay.Services.DichVuService
{
    public interface IDichVuService
    {
        Task<dynamic> LayDanhSachDichVu(long idTaiKhoan);
        Task<dynamic> ChiTietDichVu(long idDichVu, int loaiDichVu);
    }
    public class DichVuService : IDichVuService
    {
        private readonly ThinhContext _context;
        public DichVuService(ThinhContext thinhContext)
        {
            _context = thinhContext;
        }
        public async Task<dynamic> LayDanhSachDichVu(long idTaiKhoan)
        {
            try
            {
                // === Vé máy bay ===
                var veMayBay = await _context.DatVes
                    .Where(r => r.IdTaiKhoan == idTaiKhoan)
                    .Select(r => new
                    {
                        LoaiDichVu = "VeMayBay",
                        Id = r.Id,
                        IdLichBay = r.IdLichBay,
                        NgayDat = r.NgayDat,
                        DiemDi = r.IdChuyenBayNavigation.MaSanBayDiNavigation.Ten,
                        DiemDen = r.IdChuyenBayNavigation.MaSanBayDiNavigation.Ten,
                        ThoiGianBatDau = r.LichBay.ThoiGianOsanBayDiUtc,                          
                        ThoiGianKetThuc = r.LichBay.ThoiGianOsanBayDenUtc,
                        TrangThai = r.TrangThai,
                    })
                    .OrderByDescending(r => r.NgayDat)
                    .ToListAsync();

                var hotel = await _context.DatPhongs.Where(r => r.IdTaiKhoan == idTaiKhoan).Select(r => new
                {
                    r.Id,
                    r.NgayDat,
                    r.IdPhongNavigation.Gia,
                    r.IdPhongNavigation.Hinh,
                    r.IdPhongNavigation.TenPhong,
                    r.IdPhongNavigation.SoGiuong,
                    r.IdPhongNavigation.MoTa
                }).OrderByDescending(r => r.NgayDat).ToListAsync();
                return new
                {
                    statusCode = 200,
                    message = "Lấy danh sách dịch vụ thành công",
                    data = new
                    {
                        VeMayBay = veMayBay,
                        Hotel = hotel
                    }
                };
            }catch(Exception ex)
            {
                return new
                {
                    statusCode = 500,
                    message = "Lấy danh sách dịch vụ thất bại: " + ex.Message
                };
            }
        }
        public async Task<dynamic> ChiTietDichVu(long idDichVu,int loaiDichVu)
        {
            if(loaiDichVu == 1)
            {
                var dichVu = await _context.DatVes.Where(r=>r.Id == idDichVu)
                     .Select(r => new
                     {
                         Id = r.Id,
                         NgayDat = r.NgayDat,
                         DiemDi = r.IdChuyenBayNavigation.MaSanBayDiNavigation.Ten,
                         DiemDen = r.IdChuyenBayNavigation.MaSanBayDenNavigation.Ten,
                         MaSanBayDi = r.IdChuyenBayNavigation.MaSanBayDi,
                         MaSanBayDen = r.IdChuyenBayNavigation.MaSanBayDen,
                         ThoiGianBatDau = r.LichBay.ThoiGianOsanBayDiUtc,
                         ThoiGianKetThuc = r.LichBay.ThoiGianOsanBayDenUtc
                     }).FirstOrDefaultAsync();
                return new
                {
                    statusCode = 200,
                    message = "Lấy chi tiết dịch vụ vé máy bay thành công",
                    data = dichVu
                };
            }
            else
            {
                return new
                {
                    statusCode = 200,
                    message = "Chức năng đang được phát triển"
                };
            }
        }

    }
}
