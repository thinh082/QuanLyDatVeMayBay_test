using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Models.Model;
using QuanLyDatVeMayBay.Services;

namespace QuanLyDatVeMayBay.Services.QuanLy
{
    public interface IQuanLyChuyenBayService
    {
        // ChuyenBay
        Task<dynamic> GetDanhSachChuyenBay(LocChuyenBayModel? filter = null);
        Task<dynamic> GetChiTietChuyenBay(long idChuyenBay);
        Task<dynamic> LuuChuyenBay(LuuChuyenBayModel model);
        Task<dynamic> XoaChuyenBay(long idChuyenBay);

        // SanBay
        Task<dynamic> GetDanhSachSanBay();
        Task<dynamic> GetChiTietSanBay(string maIata);
        Task<dynamic> ThemSanBay(string maIata, string ten, string? thanhPho, string? quocGia, string timeZoneId, string? ianaTimeZoneId);
        Task<dynamic> CapNhatSanBay(string maIata, string ten, string? thanhPho, string? quocGia, string timeZoneId, string? ianaTimeZoneId);
        Task<dynamic> XoaSanBay(string maIata);

        // HangBay
        Task<dynamic> GetDanhSachHangBay();
        Task<dynamic> GetChiTietHangBay(int idHangBay);
        Task<dynamic> ThemHangBay(string tenHang);
        Task<dynamic> CapNhatHangBay(int idHangBay, string tenHang);
        Task<dynamic> XoaHangBay(int idHangBay);
    }

    public class QuanLyChuyenBayService : IQuanLyChuyenBayService
    {
        private readonly ThinhContext _context;

        public QuanLyChuyenBayService(ThinhContext context)
        {
            _context = context;
        }

        // ChuyenBay Methods
        public async Task<dynamic> GetDanhSachChuyenBay(LocChuyenBayModel? filter = null)
        {
            var query = _context.ChuyenBays.AsQueryable();

            // Lọc theo mã sân bay đi
            if (filter != null && !string.IsNullOrEmpty(filter.MaSanBayDi))
            {
                query = query.Where(cb => cb.MaSanBayDi == filter.MaSanBayDi);
            }

            // Lọc theo mã sân bay đến
            if (filter != null && !string.IsNullOrEmpty(filter.MaSanBayDen))
            {
                query = query.Where(cb => cb.MaSanBayDen == filter.MaSanBayDen);
            }

            // Lọc theo tên hãng bay
            if (filter != null && !string.IsNullOrEmpty(filter.TenHangBay))
            {
                query = query.Where(cb => _context.HangBays
                    .Any(hb => hb.Id == cb.IdHangBay && hb.TenHang.Contains(filter.TenHangBay)));
            }

            // Lấy danh sách chuyến bay (không tính giá trong Select để tránh lỗi EF Core)
            var danhSachChuyenBay = await query
                .Select(cb => new
                {
                    cb.Id,
                    cb.IdHangBay,
                    TenHangBay = _context.HangBays.Where(hb => hb.Id == cb.IdHangBay).Select(hb => hb.TenHang).FirstOrDefault(),
                    cb.MaSanBayDi,
                    TenSanBayDi = _context.SanBays.Where(sb => sb.MaIata == cb.MaSanBayDi).Select(sb => sb.Ten).FirstOrDefault(),
                    cb.MaSanBayDen,
                    TenSanBayDen = _context.SanBays.Where(sb => sb.MaIata == cb.MaSanBayDen).Select(sb => sb.Ten).FirstOrDefault(),
                    SoLichBay = _context.LichBays.Where(lb => lb.IdTuyenBay == cb.Id).Count(),
                    SoGheNgoi = _context.GheNgois.Where(g => g.IdChuyenBay == cb.Id).Count()
                })
                .ToListAsync();

            // Lấy danh sách ID chuyến bay để tính giá
            var chuyenBayIds = danhSachChuyenBay.Select(cb => cb.Id).ToList();

            // Lấy tất cả giá vé liên quan đến các chuyến bay này
            var giaVes = await _context.LichBays
                .Where(lb => chuyenBayIds.Contains(lb.IdTuyenBay))
                .SelectMany(lb => _context.BangGiaVes.Where(bgv => bgv.IdLichBay == lb.Id))
                .Select(bgv => new
                {
                    IdLichBay = bgv.IdLichBay,
                    Gia = bgv.Gia
                })
                .ToListAsync();

            // Lấy mapping IdLichBay -> IdTuyenBay (IdChuyenBay)
            var lichBayToChuyenBay = await _context.LichBays
                .Where(lb => chuyenBayIds.Contains(lb.IdTuyenBay))
                .Select(lb => new
                {
                    IdLichBay = lb.Id,
                    IdChuyenBay = lb.IdTuyenBay
                })
                .ToListAsync();

            // Tính giá min/max cho từng chuyến bay
            var giaByChuyenBay = giaVes
                .Join(lichBayToChuyenBay, g => g.IdLichBay, l => l.IdLichBay, (g, l) => new { l.IdChuyenBay, g.Gia })
                .GroupBy(x => x.IdChuyenBay)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        GiaMin = g.Min(x => x.Gia),
                        GiaMax = g.Max(x => x.Gia)
                    }
                );

