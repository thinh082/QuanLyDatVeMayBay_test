using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Models.Entities;

namespace QuanLyDatVeMayBay.Services.ThanhToanServices
{
    public interface IThanhToanService
    {
        Task<dynamic> ThanhToan(long idTaiKhoan, decimal gia, int idPhuongThucThanhToan, string LoaiDichVu, int selectId);
        Task<dynamic> GetThanhToan(long idTaiKhoan);
    }
    public class ThanhToanService : IThanhToanService
    {
        private readonly ThinhContext _context;
        public ThanhToanService(ThinhContext thinhContext)
        {
            _context = thinhContext;
        }
        public async Task<dynamic> GetThanhToan(long idTaiKhoan)
        {
            if (idTaiKhoan <= 0)
            {
                return new { statusCode = 500, Message = "ID tài khoản không hợp lệ." };
            }
            var taiKhoan = await _context.TaiKhoans.FindAsync(idTaiKhoan);
            if (taiKhoan == null)
            {
                return new { statusCode = 404, Message = "Tài khoản không tồn tại." };
            }
            var thanhToan = await _context.LichSuThanhToans
                .Where(t => t.IdTaiKhoan == idTaiKhoan)
                .Select(t => new
                {
                    t.Id,
                    t.MaThanhToan,
                    t.SoTien,
                    t.NgayThanhToan,
                    t.LoaiDichVu,
                    PhuongThucThanhToan = t.IdPhuongThucThanhToanNavigation.TenPhuongThuc,
                    TrangThai = t.TrangThai.TenTrangThai,
                    congThanhToan = t.PayPalId == true ? "PayPal" : t.VnPayId == true ? "VnPay" : "Khác"
                })
                .OrderByDescending(r=>r.NgayThanhToan)
                .ToListAsync();
            return new { statusCode = 200, Message = "Lấy lịch sử thanh toán thành công.", Data = thanhToan };
        }
        public async Task<dynamic> GetBillThanhToan(long idTaiKhoan)
        {
            if (idTaiKhoan <= 0)
            {
                return new { statusCode = 500, Message = "ID tài khoản không hợp lệ." };
            }
            var taiKhoan = await _context.TaiKhoans.FindAsync(idTaiKhoan);
            if (taiKhoan == null)
            {
                return new { statusCode = 404, Message = "Tài khoản không tồn tại." };
            }
            var thanhToan = await _context.LichSuThanhToans
                .Where(t => t.IdTaiKhoan == idTaiKhoan)
                .Select(t => new
                {
                    t.Id,
                    t.MaThanhToan,
                    t.SoTien,
                    t.NgayThanhToan,
                    t.LoaiDichVu,
                    PhuongThucThanhToan = t.IdPhuongThucThanhToanNavigation.TenPhuongThuc,
                    TrangThai = t.TrangThai.TenTrangThai,
                    congThanhToan = t.PayPalId == true ? "PayPal" : t.VnPayId == true ? "VnPay" : "Khác"
                })
                .ToListAsync();
            return new { 
                statusCode = 200, 
                Message = "Lấy lịch sử thanh toán thành công.", 
                Data = thanhToan 
            };
        }
        public async Task<dynamic> ThanhToan(long idTaiKhoan,decimal gia,int idPhuongThucThanhToan,string LoaiDichVu,int selectId)
        {
            if (idTaiKhoan <= 0)
            {
                return new { statusCode = 500, Message = "ID tài khoản không hợp lệ." };
            }
            if (gia <= 0)
            {
                return new { statusCode = 500, Message = "Giá tiền không hợp lệ." };
            }
            bool vnpay = false;
            bool paypal = false;
            if (selectId ==1)
            {
                vnpay = true;
            }
            else if(selectId == 2)
            {
                paypal = true;
            }
                var taiKhoan = await _context.TaiKhoans.FindAsync(idTaiKhoan);
            if (taiKhoan == null)
            {
                return new { statusCode = 404, Message = "Tài khoản không tồn tại." };
            }
            // Thực hiện các bước thanh toán ở đây (giả sử thành công)
            var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var thanhToan = new LichSuThanhToan
                {
                    IdTaiKhoan = idTaiKhoan,
                    MaThanhToan = ThinhService.SinhMaNgauNhien("ThanhToanS"),
                    IdPhuongThucThanhToan = idPhuongThucThanhToan,
                    NgayThanhToan = DateTime.Now,
                    SoTien = gia,
                    TrangThaiId = idPhuongThucThanhToan,
                    LoaiDichVu = LoaiDichVu,
                    VnPayId = vnpay ? true : null,
                    PayPalId = paypal ? true : null,

                };
                await _context.LichSuThanhToans.AddAsync(thanhToan);
                 _context.SaveChanges();
                trans.Commit();
                return new { statusCode = 200, Message = "Thanh toán thành công." };
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return new { statusCode = 500, Message = ex.InnerException?.Message ?? ex.Message };
            }
        }
    }
}
