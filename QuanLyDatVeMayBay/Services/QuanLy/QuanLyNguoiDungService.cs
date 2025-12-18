using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Models.Model;

namespace QuanLyDatVeMayBay.Services.QuanLy
{
    public interface IQuanLyNguoiDungService
    {
        Task<dynamic> GetDanhSachNguoiDung(LocNguoiDungModel? filter = null);
        Task<dynamic> GetChiTietNguoiDung(long idTaiKhoan);
        Task<dynamic> CapNhatNguoiDung(CapNhatNguoiDungModel model);
        Task<dynamic> XoaNguoiDung(long idTaiKhoan);
    }

    public class QuanLyNguoiDungService : IQuanLyNguoiDungService
    {
        private readonly ThinhContext _context;

        public QuanLyNguoiDungService(ThinhContext context)
        {
            _context = context;
        }

        public async Task<dynamic> GetDanhSachNguoiDung(LocNguoiDungModel? filter = null)
        {
            var query = _context.TaiKhoans
                .Where(tk => tk.LoaiTaiKhoanId != 3) // Loại trừ admin (LoaiTaiKhoan = 3)
                .AsQueryable();

            // Lọc theo email
            if (filter != null && !string.IsNullOrEmpty(filter.Email))
            {
                query = query.Where(tk => tk.Email.Contains(filter.Email));
            }

            // Lọc theo số điện thoại
            if (filter != null && !string.IsNullOrEmpty(filter.SoDienThoai))
            {
                query = query.Where(tk => tk.SoDienThoai != null && tk.SoDienThoai.Contains(filter.SoDienThoai));
            }

            // Lọc theo loại tài khoản
            if (filter != null && filter.LoaiTaiKhoanId.HasValue)
            {
                query = query.Where(tk => tk.LoaiTaiKhoanId == filter.LoaiTaiKhoanId.Value);
            }

            // Lọc theo tên khách hàng
            if (filter != null && !string.IsNullOrEmpty(filter.TenKhachHang))
            {
                query = query.Where(tk => _context.KhachHangs
                    .Any(kh => kh.IdTaiKhoan == tk.Id && kh.TenKh.Contains(filter.TenKhachHang)));
            }

            // Lọc theo quốc tịch
            if (filter != null && filter.IdQuocTich.HasValue)
            {
                query = query.Where(tk => _context.KhachHangs
                    .Any(kh => kh.IdTaiKhoan == tk.Id && kh.IdQuocTich == filter.IdQuocTich.Value));
            }

            var danhSachNguoiDung = await query
                .Select(tk => new
                {
                    tk.Id,
                    tk.Email,
                    tk.SoDienThoai,
                    tk.LoaiTaiKhoanId,
                    TenLoaiTaiKhoan = _context.LoaiTaiKhoans
                        .Where(ltk => ltk.Id == tk.LoaiTaiKhoanId)
                        .Select(ltk => ltk.TenLoai)
                        .FirstOrDefault(),
                    TenKhachHang = _context.KhachHangs
                        .Where(kh => kh.IdTaiKhoan == tk.Id)
                        .Select(kh => kh.TenKh)
                        .FirstOrDefault(),
                    IdQuocTich = _context.KhachHangs
                        .Where(kh => kh.IdTaiKhoan == tk.Id)
                        .Select(kh => kh.IdQuocTich)
                        .FirstOrDefault(),
                    TenQuocTich = _context.KhachHangs
                        .Where(kh => kh.IdTaiKhoan == tk.Id)
                        .Select(kh => _context.QuocTiches
                            .Where(qt => qt.Id == kh.IdQuocTich)
                            .Select(qt => qt.QuocTich1)
                            .FirstOrDefault())
                        .FirstOrDefault(),
                    tk.HinhAnh,
                    SoDatVe = _context.DatVes.Where(dv => dv.IdTaiKhoan == tk.Id).Count(),
                    SoDatPhong = _context.DatPhongs.Where(dp => dp.IdTaiKhoan == tk.Id).Count(),
                    CoCCCD = _context.KhachHangs
                        .Any(kh => kh.IdTaiKhoan == tk.Id && kh.KhachHangCccd != null),
                    SoPassport = _context.KhachHangs
                        .Where(kh => kh.IdTaiKhoan == tk.Id)
                        .SelectMany(kh => kh.KhachHangPassports)
                        .Count()
                })
                .ToListAsync();

            return new
            {
                statusCode = 200,
                message = "Lấy danh sách người dùng thành công",
                data = danhSachNguoiDung
            };
        }

