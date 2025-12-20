using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Models.Model;

namespace QuanLyDatVeMayBay.Services.QuanLy
{
    public interface IQuanLyLichSuThanhToanService
    {
        Task<dynamic> GetDanhSachLichSuThanhToan(LocLichSuThanhToanModel? filter = null, int page = 1, int pageSize = 20);
        Task<dynamic> GetChiTietLichSuThanhToan(long id);
        Task<dynamic> ThongKeTheoPhuongThucThanhToan(DateTime? fromDate = null, DateTime? toDate = null);
        Task<dynamic> ThongKeTheoTrangThai(DateTime? fromDate = null, DateTime? toDate = null);
        Task<dynamic> ThongKeTongQuan(DateTime? fromDate = null, DateTime? toDate = null);
        Task<dynamic> ExportExcel(LocLichSuThanhToanModel? filter = null);
    }

    public class QuanLyLichSuThanhToanService : IQuanLyLichSuThanhToanService
    {
        private readonly ThinhContext _context;

        public QuanLyLichSuThanhToanService(ThinhContext context)
        {
            _context = context;
        }

        public async Task<dynamic> GetDanhSachLichSuThanhToan(LocLichSuThanhToanModel? filter = null, int page = 1, int pageSize = 20)
        {
            var query = _context.LichSuThanhToans
                .Include(l => l.IdTaiKhoanNavigation)
                .Include(l => l.IdPhuongThucThanhToanNavigation)
                .Include(l => l.TrangThai)
                .AsQueryable();

            // Áp dụng filter
            if (filter != null)
            {
                if (filter.Id.HasValue && filter.Id.Value > 0)
                    query = query.Where(l => l.Id == filter.Id.Value);

                if (!string.IsNullOrEmpty(filter.MaThanhToan))
                    query = query.Where(l => l.MaThanhToan != null && l.MaThanhToan.Contains(filter.MaThanhToan));

                if (!string.IsNullOrEmpty(filter.Email))
                    query = query.Where(l => l.IdTaiKhoanNavigation.Email != null && 
                                          l.IdTaiKhoanNavigation.Email.Contains(filter.Email));

                if (!string.IsNullOrEmpty(filter.SoDienThoai))
                    query = query.Where(l => l.IdTaiKhoanNavigation.SoDienThoai != null && 
                                          l.IdTaiKhoanNavigation.SoDienThoai.Contains(filter.SoDienThoai));

                if (filter.IdPhuongThucThanhToan.HasValue)
                    query = query.Where(l => l.IdPhuongThucThanhToan == filter.IdPhuongThucThanhToan.Value);

                if (filter.TrangThaiId.HasValue)
                    query = query.Where(l => l.TrangThaiId == filter.TrangThaiId.Value);

                if (!string.IsNullOrEmpty(filter.LoaiDichVu))
                    query = query.Where(l => l.LoaiDichVu != null && l.LoaiDichVu.Contains(filter.LoaiDichVu));

                if (filter.NgayThanhToanFrom.HasValue)
                    query = query.Where(l => l.NgayThanhToan.Date >= filter.NgayThanhToanFrom.Value.Date);

                if (filter.NgayThanhToanTo.HasValue)
                    query = query.Where(l => l.NgayThanhToan.Date <= filter.NgayThanhToanTo.Value.Date);

                if (filter.SoTienMin.HasValue)
                    query = query.Where(l => l.SoTien >= filter.SoTienMin.Value);

                if (filter.SoTienMax.HasValue)
                    query = query.Where(l => l.SoTien <= filter.SoTienMax.Value);
            }

            // Đếm tổng số bản ghi
            var totalCount = await query.CountAsync();

            // Phân trang
            var data = await query
                .OrderByDescending(l => l.NgayThanhToan)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new
                {
                    l.Id,
                    l.MaThanhToan,
                    Email = l.IdTaiKhoanNavigation.Email,
                    SoDienThoai = l.IdTaiKhoanNavigation.SoDienThoai,
                    TenKhachHang = l.IdTaiKhoanNavigation.KhachHang != null 
                        ? l.IdTaiKhoanNavigation.KhachHang.TenKh 
                        : null,
                    l.SoTien,
                    PhuongThucThanhToan = l.IdPhuongThucThanhToanNavigation.TenPhuongThuc,
                    TrangThai = l.TrangThai.TenTrangThai,
                    l.NgayThanhToan,
                    l.LoaiDichVu,
                    IsVnPay = l.VnPayId.HasValue && l.VnPayId.Value,
                    IsPayPal = l.PayPalId.HasValue && l.PayPalId.Value
                })
                .ToListAsync();

            return new
            {
                statusCode = 200,
                message = "Lấy danh sách lịch sử thanh toán thành công",
                data,
                pagination = new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            };
        }