            // Thêm giá vào danh sách chuyến bay
            var danhSachChuyenBayWithGia = danhSachChuyenBay.Select(cb => new
            {
                cb.Id,
                cb.IdHangBay,
                cb.TenHangBay,
                cb.MaSanBayDi,
                cb.TenSanBayDi,
                cb.MaSanBayDen,
                cb.TenSanBayDen,
                cb.SoLichBay,
                cb.SoGheNgoi,
                GiaMin = giaByChuyenBay.ContainsKey(cb.Id) ? giaByChuyenBay[cb.Id].GiaMin : (decimal?)null,
                GiaMax = giaByChuyenBay.ContainsKey(cb.Id) ? giaByChuyenBay[cb.Id].GiaMax : (decimal?)null
            }).ToList();

            // Lọc theo giá sau khi tính giá
            if (filter != null && (filter.GiaMin.HasValue || filter.GiaMax.HasValue))
            {
                danhSachChuyenBayWithGia = danhSachChuyenBayWithGia
                    .Where(cb => 
                        (!filter.GiaMin.HasValue || (cb.GiaMin.HasValue && cb.GiaMin.Value >= filter.GiaMin.Value)) &&
                        (!filter.GiaMax.HasValue || (cb.GiaMax.HasValue && cb.GiaMax.Value <= filter.GiaMax.Value))
                    )
                    .ToList();
            }

            // Lấy danh sách lịch bay cho từng chuyến bay
            var finalChuyenBayList = new List<dynamic>();

            // Lấy tất cả lịch bay của các chuyến bay
            var allLichBays = await _context.LichBays
                .Where(lb => chuyenBayIds.Contains(lb.IdTuyenBay))
                .Select(lb => new
                {
                    lb.Id,
                    lb.IdTuyenBay,
                    lb.ThoiGianOsanBayDiUtc,
                    lb.ThoiGianOsanBayDenUtc,
                    lb.ThoiGianBay
                })
                .ToListAsync();

