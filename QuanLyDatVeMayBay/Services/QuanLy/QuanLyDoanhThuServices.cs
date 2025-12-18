using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Models.Entities;
using System.Globalization;

namespace QuanLyDatVeMayBay.Services.QuanLy
{
    public interface IQuanLyDoanhThuServices
    {
        Task<dynamic> DoanhThuTheoNgay(string ngay);
        Task<dynamic> DoanhThuTheoThang(string thang);
        Task<dynamic> DoanhThuTheoNam(string nam);
    }
    public class QuanLyDoanhThuServices:IQuanLyDoanhThuServices
    {
        private ThinhContext _context;
        public QuanLyDoanhThuServices(ThinhContext context)
        {
            _context = context;
        }
        public async Task<dynamic> DoanhThuTheoNgay(string ngay)
        {
            if(string.IsNullOrEmpty(ngay))
            {
                return new
                {
                    statusCode = 400,
                    message = "Ngày không được để trống"
                };
            }
            if(!DateTime.TryParseExact(ngay,"dd-MM-yyyy",CultureInfo.InvariantCulture,DateTimeStyles.None,out var ngayFormat))
            {
                return new
                {
                    statusCode = 400,
                    message = "Ngày không đúng định dạng dd-MM-yyyy"
                };
            }
            var doanhThu = await _context.DatVes
                .Where(dv => dv.NgayDat.Value.Date == ngayFormat.Date)
                .SumAsync(dv => (decimal?)dv.Gia) ?? 0;
            return new
            {
                statusCode = 200,
                message = "Thành công",
                doanhThu
            };
        }
        public async Task<dynamic> DoanhThuTheoThang(string thang)
        {
            if (string.IsNullOrEmpty(thang))
            {
                return new
                {
                    statusCode = 400,
                    message = "Tháng không được để trống"
                };
            }
            if (!DateTime.TryParseExact(thang, "MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var thangFormat))
            {
                return new
                {
                    statusCode = 400,
                    message = "Tháng không đúng định dạng MM-yyyy"
                };
            }
            var doanhThu = await _context.DatVes
                .Where(dv => dv.NgayDat.Value.Month == thangFormat.Month && dv.NgayDat.Value.Year == thangFormat.Year)
                .SumAsync(dv => (decimal?)dv.Gia) ?? 0;
            return new
            {
                statusCode = 200,
                message = "Thành công",
                doanhThu
            };
        }
        public async Task<dynamic> DoanhThuTheoNam(string nam)
        {
            if (string.IsNullOrEmpty(nam))
            {
                return new
                {
                    statusCode = 400,
                    message = "Năm không được để trống"
                };
            }
            if (!int.TryParse(nam, out var namFormat))
            {
                return new
                {
                    statusCode = 400,
                    message = "Năm không đúng định dạng yyyy"
                };
            }
            var doanhThu = await _context.DatVes
                .Where(dv => dv.NgayDat.Value.Year == namFormat)
                .SumAsync(dv => (decimal?)dv.Gia) ?? 0;
            return new
            {
                statusCode = 200,
                message = "Thành công",
                doanhThu
            };
        }
    }
}