        public async Task<dynamic> GetChiTietLichSuThanhToan(long id)
        {
            if (id <= 0)
                return new { statusCode = 400, message = "Id không hợp lệ" };

            var lichSu = await _context.LichSuThanhToans
                .Include(l => l.IdTaiKhoanNavigation)
                    .ThenInclude(t => t.KhachHang)
                .Include(l => l.IdPhuongThucThanhToanNavigation)
                .Include(l => l.TrangThai)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lichSu == null)
                return new { statusCode = 404, message = "Không tìm thấy lịch sử thanh toán" };

            var result = new
            {
                lichSu.Id,
                lichSu.MaThanhToan,
                TaiKhoan = new
                {
                    lichSu.IdTaiKhoanNavigation.Id,
                    lichSu.IdTaiKhoanNavigation.Email,
                    lichSu.IdTaiKhoanNavigation.SoDienThoai,
                    TenKhachHang = lichSu.IdTaiKhoanNavigation.KhachHang != null
                        ? lichSu.IdTaiKhoanNavigation.KhachHang.TenKh
                        : null
                },
                lichSu.SoTien,
                PhuongThucThanhToan = new
                {
                    lichSu.IdPhuongThucThanhToanNavigation.Id,
                    lichSu.IdPhuongThucThanhToanNavigation.TenPhuongThuc
                },
                TrangThai = new
                {
                    lichSu.TrangThai.Id,
                    lichSu.TrangThai.TenTrangThai
                },
                lichSu.NgayThanhToan,
                lichSu.LoaiDichVu,
                lichSu.VnPayId,
                lichSu.PayPalId
            };

            return new
            {
                statusCode = 200,
                message = "Lấy chi tiết lịch sử thanh toán thành công",
                data = result
            };
        }

        public async Task<dynamic> ThongKeTheoPhuongThucThanhToan(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.LichSuThanhToans
                .Include(l => l.IdPhuongThucThanhToanNavigation)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(l => l.NgayThanhToan.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(l => l.NgayThanhToan.Date <= toDate.Value.Date);

            var thongKe = await query
                .GroupBy(l => new
                {
                    l.IdPhuongThucThanhToan,
                    TenPhuongThuc = l.IdPhuongThucThanhToanNavigation.TenPhuongThuc
                })
                .Select(g => new
                {
                    g.Key.IdPhuongThucThanhToan,
                    g.Key.TenPhuongThuc,
                    SoLuong = g.Count(),
                    TongTien = g.Sum(l => l.SoTien),
                    TrungBinh = g.Average(l => l.SoTien)
                })
                .ToListAsync();

            return new
            {
                statusCode = 200,
                message = "Thống kê theo phương thức thanh toán thành công",
                data = thongKe
            };
        }

        public async Task<dynamic> ThongKeTheoTrangThai(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.LichSuThanhToans
                .Include(l => l.TrangThai)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(l => l.NgayThanhToan.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(l => l.NgayThanhToan.Date <= toDate.Value.Date);

            var thongKe = await query
                .GroupBy(l => new
                {
                    l.TrangThaiId,
                    TenTrangThai = l.TrangThai.TenTrangThai
                })
                .Select(g => new
                {
                    g.Key.TrangThaiId,
                    g.Key.TenTrangThai,
                    SoLuong = g.Count(),
                    TongTien = g.Sum(l => l.SoTien),
                    TrungBinh = g.Average(l => l.SoTien)
                })
                .ToListAsync();

            return new
            {
                statusCode = 200,
                message = "Thống kê theo trạng thái thành công",
                data = thongKe
            };
        }

        public async Task<dynamic> ThongKeTongQuan(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.LichSuThanhToans.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(l => l.NgayThanhToan.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(l => l.NgayThanhToan.Date <= toDate.Value.Date);

            var tongQuan = await query
                .GroupBy(l => 1)
                .Select(g => new
                {
                    TongSoGiaoDich = g.Count(),
                    TongTien = g.Sum(l => l.SoTien),
                    TrungBinhGiaTri = g.Average(l => l.SoTien),
                    GiaoDichThanhCong = g.Count(l => l.TrangThaiId == 1), // Giả sử 1 là thành công
                    GiaoDichThatBai = g.Count(l => l.TrangThaiId != 1)
                })
                .FirstOrDefaultAsync();

            return new
            {
                statusCode = 200,
                message = "Thống kê tổng quan thành công",
                data = tongQuan 
            };
        }

        public async Task<dynamic> ExportExcel(LocLichSuThanhToanModel? filter = null)
        {
            // TODO: Implement export Excel using EPPlus
            // Tương tự như các service khác, bạn có thể sử dụng EPPlus để export
            return new
            {
                statusCode = 501,
                message = "Chức năng export Excel đang được phát triển"
            };
        }
    }
}

