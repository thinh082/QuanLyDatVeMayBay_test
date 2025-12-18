using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Models.Model;

namespace QuanLyDatVeMayBay.Services.PhieuGiamGiaServices
{
    public interface IPhieuGiamGiaService
    {
        Task<dynamic> LayToanBoPhieuGiamGia(long IdTaiKhoan);
        Task<dynamic> TimKiemMaGiamGia(string maGiamGia);
        Task<dynamic> ApplyVoucher(long idTaiKhoan, long idMaGiamGia);
        Task<dynamic> LayDanhSachChiTietPhieuGiamGia(long idTaiKhoan);
        Task<dynamic> GetDanhSachPhieuGiamGia();
        Task<dynamic> ActivePhieuGiamGia(long idPhieuGiamGia);
        Task<dynamic> CapNhatPhieuGiamGia(ThemPhieuGiamGiaModel model);
    }
    public class PhieuGiamGiaSerivce : IPhieuGiamGiaService
    {
        private readonly ThinhContext _context;
        public PhieuGiamGiaSerivce(ThinhContext context)
        {
            _context = context;
        }
        public async Task<dynamic> LayToanBoPhieuGiamGia(long IdTaiKhoan)
        {
            if (IdTaiKhoan <= 0)
            {
                return new
                {
                    statusCode = 400,
                    message = "Id tài khoản không hợp lệ"
                };
            }
            var phieuGiamGia = (await _context.PhieuGiamGia
            .Where(r => r.Active == true && r.NgayKetThuc >= DateOnly.FromDateTime(DateTime.Today))
            .Select(p => new PhieuGiamGiaModel
            {
                Id = p.Id,
                MaGiamGia = p.MaGiamGia,
                GiaTriGiam = p.GiaTriGiam,
                NgayKetThuc = p.NgayKetThuc,
                NoiDung = p.NoiDung,
                Active = (p.Active ?? true) &&
                         !p.ChiTietPhieuGiamGia.Any(ct => ct.IdTaiKhoan == IdTaiKhoan && ct.IdMaGiamGia == p.Id)
            })
            .ToListAsync());

                    phieuGiamGia = phieuGiamGia
                        .OrderByDescending(p => p.Active)
                        .ThenByDescending(p => p.NgayKetThuc)
                        .ToList();


            return new
            {
                statusCode = 200,
                message = "Lấy phiếu giảm giá thành công",
                data = phieuGiamGia
            };
        }
        public async Task<dynamic> TimKiemMaGiamGia(string maGiamGia)
        {
            if (string.IsNullOrEmpty(maGiamGia))
            {
                return new
                {
                    statusCode = 400,
                    message = "Mã giảm giá không được để trống"
                };
            }
            var phieuGiamGia = await _context.PhieuGiamGia
                .Where(p => p.MaGiamGia.Contains(maGiamGia))
                .Select(p => new PhieuGiamGiaModel
                {
                    Id = p.Id,
                    MaGiamGia = p.MaGiamGia,
                    GiaTriGiam = p.GiaTriGiam,
                    NgayKetThuc = p.NgayKetThuc,
                    NoiDung = p.NoiDung
                }).ToListAsync();
            return new
            {
                statusCode = 200,
                message = "Tìm kiếm mã giảm giá thành công",
                data = phieuGiamGia
            };
        }
        public async Task<dynamic> ApplyVoucher(long idTaiKhoan, long idMaGiamGia)
        {
            if (idTaiKhoan <= 0 || idMaGiamGia <= 0)
                return new { statusCode = 400, message = "Id tài khoản hoặc mã giảm giá không hợp lệ" };

            var phieuGiamGia = await _context.PhieuGiamGia.FindAsync(idMaGiamGia);
            if (phieuGiamGia == null)
                return new { statusCode = 404, message = "Mã giảm giá không tồn tại" };

            if (!(phieuGiamGia.Active ?? false) || phieuGiamGia.NgayKetThuc < DateOnly.FromDateTime(DateTime.Today))
                return new { statusCode = 410, message = "Mã giảm giá đã hết hạn hoặc không còn hiệu lực" };

            var daSuDung = await _context.ChiTietPhieuGiamGia
                .AnyAsync(c => c.IdTaiKhoan == idTaiKhoan && c.IdMaGiamGia == idMaGiamGia);
            if (daSuDung)
                return new { statusCode = 409, message = "Bạn đã sử dụng mã giảm giá này rồi" };

            try
            {
                var chiTietPhieuGiamGia = new ChiTietPhieuGiamGium
                {
                    IdTaiKhoan = idTaiKhoan,
                    IdMaGiamGia = idMaGiamGia,
                    NgaySuDung = DateOnly.FromDateTime(DateTime.Now),
                    Active = false
                };

                _context.ChiTietPhieuGiamGia.Add(chiTietPhieuGiamGia);
                _context.PhieuGiamGia.Update(phieuGiamGia);
                await _context.SaveChangesAsync();

                return new { statusCode = 200, message = "Áp dụng mã giảm giá thành công" };
            }
            catch (Exception ex)
            {
                return new { statusCode = 500, message = "Đã xảy ra lỗi hệ thống" };
            }
        }
        public async Task<dynamic> LayDanhSachChiTietPhieuGiamGia(long idTaiKhoan)
        {
            if (idTaiKhoan <= 0)
                return new { statusCode = 400, message = "Id tài khoản không hợp lệ" };
            var chiTietPhieuGiamGia = await _context.ChiTietPhieuGiamGia
                .Where(c => c.IdTaiKhoan == idTaiKhoan && c.Active == false)
                .Select(c => new
                {
                    c.Id,
                    c.IdMaGiamGia,
                    c.NgaySuDung,
                    MaGiamGia = c.IdMaGiamGiaNavigation.MaGiamGia ?? "",
                    c.IdMaGiamGiaNavigation.GiaTriGiam,
                    c.IdMaGiamGiaNavigation.IdLoaiGiamGia,
                    c.IdMaGiamGiaNavigation.NgayKetThuc,
                    c.IdMaGiamGiaNavigation.NoiDung
                })
                .ToListAsync();
            return new
            {
                statusCode = 200,
                message = "Lấy danh sách chi tiết phiếu giảm giá thành công",
                data2 = chiTietPhieuGiamGia
            };
        }
        public async Task<dynamic> CapNhatPhieuGiamGia(ThemPhieuGiamGiaModel model)
        {
            if (model == null)
            {
                return new { statusCode = 400, message = "Dữ liệu không hợp lệ" };
            }

            try
            {
                var exists = await _context.PhieuGiamGia.FindAsync(model.Id);
                if (exists != null)
                {
                    exists.NoiDung = model.NoiDung;
                    exists.GiaTriGiam = model.GiaTriGiam;
                    exists.NgayKetThuc = model.NgayKetThuc;
                    exists.Active = model.Active;
                    exists.IdLoaiGiamGia = model.IdLoaiGiamGia;
                    _context.PhieuGiamGia.Update(exists);
                }
                else
                {
                    var phieuGiamGia = new PhieuGiamGium
                    {
                        MaGiamGia = model.MaGiamGia,
                        GiaTriGiam = model.GiaTriGiam,
                        NgayKetThuc = model.NgayKetThuc,
                        NoiDung = model.NoiDung,
                        Active = model.Active,
                        IdLoaiGiamGia = model.IdLoaiGiamGia

                    };
                    _context.PhieuGiamGia.Add(phieuGiamGia);
                }
                await _context.SaveChangesAsync();
                return new { statusCode = 200, message = "Thêm phiếu giảm giá thành công" };
            }
            catch (Exception ex)
            {
                return new { statusCode = 500, message = "Đã xảy ra lỗi hệ thống" };
            }
        }
        public async Task<dynamic> GetDanhSachPhieuGiamGia()
        {
            var phieuGiamGia = await _context.PhieuGiamGia
                .Select(p => new
                {
                    p.Id,
                    p.MaGiamGia,
                    p.GiaTriGiam,
                    p.NgayKetThuc,
                    p.NoiDung,
                    p.Active,
                    LoaiGiamGia = p.IdLoaiGiamGiaNavigation.LoaiGiam,
                })
                .ToListAsync();
            return new
            {
                statusCode = 200,
                message = "Lấy danh sách phiếu giảm giá thành công",
                data = phieuGiamGia
            };

        }
        public async Task<dynamic> ActivePhieuGiamGia(long idPhieuGiamGia)
        {
            if (idPhieuGiamGia <= 0)
            {
                return new { statusCode = 400, message = "Id phiếu giảm giá không hợp lệ" };
            }
            var phieuGiamGia = await _context.PhieuGiamGia.FindAsync(idPhieuGiamGia);
            if (phieuGiamGia == null)
            {
                return new { statusCode = 404, message = "Phiếu giảm giá không tồn tại" };
            }
            phieuGiamGia.Active = !(phieuGiamGia.Active ?? false);
            _context.PhieuGiamGia.Update(phieuGiamGia);
            await _context.SaveChangesAsync();
            return new { statusCode = 200, message = "Cập nhật trạng thái phiếu giảm giá thành công" };
        }
    }
}