            // Lấy tất cả giá vé cho các lịch bay
            var lichBayIds = allLichBays.Select(lb => lb.Id).ToList();
            var giaVesByLichBay = await _context.BangGiaVes
                .Where(bgv => lichBayIds.Contains(bgv.IdLichBay))
                .GroupBy(bgv => bgv.IdLichBay)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Min(bgv => bgv.Gia)
                );

            // Xử lý từng chuyến bay
            foreach (var cb in danhSachChuyenBayWithGia)
            {
                // Lấy tất cả lịch bay của chuyến bay này (không lọc theo ngày)
                var lichBayList = allLichBays
                    .Where(lb => lb.IdTuyenBay == cb.Id)
                    .OrderBy(lb => lb.ThoiGianOsanBayDiUtc)
                    .Select(lb => new
                    {
                        lb.Id,
                        lb.IdTuyenBay,
                        lb.ThoiGianOsanBayDiUtc,
                        lb.ThoiGianOsanBayDenUtc,
                        lb.ThoiGianBay,
                        Gia = giaVesByLichBay.ContainsKey(lb.Id) ? giaVesByLichBay[lb.Id] : (decimal?)null
                    })
                    .ToList();

                // Thêm danh sách lịch bay vào chuyến bay
                finalChuyenBayList.Add(new
                {
                    cb.Id,
                    cb.IdHangBay,
                    cb.TenHangBay,
                    cb.MaSanBayDi,
                    cb.TenSanBayDi,
                    cb.MaSanBayDen,
                    cb.TenSanBayDen,
                    cb.SoLichBay,
                    cb.SoGheNgoi,
                    cb.GiaMin,
                    cb.GiaMax,
                    LichBayHomNay = lichBayList
                });
            }

            return new
            {
                statusCode = 200,
                message = "Lấy danh sách chuyến bay thành công",
                data = finalChuyenBayList
            };
        }

        public async Task<dynamic> GetChiTietChuyenBay(long idChuyenBay)
        {
            if (idChuyenBay <= 0)
            {
                return new { statusCode = 400, message = "Id chuyến bay không hợp lệ" };
            }

            var chuyenBay = await _context.ChuyenBays
                .Where(cb => cb.Id == idChuyenBay)
                .Select(cb => new
                {
                    cb.Id,
                    cb.IdHangBay,
                    HangBay = _context.HangBays.Where(hb => hb.Id == cb.IdHangBay).Select(hb => new
                    {
                        hb.Id,
                        hb.TenHang
                    }).FirstOrDefault(),
                    cb.MaSanBayDi,
                    SanBayDi = _context.SanBays.Where(sb => sb.MaIata == cb.MaSanBayDi).Select(sb => new
                    {
                        sb.MaIata,
                        sb.Ten,
                        sb.ThanhPho,
                        sb.QuocGia,
                        sb.TimeZoneId,
                        sb.IanaTimeZoneId
                    }).FirstOrDefault(),
                    cb.MaSanBayDen,
                    SanBayDen = _context.SanBays.Where(sb => sb.MaIata == cb.MaSanBayDen).Select(sb => new
                    {
                        sb.MaIata,
                        sb.Ten,
                        sb.ThanhPho,
                        sb.QuocGia,
                        sb.TimeZoneId,
                        sb.IanaTimeZoneId
                    }).FirstOrDefault(),
                    SoLichBay = _context.LichBays.Where(lb => lb.IdTuyenBay == cb.Id).Count(),
                    SoGheNgoi = _context.GheNgois.Where(g => g.IdChuyenBay == cb.Id).Count(),
                    SoDatVe = _context.DatVes.Where(dv => _context.LichBays.Any(lb => lb.Id == dv.LichBayId && lb.IdTuyenBay == cb.Id)).Count()
                })
                .FirstOrDefaultAsync();

            if (chuyenBay == null)
            {
                return new { statusCode = 404, message = "Chuyến bay không tồn tại" };
            }

            // Lấy thông tin sân bay đi để có timezone
            var sanBayDi = await _context.SanBays.FindAsync(chuyenBay.MaSanBayDi);
            if (sanBayDi == null)
            {
                return new { statusCode = 404, message = "Sân bay đi không tồn tại" };
            }

            // Tính ngày hôm nay và chuyển đổi sang UTC
            var ngayHomNay = DateTime.SpecifyKind(DateTime.Now.Date, DateTimeKind.Unspecified);
            var start_UTC_ngayHomNay = ThinhService.TimeZoneHelper.ToUtc(ngayHomNay, sanBayDi.TimeZoneId);
            var end_UTC_ngayHomNay = ThinhService.TimeZoneHelper.ToUtc(ngayHomNay.AddDays(1).AddTicks(-1), sanBayDi.TimeZoneId);

            // Lấy danh sách lịch bay trong ngày hôm nay
            var lichBayList = await _context.LichBays
                .Where(lb => lb.IdTuyenBay == idChuyenBay &&
                             lb.ThoiGianOsanBayDiUtc >= start_UTC_ngayHomNay &&
                             lb.ThoiGianOsanBayDiUtc <= end_UTC_ngayHomNay)
                .OrderBy(lb => lb.ThoiGianOsanBayDiUtc)
                .Select(lb => new
                {
                    lb.Id,
                    lb.IdTuyenBay,
                    lb.ThoiGianOsanBayDiUtc,
                    lb.ThoiGianOsanBayDenUtc,
                    lb.ThoiGianBay
                })
                .ToListAsync();

            // Lấy giá vé cho các lịch bay (nếu có)
            Dictionary<long, decimal> giaVes = new Dictionary<long, decimal>();
            if (lichBayList.Any())
            {
                var lichBayIds = lichBayList.Select(lb => lb.Id).ToList();
                var giaVesData = await _context.BangGiaVes
                    .Where(bgv => lichBayIds.Contains(bgv.IdLichBay))
                    .GroupBy(bgv => bgv.IdLichBay)
                    .Select(g => new
                    {
                        IdLichBay = g.Key,
                        GiaMin = g.Min(bgv => bgv.Gia)
                    })
                    .ToListAsync();
                
                giaVes = giaVesData.ToDictionary(g => g.IdLichBay, g => g.GiaMin);
            }

            // Kết hợp dữ liệu
            var lichBayHomNay = lichBayList.Select(lb => new
            {
                lb.Id,
                lb.IdTuyenBay,
                lb.ThoiGianOsanBayDiUtc,
                lb.ThoiGianOsanBayDenUtc,
                lb.ThoiGianBay,
                Gia = giaVes.ContainsKey(lb.Id) ? (decimal?)giaVes[lb.Id] : null
            }).ToList();

            // Lấy danh sách tiện nghi của chuyến bay
            var tienNghi = await _context.TienNghiChuyenMayBays
                .Where(tn => tn.IdChuyenBay == idChuyenBay)
                .Select(tn => new
                {
                    tn.Id,
                    tn.IdTienNghi,
                    TenTienNghi = tn.IdTienNghiNavigation != null ? tn.IdTienNghiNavigation.TenTienIch : null,
                    MaTienIch = tn.IdTienNghiNavigation != null ? tn.IdTienNghiNavigation.MaTienIch : null
                })
                .ToListAsync();

            return new
            {
                statusCode = 200,
                message = "Lấy chi tiết chuyến bay thành công",
                data = new
                {
                    chuyenBay.Id,
                    chuyenBay.IdHangBay,
                    chuyenBay.HangBay,
                    chuyenBay.MaSanBayDi,
                    chuyenBay.SanBayDi,
                    chuyenBay.MaSanBayDen,
                    chuyenBay.SanBayDen,
                    chuyenBay.SoLichBay,
                    chuyenBay.SoGheNgoi,
                    chuyenBay.SoDatVe,
                    LichBayHomNay = lichBayHomNay,
                    TienNghi = tienNghi
                }
            };
        }

        public async Task<dynamic> LuuChuyenBay(LuuChuyenBayModel model)
        {
            // ===== VALIDATE INPUT =====
            if (model.idHangBay <= 0 || string.IsNullOrEmpty(model.maSanBayDi) || string.IsNullOrEmpty(model.maSanBayDen))
                return new { statusCode = 400, message = "Dữ liệu không hợp lệ" };

            if (model.maSanBayDi == model.maSanBayDen)
                return new { statusCode = 400, message = "Sân bay đi và sân bay đến không được trùng nhau" };

            // ===== CHECK HÃNG BAY =====
            var hangBay = await _context.HangBays.FindAsync((model.idHangBay));
            if (hangBay == null)
                return new { statusCode = 404, message = "Hãng bay không tồn tại" };

            // ===== CHECK SÂN BAY =====
            var sanBayDi = await _context.SanBays.FindAsync((model.maSanBayDi));
            var sanBayDen = await _context.SanBays.FindAsync((model.maSanBayDen));

            if (sanBayDi == null || sanBayDen == null)
                return new { statusCode = 404, message = "Sân bay không tồn tại" };

            try
            {
                // ===================================
                // 1) THÊM MỚI
                // ===================================
                if (model.idChuyenBay <= 0)
                {
                    var chuyenBayMoi = new ChuyenBay
                    {
                        IdHangBay = model.idHangBay,
                        MaSanBayDi = model.maSanBayDi,
                        MaSanBayDen = model.maSanBayDen
                    };

                    _context.ChuyenBays.Add(chuyenBayMoi);
                    await _context.SaveChangesAsync();

                    return new
                    {
                        statusCode = 200,
                        message = "Thêm chuyến bay thành công",
                        data = new { id = chuyenBayMoi.Id }
                    };
                }

                // ===================================
                // 2) CẬP NHẬT
                // ===================================
                var chuyenBay = await _context.ChuyenBays.FindAsync(model.idChuyenBay);
                if (chuyenBay == null)
                    return new { statusCode = 404, message = "Chuyến bay không tồn tại" };

                chuyenBay.IdHangBay = model.idHangBay;
                chuyenBay.MaSanBayDi = model.maSanBayDi;
                chuyenBay.MaSanBayDen = model.maSanBayDen;

                _context.ChuyenBays.Update(chuyenBay);
                await _context.SaveChangesAsync();

                return new { statusCode = 200, message = "Cập nhật chuyến bay thành công" };
            }
            catch (Exception ex)
            {
                return new { statusCode = 500, message = "Đã xảy ra lỗi hệ thống", error = ex.Message };
            }
        }


        public async Task<dynamic> XoaChuyenBay(long idChuyenBay)
        {
            if (idChuyenBay <= 0)
            {
                return new { statusCode = 400, message = "Id chuyến bay không hợp lệ" };
            }

            var chuyenBay = await _context.ChuyenBays.FindAsync(idChuyenBay);
            if (chuyenBay == null)
            {
                return new { statusCode = 404, message = "Chuyến bay không tồn tại" };
            }

            // Kiểm tra xem có lịch bay nào đang sử dụng chuyến bay này không
            var coLichBay = await _context.LichBays.AnyAsync(lb => lb.IdTuyenBay == idChuyenBay);
            if (coLichBay)
            {
                return new { statusCode = 400, message = "Không thể xóa chuyến bay vì đang có lịch bay sử dụng" };
            }

            try
            {
                _context.ChuyenBays.Remove(chuyenBay);
                await _context.SaveChangesAsync();

                return new { statusCode = 200, message = "Xóa chuyến bay thành công" };
            }
            catch (Exception ex)
            {
                return new { statusCode = 500, message = "Đã xảy ra lỗi hệ thống", error = ex.Message };
            }
        }

        // SanBay Methods
        public async Task<dynamic> GetDanhSachSanBay()
        {
            var danhSachSanBay = await _context.SanBays
                .Select(sb => new
                {
                    sb.MaIata,
                    sb.Ten,
                    sb.ThanhPho,
                    sb.QuocGia,
                    sb.TimeZoneId,
                    sb.IanaTimeZoneId,
                    SoChuyenBayDi = _context.ChuyenBays.Where(cb => cb.MaSanBayDi == sb.MaIata).Count(),
                    SoChuyenBayDen = _context.ChuyenBays.Where(cb => cb.MaSanBayDen == sb.MaIata).Count()
                })
                .ToListAsync();

            return new
            {
                statusCode = 200,
                message = "Lấy danh sách sân bay thành công",
                data = danhSachSanBay
            };
        }

        public async Task<dynamic> GetChiTietSanBay(string maIata)
        {
            if (string.IsNullOrEmpty(maIata))
            {
                return new { statusCode = 400, message = "Mã IATA không hợp lệ" };
            }

            var sanBay = await _context.SanBays
                .Where(sb => sb.MaIata == maIata)
                .Select(sb => new
                {
                    sb.MaIata,
                    sb.Ten,
                    sb.ThanhPho,
                    sb.QuocGia,
                    sb.TimeZoneId,
                    sb.IanaTimeZoneId,
                    SoChuyenBayDi = _context.ChuyenBays.Where(cb => cb.MaSanBayDi == sb.MaIata).Count(),
                    SoChuyenBayDen = _context.ChuyenBays.Where(cb => cb.MaSanBayDen == sb.MaIata).Count(),
                    ChuyenBayDi = _context.ChuyenBays.Where(cb => cb.MaSanBayDi == sb.MaIata)
                        .Select(cb => new
                        {
                            cb.Id,
                            TenHangBay = _context.HangBays.Where(hb => hb.Id == cb.IdHangBay).Select(hb => hb.TenHang).FirstOrDefault(),
                            TenSanBayDen = _context.SanBays.Where(sb2 => sb2.MaIata == cb.MaSanBayDen).Select(sb2 => sb2.Ten).FirstOrDefault()
                        })
                        .Take(10)
                        .ToList(),
                    ChuyenBayDen = _context.ChuyenBays.Where(cb => cb.MaSanBayDen == sb.MaIata)
                        .Select(cb => new
                        {
                            cb.Id,
                            TenHangBay = _context.HangBays.Where(hb => hb.Id == cb.IdHangBay).Select(hb => hb.TenHang).FirstOrDefault(),
                            TenSanBayDi = _context.SanBays.Where(sb2 => sb2.MaIata == cb.MaSanBayDi).Select(sb2 => sb2.Ten).FirstOrDefault()
                        })
                        .Take(10)
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (sanBay == null)
            {
                return new { statusCode = 404, message = "Sân bay không tồn tại" };
            }

            return new
            {
                statusCode = 200,
                message = "Lấy chi tiết sân bay thành công",
                data = sanBay
            };
        }

        public async Task<dynamic> ThemSanBay(string maIata, string ten, string? thanhPho, string? quocGia, string timeZoneId, string? ianaTimeZoneId)
        {
            if (string.IsNullOrEmpty(maIata) || string.IsNullOrEmpty(ten) || string.IsNullOrEmpty(timeZoneId))
            {
                return new { statusCode = 400, message = "Dữ liệu không hợp lệ" };
            }

            var sanBayTonTai = await _context.SanBays.FindAsync(maIata);
            if (sanBayTonTai != null)
            {
                return new { statusCode = 409, message = "Mã IATA đã tồn tại" };
            }

            try
            {
                var sanBay = new SanBay
                {
                    MaIata = maIata,
                    Ten = ten,
                    ThanhPho = thanhPho,
                    QuocGia = quocGia,
                    TimeZoneId = timeZoneId,
                    IanaTimeZoneId = ianaTimeZoneId
                };

                _context.SanBays.Add(sanBay);
                await _context.SaveChangesAsync();

                return new
                {
                    statusCode = 200,
                    message = "Thêm sân bay thành công",
                    data = new { sanBay.MaIata }
                };
            }
            catch (Exception ex)
            {
                return new { statusCode = 500, message = "Đã xảy ra lỗi hệ thống", error = ex.Message };
            }
        }

        public async Task<dynamic> CapNhatSanBay(string maIata, string ten, string? thanhPho, string? quocGia, string timeZoneId, string? ianaTimeZoneId)
        {
            if (string.IsNullOrEmpty(maIata) || string.IsNullOrEmpty(ten) || string.IsNullOrEmpty(timeZoneId))
            {
                return new { statusCode = 400, message = "Dữ liệu không hợp lệ" };
            }

            var sanBay = await _context.SanBays.FindAsync(maIata);
            if (sanBay == null)
            {
                return new { statusCode = 404, message = "Sân bay không tồn tại" };
            }

            try
            {
                sanBay.Ten = ten;
                sanBay.ThanhPho = thanhPho;
                sanBay.QuocGia = quocGia;
                sanBay.TimeZoneId = timeZoneId;
                sanBay.IanaTimeZoneId = ianaTimeZoneId;

                _context.SanBays.Update(sanBay);
                await _context.SaveChangesAsync();

                return new { statusCode = 200, message = "Cập nhật sân bay thành công" };
            }
            catch (Exception ex)
            {
                return new { statusCode = 500, message = "Đã xảy ra lỗi hệ thống", error = ex.Message };
            }
        }

        public async Task<dynamic> XoaSanBay(string maIata)
        {
            if (string.IsNullOrEmpty(maIata))
            {
                return new { statusCode = 400, message = "Mã IATA không hợp lệ" };
            }

            var sanBay = await _context.SanBays.FindAsync(maIata);
            if (sanBay == null)
            {
                return new { statusCode = 404, message = "Sân bay không tồn tại" };
            }

            // Kiểm tra xem có chuyến bay nào đang sử dụng sân bay này không
            var coChuyenBay = await _context.ChuyenBays
                .AnyAsync(cb => cb.MaSanBayDi == maIata || cb.MaSanBayDen == maIata);
            if (coChuyenBay)
            {
                return new { statusCode = 400, message = "Không thể xóa sân bay vì đang có chuyến bay sử dụng" };
            }

            try
            {
                _context.SanBays.Remove(sanBay);
                await _context.SaveChangesAsync();

                return new { statusCode = 200, message = "Xóa sân bay thành công" };
            }
            catch (Exception ex)
            {
                return new { statusCode = 500, message = "Đã xảy ra lỗi hệ thống", error = ex.Message };
            }
        }

        // HangBay Methods
        public async Task<dynamic> GetDanhSachHangBay()
        {
            var danhSachHangBay = await _context.HangBays
                .Select(hb => new
                {
                    hb.Id,
                    hb.TenHang,
                    SoChuyenBay = _context.ChuyenBays.Where(cb => cb.IdHangBay == hb.Id).Count()
                })
                .ToListAsync();

            return new
            {
                statusCode = 200,
                message = "Lấy danh sách hãng bay thành công",
                data = danhSachHangBay
            };
        }

        public async Task<dynamic> GetChiTietHangBay(int idHangBay)
        {
            if (idHangBay <= 0)
            {
                return new { statusCode = 400, message = "Id hãng bay không hợp lệ" };
            }

            var hangBay = await _context.HangBays
                .Where(hb => hb.Id == idHangBay)
                .Select(hb => new
                {
                    hb.Id,
                    hb.TenHang,
                    SoChuyenBay = _context.ChuyenBays.Where(cb => cb.IdHangBay == hb.Id).Count(),
                    ChuyenBays = _context.ChuyenBays.Where(cb => cb.IdHangBay == hb.Id)
                        .Select(cb => new
                        {
                            cb.Id,
                            TenSanBayDi = _context.SanBays.Where(sb => sb.MaIata == cb.MaSanBayDi).Select(sb => sb.Ten).FirstOrDefault(),
                            TenSanBayDen = _context.SanBays.Where(sb => sb.MaIata == cb.MaSanBayDen).Select(sb => sb.Ten).FirstOrDefault()
                        })
                        .Take(10)
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (hangBay == null)
            {
                return new { statusCode = 404, message = "Hãng bay không tồn tại" };
            }

            return new
            {
                statusCode = 200,
                message = "Lấy chi tiết hãng bay thành công",
                data = hangBay
            };
        }

        public async Task<dynamic> ThemHangBay(string tenHang)
        {
            if (string.IsNullOrEmpty(tenHang))
            {
                return new { statusCode = 400, message = "Tên hãng bay không được để trống" };
            }

            try
            {
                var hangBay = new HangBay
                {
                    TenHang = tenHang
                };

                _context.HangBays.Add(hangBay);
                await _context.SaveChangesAsync();

                return new
                {
                    statusCode = 200,
                    message = "Thêm hãng bay thành công",
                    data = new { hangBay.Id }
                };
            }
            catch (Exception ex)
            {
                return new { statusCode = 500, message = "Đã xảy ra lỗi hệ thống", error = ex.Message };
            }
        }

        public async Task<dynamic> CapNhatHangBay(int idHangBay, string tenHang)
        {
            if (idHangBay <= 0 || string.IsNullOrEmpty(tenHang))
            {
                return new { statusCode = 400, message = "Dữ liệu không hợp lệ" };
            }

            var hangBay = await _context.HangBays.FindAsync(idHangBay);
            if (hangBay == null)
            {
                return new { statusCode = 404, message = "Hãng bay không tồn tại" };
            }

            try
            {
                hangBay.TenHang = tenHang;

                _context.HangBays.Update(hangBay);
                await _context.SaveChangesAsync();

                return new { statusCode = 200, message = "Cập nhật hãng bay thành công" };
            }
            catch (Exception ex)
            {
                return new { statusCode = 500, message = "Đã xảy ra lỗi hệ thống", error = ex.Message };
            }
        }

        public async Task<dynamic> XoaHangBay(int idHangBay)
        {
            if (idHangBay <= 0)
            {
                return new { statusCode = 400, message = "Id hãng bay không hợp lệ" };
            }

            var hangBay = await _context.HangBays.FindAsync(idHangBay);
            if (hangBay == null)
            {
                return new { statusCode = 404, message = "Hãng bay không tồn tại" };
            }

            // Kiểm tra xem có chuyến bay nào đang sử dụng hãng bay này không
            var coChuyenBay = await _context.ChuyenBays.AnyAsync(cb => cb.IdHangBay == idHangBay);
            if (coChuyenBay)
            {
                return new { statusCode = 400, message = "Không thể xóa hãng bay vì đang có chuyến bay sử dụng" };
            }

            try
            {
                _context.HangBays.Remove(hangBay);
                await _context.SaveChangesAsync();

                return new { statusCode = 200, message = "Xóa hãng bay thành công" };
            }
            catch (Exception ex)
            {
                return new { statusCode = 500, message = "Đã xảy ra lỗi hệ thống", error = ex.Message };
            }
        }
    }
}

