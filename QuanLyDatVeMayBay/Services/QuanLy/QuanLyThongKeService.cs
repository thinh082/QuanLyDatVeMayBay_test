using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Models.Entities;

namespace QuanLyDatVeMayBay.Services.QuanLy
{
    public interface IQuanLyThongKeService
    {
        Task<dynamic> GetDashboardKpi();
        Task<dynamic> DoanhThuTheoNgayRange(DateTime? from = null, DateTime? to = null);
        Task<dynamic> VeTheoNgayRange(DateTime? from = null, DateTime? to = null);
    }

    public class QuanLyThongKeService : IQuanLyThongKeService
    {
        private readonly ThinhContext _context;

        public QuanLyThongKeService(ThinhContext context)
        {
            _context = context;
        }

        public async Task<dynamic> GetDashboardKpi()
        {
            var homNay = DateTime.Now.Date;
            var dauThang = new DateTime(homNay.Year, homNay.Month, 1);
            var bayNgayTruoc = homNay.AddDays(-7);

            // Doanh thu hôm nay
            var doanhThuHomNay = await _context.DatVes
                .Where(dv => dv.NgayDat.HasValue && dv.NgayDat.Value.Date == homNay)
                .SumAsync(dv => (decimal?)dv.Gia) ?? 0;

            // Tổng vé đã bán (hôm nay hoặc tháng này - tùy bạn muốn)
            var tongVeDaBan = await _context.DatVes
                .Where(dv => dv.NgayDat.HasValue && dv.NgayDat.Value.Date >= dauThang)
                .CountAsync();

            // Người dùng hoạt động (7 ngày gần nhất)
            var nguoiDungHoatDong = await _context.DatVes
                .Where(dv => dv.NgayDat.HasValue && dv.NgayDat.Value.Date >= bayNgayTruoc)
                .Select(dv => dv.IdTaiKhoan)
                .Distinct()
                .CountAsync();

            // Tỉ lệ hủy hôm nay
            var tongVeHomNay = await _context.DatVes
                .Where(dv => dv.NgayDat.HasValue && dv.NgayDat.Value.Date == homNay)
                .CountAsync();

            var veHuyHomNay = await _context.DatVes
                .Where(dv => dv.NgayDat.HasValue && dv.NgayDat.Value.Date == homNay
                    && (dv.TrangThai != null && (dv.TrangThai.Contains("hủy") || dv.TrangThai.Contains("huy"))))
                .CountAsync();

            var tiLeHuy = tongVeHomNay > 0 ? (double)veHuyHomNay / tongVeHomNay * 100 : 0;

            return new
            {
                statusCode = 200,
                message = "Lấy KPI dashboard thành công",
                data = new
                {
                    doanhThuHomNay,
                    tongVeDaBan,
                    nguoiDungHoatDong,
                    tiLeHuy = Math.Round(tiLeHuy, 2)
                }
            };
        }

        public async Task<dynamic> DoanhThuTheoNgayRange(DateTime? from = null, DateTime? to = null)
        {
            var fromDate = from ?? DateTime.Now.AddDays(-30).Date;
            var toDate = to ?? DateTime.Now.Date;

            var data = await _context.DatVes
                .Where(dv => dv.NgayDat.HasValue 
                    && dv.NgayDat.Value.Date >= fromDate 
                    && dv.NgayDat.Value.Date <= toDate)
                .GroupBy(dv => dv.NgayDat.Value.Date)
                .Select(g => new
                {
                    ngay = g.Key,
                    doanhThu = g.Sum(dv => (decimal?)dv.Gia) ?? 0
                })
                .OrderBy(x => x.ngay)
                .ToListAsync();

            return new
            {
                statusCode = 200,
                message = "Lấy doanh thu theo ngày thành công",
                data
            };
        }

        public async Task<dynamic> VeTheoNgayRange(DateTime? from = null, DateTime? to = null)
        {
            var fromDate = from ?? DateTime.Now.AddDays(-30).Date;
            var toDate = to ?? DateTime.Now.Date;

            var data = await _context.DatVes
                .Where(dv => dv.NgayDat.HasValue 
                    && dv.NgayDat.Value.Date >= fromDate 
                    && dv.NgayDat.Value.Date <= toDate)
                .GroupBy(dv => dv.NgayDat.Value.Date)
                .Select(g => new
                {
                    ngay = g.Key,
                    tongVe = g.Count(),
                    veDaDat = g.Count(dv => dv.TrangThai == null || dv.TrangThai == "đã đặt"),
                    veDaCheckin = g.Count(dv => dv.TrangThai != null && dv.TrangThai.Contains("checkin")),
                    veDaHuy = g.Count(dv => dv.TrangThai != null && (dv.TrangThai.Contains("hủy") || dv.TrangThai.Contains("huy")))
                })
                .OrderBy(x => x.ngay)
                .ToListAsync();

            return new
            {
                statusCode = 200,
                message = "Lấy số vé theo ngày thành công",
                data
            };
        }
    }
}

