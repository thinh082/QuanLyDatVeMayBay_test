using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Models.Model;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QuanLyDatVeMayBay.Services.QuanLy
{
    public interface IQuanLyVeService
    {
        Task<dynamic> GetDanhSachDatVe(LocDatVeAdminModel? filter = null);
        Task<dynamic> GetChiTietDatVe(long idDatVe);
        Task<dynamic> CapNhatTrangThai(CapNhatTrangThaiVeModel model);
        Task<byte[]?> InChiTietVe(long idDatVe);
    }

    public class QuanLyVeService : IQuanLyVeService
    {
        private readonly ThinhContext _context;
        public QuanLyVeService(ThinhContext context)
        {
            _context = context;
        }

        public async Task<dynamic> GetDanhSachDatVe(LocDatVeAdminModel? filter = null)
        {
            var query = _context.DatVes
                .Include(d => d.IdTaiKhoanNavigation)
                .Include(d => d.IdChuyenBayNavigation)
                    .ThenInclude(cb => cb.MaSanBayDiNavigation)
                .Include(d => d.IdChuyenBayNavigation)
                    .ThenInclude(cb => cb.MaSanBayDenNavigation)
                .Include(d => d.LichBay)
                .Include(d => d.ChiTietDatVes)
                .AsQueryable();

            if (filter != null)
            {
                if (filter.MaDatVe.HasValue && filter.MaDatVe.Value > 0)
                    query = query.Where(d => d.Id == filter.MaDatVe.Value);
                if (!string.IsNullOrEmpty(filter.Email))
                    query = query.Where(d => d.IdTaiKhoanNavigation.Email.Contains(filter.Email));
                if (!string.IsNullOrEmpty(filter.SoDienThoai))
                    query = query.Where(d => d.IdTaiKhoanNavigation.SoDienThoai != null && d.IdTaiKhoanNavigation.SoDienThoai.Contains(filter.SoDienThoai));
                if (filter.IdChuyenBay.HasValue)
                    query = query.Where(d => d.IdChuyenBay == filter.IdChuyenBay);
                if (filter.IdLichBay.HasValue)
                    query = query.Where(d => d.LichBayId == filter.IdLichBay);
                if (!string.IsNullOrEmpty(filter.MaSanBayDi))
                    query = query.Where(d => d.IdChuyenBayNavigation.MaSanBayDi == filter.MaSanBayDi);
                if (!string.IsNullOrEmpty(filter.MaSanBayDen))
                    query = query.Where(d => d.IdChuyenBayNavigation.MaSanBayDen == filter.MaSanBayDen);
                if (!string.IsNullOrEmpty(filter.TrangThai))
                    query = query.Where(d => d.TrangThai == filter.TrangThai);
                if (filter.NgayDatFrom.HasValue)
                    query = query.Where(d => d.NgayDat.HasValue && d.NgayDat.Value.Date >= filter.NgayDatFrom.Value.Date);
                if (filter.NgayDatTo.HasValue)
                    query = query.Where(d => d.NgayDat.HasValue && d.NgayDat.Value.Date <= filter.NgayDatTo.Value.Date);
            }

            var data = await query
                .OrderByDescending(d => d.NgayDat)
                .Select(d => new
                {
                    d.Id,
                    Email = d.IdTaiKhoanNavigation.Email,
                    SoDienThoai = d.IdTaiKhoanNavigation.SoDienThoai,
                    d.TrangThai,
                    d.NgayDat,
                    d.Gia,
                    d.IdChuyenBay,
                    MaSanBayDi = d.IdChuyenBayNavigation.MaSanBayDiNavigation.Ten,
                    MaSanBayDen = d.IdChuyenBayNavigation.MaSanBayDenNavigation.Ten,
                    d.LichBayId,
                    SoGhe = d.ChiTietDatVes.Count
                })
                .ToListAsync();

            return new
            {
                statusCode = 200,
                message = "Lấy danh sách đặt vé thành công",
                data
            };
        }

        public async Task<dynamic> GetChiTietDatVe(long idDatVe)
        {
            if (idDatVe <= 0)
                return new { statusCode = 400, message = "Id đặt vé không hợp lệ" };

            var datVe = await _context.DatVes
                .Include(d => d.IdTaiKhoanNavigation)
                .Include(d => d.IdChuyenBayNavigation)
                    .ThenInclude(cb => cb.MaSanBayDiNavigation)
                .Include(d => d.IdChuyenBayNavigation)
                    .ThenInclude(cb => cb.MaSanBayDenNavigation)
                .Include(d => d.LichBay)
                .Include(d => d.ChiTietDatVes)
                    .ThenInclude(ct => ct.IdGheNgoiNavigation)
                .FirstOrDefaultAsync(d => d.Id == idDatVe);

            if (datVe == null)
                return new { statusCode = 404, message = "Không tìm thấy vé" };

            var chiTiet = datVe.ChiTietDatVes.Select(ct => new
            {
                ct.Id,
                ct.IdGheNgoi,
                SoGhe = ct.IdGheNgoiNavigation?.SoGhe,
                IdLoaiVe = ct.IdGheNgoiNavigation?.IdLoaiVe
            });

            var result = new
            {
                datVe.Id,
                datVe.IdTaiKhoan,
                Email = datVe.IdTaiKhoanNavigation.Email,
                SoDienThoai = datVe.IdTaiKhoanNavigation.SoDienThoai,
                datVe.IdChuyenBay,
                SanBayDi = datVe.IdChuyenBayNavigation?.MaSanBayDiNavigation?.Ten,
                SanBayDen = datVe.IdChuyenBayNavigation?.MaSanBayDenNavigation?.Ten,
                datVe.LichBayId,
                datVe.TrangThai,
                datVe.NgayDat,
                datVe.NgayHuy,
                datVe.Gia,
                ChiTiet = chiTiet
            };

            return new { statusCode = 200, message = "Lấy chi tiết vé thành công", data = result };
        }

        public async Task<dynamic> CapNhatTrangThai(CapNhatTrangThaiVeModel model)
        {
            if (model == null || model.IdDatVe <= 0 || string.IsNullOrEmpty(model.TrangThaiMoi))
                return new { statusCode = 400, message = "Dữ liệu không hợp lệ" };

            var datVe = await _context.DatVes
                .Include(d => d.ChiTietDatVes)
                .FirstOrDefaultAsync(d => d.Id == model.IdDatVe);

            if (datVe == null)
                return new { statusCode = 404, message = "Không tìm thấy vé" };

            datVe.TrangThai = model.TrangThaiMoi;
            if (model.TrangThaiMoi.ToLower().Contains("hủy") || model.TrangThaiMoi.ToLower().Contains("huy"))
            {
                datVe.NgayHuy = DateTime.Now;

                // mở lại ghế
                var gheIds = datVe.ChiTietDatVes.Select(ct => ct.IdGheNgoi).ToList();
                var gheLichBay = await _context.GheNgoiLichBays
                    .Where(g => gheIds.Contains(g.IdGheNgoi ?? 0) && g.IdLichBay == datVe.LichBayId)
                    .ToListAsync();
                foreach (var g in gheLichBay)
                {
                    g.TrangThai = 0;
                }
                _context.GheNgoiLichBays.UpdateRange(gheLichBay);
            }

            _context.DatVes.Update(datVe);
            await _context.SaveChangesAsync();

            return new { statusCode = 200, message = "Cập nhật trạng thái vé thành công" };
        }

        public async Task<byte[]?> InChiTietVe(long idDatVe)
        {
            var datVe = await _context.DatVes
                .Include(d => d.IdTaiKhoanNavigation)
                    .ThenInclude(tk => tk.KhachHang)
                .Include(d => d.IdChuyenBayNavigation)
                    .ThenInclude(cb => cb.MaSanBayDiNavigation)
                .Include(d => d.IdChuyenBayNavigation)
                    .ThenInclude(cb => cb.MaSanBayDenNavigation)
                .Include(d => d.IdChuyenBayNavigation)
                    .ThenInclude(cb => cb.IdHangBayNavigation)
                .Include(d => d.LichBay)
                .Include(d => d.ChiTietDatVes)
                    .ThenInclude(ct => ct.IdGheNgoiNavigation)
                        .ThenInclude(g => g.IdLoaiVeNavigation)
                .FirstOrDefaultAsync(d => d.Id == idDatVe);

            if (datVe == null) return null;

            var tenKhachHang = datVe.IdTaiKhoanNavigation?.KhachHang?.TenKh ?? "N/A";
            var email = datVe.IdTaiKhoanNavigation?.Email ?? "";
            var sdt = datVe.IdTaiKhoanNavigation?.SoDienThoai ?? "";
            var sanBayDi = datVe.IdChuyenBayNavigation?.MaSanBayDiNavigation?.Ten ?? "";
            var sanBayDen = datVe.IdChuyenBayNavigation?.MaSanBayDenNavigation?.Ten ?? "";
            var tenHangBay = datVe.IdChuyenBayNavigation?.IdHangBayNavigation?.TenHang ?? "";
            var thoiGianDi = datVe.LichBay?.ThoiGianOsanBayDiUtc;
            var thoiGianDen = datVe.LichBay?.ThoiGianOsanBayDenUtc;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(30);
                    // Dùng Arial để hỗ trợ đầy đủ ký tự tiếng Việt (Đ, Ă, Â,...)
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().AlignCenter().Text("VÉ MÁY BAY ĐIỆN TỬ").Bold().FontSize(18);

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Spacing(6);

                        col.Item().Text($"Mã đặt vé: {datVe.Id}").Bold();
                        col.Item().Text($"Ngày đặt: {datVe.NgayDat?.ToString("dd/MM/yyyy HH:mm") ?? ""}");
                        col.Item().Text($"Trạng thái: {datVe.TrangThai}");
                        col.Item().LineHorizontal(1);

                        col.Item().Text("THÔNG TIN HÀNH KHÁCH").Bold();
                        col.Item().Text($"Họ tên: {tenKhachHang}");
                        col.Item().Text($"Email: {email}");
                        col.Item().Text($"SĐT: {sdt}");
                        col.Item().LineHorizontal(1);

                        col.Item().Text("THÔNG TIN CHUYẾN BAY").Bold();
                        col.Item().Text($"Hãng bay: {tenHangBay}");
                        col.Item().Text($"Điểm đi: {sanBayDi}");
                        col.Item().Text($"Điểm đến: {sanBayDen}");
                        col.Item().Text($"Khởi hành: {thoiGianDi?.ToString("dd/MM/yyyy HH:mm") ?? ""}");
                        col.Item().Text($"Đến nơi: {thoiGianDen?.ToString("dd/MM/yyyy HH:mm") ?? ""}");
                        col.Item().LineHorizontal(1);

                        col.Item().Text("DANH SÁCH GHẾ").Bold();
                        foreach (var ct in datVe.ChiTietDatVes)
                        {
                            var loaiVe = ct.IdGheNgoiNavigation?.IdLoaiVeNavigation?.TenLoaiVe ?? "";
                            col.Item().Text($"- Ghế: {ct.IdGheNgoiNavigation?.SoGhe} | Loại: {loaiVe}");
                        }
                        col.Item().LineHorizontal(1);

                        col.Item().Text($"TỔNG TIỀN: {datVe.Gia?.ToString("N0")} VNĐ").Bold().FontSize(13);
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("In ngày: ");
                        x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}