        public async Task<dynamic> GetChiTietNguoiDung(long idTaiKhoan)
        {
            if (idTaiKhoan <= 0)
            {
                return new { statusCode = 400, message = "Id tài khoản không hợp lệ" };
            }

            var taiKhoan = await _context.TaiKhoans
                .Where(tk => tk.Id == idTaiKhoan && tk.LoaiTaiKhoanId != 3)
                .Select(tk => new
                {
                    tk.Id,
                    tk.Email,
                    tk.SoDienThoai,
                    tk.LoaiTaiKhoanId,
                    LoaiTaiKhoan = _context.LoaiTaiKhoans
                        .Where(ltk => ltk.Id == tk.LoaiTaiKhoanId)
                        .Select(ltk => new
                        {
                            ltk.Id,
                            ltk.TenLoai
                        })
                        .FirstOrDefault(),
                    tk.HinhAnh,
                    tk.IsEmail
                })
                .FirstOrDefaultAsync();

            // Lấy ngày tạo từ refresh token riêng để tránh lỗi datetime conversion
            DateTime? ngayTao = null;
            if (taiKhoan != null)
            {
                try
                {
                    var refreshTokens = await _context.RefreshTokens
                        .Where(rt => rt.IdTaiKhoan == idTaiKhoan)
                        .ToListAsync();
                    
                    if (refreshTokens.Any())
                    {
                        var firstToken = refreshTokens.OrderBy(rt => rt.ExpiryDate).First();
                        // Kiểm tra giá trị datetime hợp lệ (từ năm 1900 đến 2100)
                        if (firstToken.ExpiryDate.Year >= 1900 && firstToken.ExpiryDate.Year <= 2100)
                        {
                            ngayTao = firstToken.ExpiryDate;
                        }
                    }
                }
                catch
                {
                    // Bỏ qua nếu có lỗi datetime conversion
                    ngayTao = null;
                }
            }

            // Tạo lại object với NgayTao
            var taiKhoanWithNgayTao = taiKhoan != null ? new
            {
                taiKhoan.Id,
                taiKhoan.Email,
                taiKhoan.SoDienThoai,
                taiKhoan.LoaiTaiKhoanId,
                taiKhoan.LoaiTaiKhoan,
                taiKhoan.HinhAnh,
                taiKhoan.IsEmail,
                NgayTao = ngayTao
            } : null;

            if (taiKhoan == null)
            {
                return new { statusCode = 404, message = "Người dùng không tồn tại" };
            }

            var khachHang = await _context.KhachHangs
                .Where(kh => kh.IdTaiKhoan == idTaiKhoan)
                .Select(kh => new
                {
                    kh.Id,
                    kh.TenKh,
                    kh.DiaChi,
                    kh.GioiTinh,
                    kh.IdPhuong,
                    Phuong = kh.IdPhuongNavigation != null ? new
                    {
                        kh.IdPhuongNavigation.IdPhuong,
                        kh.IdPhuongNavigation.TenPhuong
                    } : null,
                    kh.IdQuan,
                    Quan = kh.IdQuanNavigation != null ? new
                    {
                        kh.IdQuanNavigation.IdQuan,
                        kh.IdQuanNavigation.TenQuan
                    } : null,
                    kh.IdTinh,
                    Tinh = kh.IdTinhNavigation != null ? new
                    {
                        kh.IdTinhNavigation.IdTinh,
                        kh.IdTinhNavigation.TenTinh
                    } : null,
                    kh.IdQuocTich,
                    QuocTich = kh.IdQuocTichNavigation != null ? new
                    {
                        kh.IdQuocTichNavigation.Id,
                        kh.IdQuocTichNavigation.QuocTich1
                    } : null
                })
                .FirstOrDefaultAsync();

            dynamic? khachHangCccd = null;
            dynamic khachHangPassports = new List<dynamic>();

            if (khachHang != null)
            {
                khachHangCccd = await _context.KhachHangCccds
                    .Where(cccd => cccd.IdKhachHang == khachHang.Id)
                    .Select(cccd => new
                    {
                        cccd.Id,
                        cccd.SoCccd,
                        cccd.TenTrenCccd,
                        cccd.NgayCap,
                        cccd.NoiCap,
                        cccd.NoiThuongTru,
                        cccd.QueQuan
                    })
                    .FirstOrDefaultAsync();

                khachHangPassports = await _context.KhachHangPassports
                    .Where(p => p.IdKhachHang == khachHang.Id)
                    .Select(p => new
                    {
                        p.Id,
                        p.SoPassport,
                        p.TenTrenPassport,
                        p.NgayCap,
                        p.NgayHetHan,
                        p.NoiCap,
                        p.QuocTich,
                        p.LoaiPassport,
                        p.GhiChu
                    })
                    .ToListAsync();
            }

            var lichSuDatVe = await _context.DatVes
                .Where(dv => dv.IdTaiKhoan == idTaiKhoan)
                .OrderByDescending(dv => dv.NgayDat)
                .Take(10)
                .Select(dv => new
                {
                    dv.Id,
                    dv.NgayDat,
                    dv.TrangThai,
                    dv.Gia,
                    TenChuyenBay = _context.ChuyenBays
                        .Where(cb => cb.Id == dv.IdChuyenBay)
                        .Select(cb => cb.MaSanBayDi + " → " + cb.MaSanBayDen)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var lichSuThanhToan = await _context.LichSuThanhToans
                .Where(tt => tt.IdTaiKhoan == idTaiKhoan)
                .OrderByDescending(tt => tt.NgayThanhToan)
                .Take(10)
                .Select(tt => new
                {
                    tt.Id,
                    tt.MaThanhToan,
                    tt.NgayThanhToan,
                    tt.SoTien,
                    tt.LoaiDichVu,
                    TenPhuongThuc = tt.IdPhuongThucThanhToanNavigation.TenPhuongThuc
                })
                .ToListAsync();

            return new
            {
                statusCode = 200,
                message = "Lấy chi tiết người dùng thành công",
                data = new
                {
                    TaiKhoan = taiKhoanWithNgayTao,
                    KhachHang = khachHang,
                    KhachHangCccd = khachHangCccd,
                    KhachHangPassports = khachHangPassports,
                    LichSuDatVe = lichSuDatVe,
                    LichSuThanhToan = lichSuThanhToan
                }
            };
        }

        public async Task<dynamic> CapNhatNguoiDung(CapNhatNguoiDungModel model)
        {
            if (model == null || model.IdTaiKhoan <= 0)
            {
                return new { statusCode = 400, message = "Dữ liệu không hợp lệ" };
            }

            var taiKhoan = await _context.TaiKhoans.FindAsync(model.IdTaiKhoan);
            if (taiKhoan == null || taiKhoan.LoaiTaiKhoanId == 3)
            {
                return new { statusCode = 404, message = "Người dùng không tồn tại hoặc không thể chỉnh sửa" };
            }

            try
            {
                // Cập nhật thông tin tài khoản
                if (!string.IsNullOrEmpty(model.Email))
                {
                    taiKhoan.Email = model.Email;
                }
                if (!string.IsNullOrEmpty(model.SoDienThoai))
                {
                    taiKhoan.SoDienThoai = model.SoDienThoai;
                }
                if (model.LoaiTaiKhoanId.HasValue && model.LoaiTaiKhoanId.Value != 3)
                {
                    taiKhoan.LoaiTaiKhoanId = model.LoaiTaiKhoanId.Value;
                }

                _context.TaiKhoans.Update(taiKhoan);

                // Cập nhật thông tin khách hàng
                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(kh => kh.IdTaiKhoan == model.IdTaiKhoan);

                if (khachHang != null)
                {
                    if (!string.IsNullOrEmpty(model.TenKhachHang))
                    {
                        khachHang.TenKh = model.TenKhachHang;
                    }
                    if (!string.IsNullOrEmpty(model.DiaChi))
                    {
                        khachHang.DiaChi = model.DiaChi;
                    }
                    if (model.GioiTinh.HasValue)
                    {
                        khachHang.GioiTinh = model.GioiTinh.Value;
                    }
                    if (model.IdTinh.HasValue)
                    {
                        khachHang.IdTinh = model.IdTinh.Value;
                    }
                    if (model.IdQuan.HasValue)
                    {
                        khachHang.IdQuan = model.IdQuan.Value;
                    }
                    if (model.IdPhuong.HasValue)
                    {
                        khachHang.IdPhuong = model.IdPhuong.Value;
                    }
                    if (model.IdQuocTich.HasValue)
                    {
                        khachHang.IdQuocTich = model.IdQuocTich.Value;
                    }

                    _context.KhachHangs.Update(khachHang);
                }

                await _context.SaveChangesAsync();

                return new { statusCode = 200, message = "Cập nhật người dùng thành công" };
            }
            catch (Exception ex)
            {
                return new { statusCode = 500, message = "Đã xảy ra lỗi hệ thống", error = ex.Message };
            }
        }

        public async Task<dynamic> XoaNguoiDung(long idTaiKhoan)
        {
            if (idTaiKhoan <= 0)
            {
                return new { statusCode = 400, message = "Id tài khoản không hợp lệ" };
            }

            var taiKhoan = await _context.TaiKhoans.FindAsync(idTaiKhoan);
            if (taiKhoan == null || taiKhoan.LoaiTaiKhoanId == 3)
            {
                return new { statusCode = 404, message = "Người dùng không tồn tại hoặc không thể xóa" };
            }

            // Kiểm tra xem có đặt vé hoặc đặt phòng nào không
            var coDatVe = await _context.DatVes.AnyAsync(dv => dv.IdTaiKhoan == idTaiKhoan);
            var coDatPhong = await _context.DatPhongs.AnyAsync(dp => dp.IdTaiKhoan == idTaiKhoan);

            if (coDatVe || coDatPhong)
            {
                return new { statusCode = 400, message = "Không thể xóa người dùng vì đã có đặt vé hoặc đặt phòng" };
            }

            try
            {
                // Xóa khách hàng passport
                var passports = await _context.KhachHangPassports
                    .Where(p => p.IdKhachHangNavigation.IdTaiKhoan == idTaiKhoan)
                    .ToListAsync();
                _context.KhachHangPassports.RemoveRange(passports);

                // Xóa khách hàng CCCD
                var cccd = await _context.KhachHangCccds
                    .Where(c => c.IdKhachHangNavigation.IdTaiKhoan == idTaiKhoan)
                    .FirstOrDefaultAsync();
                if (cccd != null)
                {
                    _context.KhachHangCccds.Remove(cccd);
                }

                // Xóa khách hàng
                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(kh => kh.IdTaiKhoan == idTaiKhoan);
                if (khachHang != null)
                {
                    _context.KhachHangs.Remove(khachHang);
                }

                // Xóa refresh tokens
                var refreshTokens = await _context.RefreshTokens
                    .Where(rt => rt.IdTaiKhoan == idTaiKhoan)
                    .ToListAsync();
                _context.RefreshTokens.RemoveRange(refreshTokens);

                // Xóa tài khoản
                _context.TaiKhoans.Remove(taiKhoan);

                await _context.SaveChangesAsync();

                return new { statusCode = 200, message = "Xóa người dùng thành công" };
            }
            catch (Exception ex)
            {
                return new { statusCode = 500, message = "Đã xảy ra lỗi hệ thống", error = ex.Message };
            }
        }
    }
}

