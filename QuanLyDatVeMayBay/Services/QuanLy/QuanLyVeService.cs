using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Models.Model;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

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
            QuestPDF.Settings.EnableDebugging = false;

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

            // Lấy mã sân bay (dùng key nếu tên không có)
            var maSanBayDi = datVe.IdChuyenBayNavigation?.MaSanBayDi ?? "";
            var maSanBayDen = datVe.IdChuyenBayNavigation?.MaSanBayDen ?? "";
            var soHieuCB = datVe.IdChuyenBay.ToString();
            var trangThaiHienThi = datVe.TrangThai ?? "";
            if (trangThaiHienThi.Length > 18)
                trangThaiHienThi = $"{trangThaiHienThi[..18]}...";

            // Tải ảnh trước (ngoài lambda để không block rendering)
            async Task<byte[]?> TaiAnhAsync(string url)
            {
                try
                {
                    using var httpClient = new System.Net.Http.HttpClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(5);
                    return await httpClient.GetByteArrayAsync(url);
                }
                catch
                {
                    return null;
                }
            }

            byte[]? TaoQrCodeBytes(string content)
            {
                try
                {
                    using var qrGenerator = new QRCoder.QRCodeGenerator();
                    using var qrCodeData = qrGenerator.CreateQrCode(content, QRCoder.QRCodeGenerator.ECCLevel.Q);
                    using var qrCode = new QRCoder.PngByteQRCode(qrCodeData);
                    return qrCode.GetGraphic(8);
                }
                catch
                {
                    return null;
                }
            }

            void VeNenHeaderGradient(SKCanvas canvas, Size size)
            {
                using var shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, 0),
                    new SKPoint(size.Width, size.Height),
                    new[] { SKColor.Parse("#0B3D91"), SKColor.Parse("#1D8FE1") },
                    null,
                    SKShaderTileMode.Clamp);

                using var paint = new SKPaint
                {
                    IsAntialias = true,
                    Shader = shader
                };

                canvas.DrawRect(new SKRect(0, 0, size.Width, size.Height), paint);
            }

            void VeIconThuongHieu(SKCanvas canvas, Size size)
            {
                using var linePaint = new SKPaint
                {
                    Color = SKColor.Parse("#FFFFFF"),
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2.4f,
                    StrokeCap = SKStrokeCap.Round
                };

                using var trailPaint = new SKPaint
                {
                    Color = SKColor.Parse("#BFDBFE"),
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1.4f,
                    StrokeCap = SKStrokeCap.Round
                };

                var flightPath = new SKPath();
                flightPath.MoveTo(size.Width * 0.1f, size.Height * 0.78f);
                flightPath.CubicTo(
                    size.Width * 0.35f, size.Height * 0.95f,
                    size.Width * 0.55f, size.Height * 0.42f,
                    size.Width * 0.92f, size.Height * 0.22f);
                canvas.DrawPath(flightPath, trailPaint);

                canvas.DrawLine(size.Width * 0.32f, size.Height * 0.56f, size.Width * 0.74f, size.Height * 0.30f, linePaint);
                canvas.DrawLine(size.Width * 0.44f, size.Height * 0.64f, size.Width * 0.60f, size.Height * 0.78f, linePaint);
            }

            void VeKhungBoGoc(SKCanvas canvas, Size size, string nenMau)
            {
                using var fillPaint = new SKPaint
                {
                    Color = SKColor.Parse(nenMau),
                    IsAntialias = true
                };

                canvas.DrawRoundRect(new SKRect(0, 0, size.Width, size.Height), 7f, 7f, fillPaint);
            }

            void TieuDeMuc(IContainer container, string tieuDe)
            {
                container.Row(row =>
                {
                    row.ConstantItem(4).Background("#2563EB");
                    row.RelativeItem().PaddingLeft(6)
                        .Text(tieuDe)
                        .SemiBold().FontSize(8).FontColor("#1E3A8A");
                });
            }

            var logoBytes = await TaiAnhAsync(
                "https://res.cloudinary.com/dzffkairf/image/upload/v1774836382/logo-khong-background2_jggyeb.png");
            var signBytes = await TaiAnhAsync(
                "https://res.cloudinary.com/dzffkairf/image/upload/v1774797195/sign_bk91ae.png");
            var qrBytes = TaoQrCodeBytes(
                $"DATVE:{datVe.Id}|CHUYENBAY:{soHieuCB}|HANHKHACH:{tenKhachHang}|GHE:{datVe.ChiTietDatVes.Count}");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(3);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(7).FontFamily("Segoe UI"));

                    // ===== HEADER =====
                    page.Header().Column(header =>
                    {
                        header.Spacing(2);

                        header.Item().MinHeight(62).Layers(layers =>
                        {
                            layers.Layer().Canvas((canvas, size) =>
                            {
                                VeNenHeaderGradient(canvas, size);
                            });

                            layers.PrimaryLayer().Padding(5).Row(row =>
                            {
                                row.RelativeItem().AlignMiddle().Row(brand =>
                                {
                                    brand.ConstantItem(22).AlignMiddle().Height(22)
                                        .Canvas((canvas, size) => VeIconThuongHieu(canvas, size));

                                    brand.ConstantItem(4);

                                    if (logoBytes != null)
                                    {
                                        brand.ConstantItem(24).AlignMiddle().Height(20)
                                            .Image(logoBytes).FitArea();
                                    }
                                    else
                                    {
                                        brand.ConstantItem(24).Height(20);
                                    }

                                    brand.RelativeItem().AlignMiddle().Column(c =>
                                    {
                                        c.Spacing(1);
                                        c.Item().Text("Parador")
                                            .FontFamily("Segoe UI")
                                            .Bold().FontSize(14).FontColor(Colors.White);
                                        c.Item().Text("Cong Ty TNHH Parador")
                                            .SemiBold().FontSize(6).FontColor("#DBEAFE");
                                        c.Item().Text("0822316128")
                                            .FontSize(6).FontColor("#DBEAFE");
                                    });
                                });

                                row.ConstantItem(84).AlignMiddle().MinHeight(42).Layers(codeBox =>
                                {
                                    codeBox.Layer().Canvas((canvas, size) =>
                                    {
                                        VeKhungBoGoc(canvas, size, "#0A2F70");
                                    });

                                    codeBox.PrimaryLayer().Padding(6).Column(c =>
                                    {
                                        c.Item().AlignCenter().Text("MÃ VÉ")
                                            .SemiBold().FontSize(5.5f).FontColor("#BFDBFE");
                                        c.Item().AlignCenter().Text($"#{datVe.Id}")
                                            .Bold().FontSize(11).FontColor(Colors.White);
                                        c.Item().AlignCenter().Text(trangThaiHienThi)
                                            .FontSize(5).FontColor("#86EFAC");
                                    });
                                });
                            });
                        });

                        header.Item()
                            .PaddingHorizontal(1).PaddingTop(1)
                            .Row(row =>
                            {
                                row.RelativeItem().Text("VE MAY BAY DIEN TU")
                                    .SemiBold().FontSize(8).FontColor("#1E3A8A");
                                row.RelativeItem().AlignRight()
                                    .Text($"Ngay dat: {datVe.NgayDat?.ToString("dd/MM/yyyy HH:mm") ?? ""}")
                                    .FontSize(6.5f).FontColor("#334155");
                            });
                    });

                                        // ===== CONTENT =====
                    page.Content().PaddingTop(4).Column(col =>
                    {
                        col.Spacing(5);

                        // --- Thong tin hanh khach ---
                        col.Item().Column(section =>
                        {
                            TieuDeMuc(section.Item(), "THONG TIN HANH KHACH");
                            section.Item().PaddingTop(3).Layers(card =>
                            {
                                card.Layer().Canvas((canvas, size) =>
                                {
                                    VeKhungBoGoc(canvas, size, "#FFFFFF");
                                });

                                card.PrimaryLayer().Padding(5).Column(info =>
                                {
                                    info.Spacing(2);
                                    info.Item().Row(r =>
                                    {
                                        r.ConstantItem(72).Text("Ho ten:").SemiBold().FontColor("#1E3A8A");
                                        r.RelativeItem().Text(tenKhachHang);
                                    });
                                    info.Item().Row(r =>
                                    {
                                        r.ConstantItem(72).Text("Email:").SemiBold().FontColor("#1E3A8A");
                                        r.RelativeItem().Text(email);
                                    });
                                    info.Item().Row(r =>
                                    {
                                        r.ConstantItem(72).Text("So dien thoai:").SemiBold().FontColor("#1E3A8A");
                                        r.RelativeItem().Text(sdt);
                                    });
                                    info.Item().Row(r =>
                                    {
                                        r.ConstantItem(72).Text("Hang bay:").SemiBold().FontColor("#1E3A8A");
                                        r.RelativeItem().Text(tenHangBay);
                                    });
                                });
                            });
                        });

                        // --- Thong tin chuyen bay ---
                        col.Item().Column(section =>
                        {
                            TieuDeMuc(section.Item(), "THONG TIN CHUYEN BAY");
                            section.Item().PaddingTop(3).Layers(card =>
                            {
                                card.Layer().Canvas((canvas, size) =>
                                {
                                    VeKhungBoGoc(canvas, size, "#EFF6FF");
                                });

                                card.PrimaryLayer().Padding(5).Column(chuyenBay =>
                                {
                                    chuyenBay.Item().Row(row =>
                                    {
                                        row.RelativeItem().AlignCenter().Column(c =>
                                        {
                                            c.Item().AlignCenter().Text(maSanBayDi)
                                                .Bold().FontSize(17).FontColor("#1D4ED8");
                                            c.Item().AlignCenter().Text(sanBayDi)
                                                .FontSize(7).FontColor("#334155");
                                            c.Item().AlignCenter().Text(thoiGianDi?.ToString("HH:mm") ?? "")
                                                .SemiBold().FontSize(9);
                                            c.Item().AlignCenter().Text(thoiGianDi?.ToString("dd/MM/yyyy") ?? "")
                                                .FontSize(7).FontColor("#64748B");
                                        });

                                        row.ConstantItem(64).AlignMiddle().Layers(center =>
                                        {
                                            center.Layer().AlignMiddle().PaddingHorizontal(6)
                                                .LineHorizontal(1).LineColor("#93C5FD");
                                            center.PrimaryLayer().AlignCenter().Column(c =>
                                            {
                                                c.Item().AlignCenter().Text("\u2708")
                                                    .FontFamily("Segoe UI Symbol")
                                                    .FontSize(12).FontColor("#2563EB");
                                                c.Item().AlignCenter().Text(soHieuCB)
                                                    .FontSize(6).FontColor("#475569");
                                            });
                                        });

                                        row.RelativeItem().AlignCenter().Column(c =>
                                        {
                                            c.Item().AlignCenter().Text(maSanBayDen)
                                                .Bold().FontSize(17).FontColor("#1D4ED8");
                                            c.Item().AlignCenter().Text(sanBayDen)
                                                .FontSize(7).FontColor("#334155");
                                            c.Item().AlignCenter().Text(thoiGianDen?.ToString("HH:mm") ?? "")
                                                .SemiBold().FontSize(9);
                                            c.Item().AlignCenter().Text(thoiGianDen?.ToString("dd/MM/yyyy") ?? "")
                                                .FontSize(7).FontColor("#64748B");
                                        });
                                    });
                                });
                            });
                        });

                        // --- Danh sach ghe ---
                        col.Item().Column(section =>
                        {
                            TieuDeMuc(section.Item(), "DANH SACH GHE");
                            section.Item().PaddingTop(3).Column(list =>
                            {
                                int stt = 1;
                                foreach (var ct in datVe.ChiTietDatVes)
                                {
                                    var loaiVe = ct.IdGheNgoiNavigation?.IdLoaiVeNavigation?.TenLoaiVe ?? "--";
                                    var soGhe = ct.IdGheNgoiNavigation?.SoGhe ?? "--";

                                    list.Item().PaddingBottom(2).MinHeight(28).Layers(card =>
                                    {
                                        card.Layer().Canvas((canvas, size) =>
                                        {
                                            VeKhungBoGoc(canvas, size, "#FFFFFF");
                                        });

                                        card.PrimaryLayer().PaddingHorizontal(6).PaddingVertical(4).Row(r =>
                                        {
                                            r.ConstantItem(24).AlignMiddle().Text($"{stt}")
                                                .SemiBold().FontSize(7).FontColor("#1D4ED8");
                                            r.ConstantItem(72).AlignMiddle().Text(soGhe)
                                                .SemiBold().FontSize(8);
                                            r.RelativeItem().AlignMiddle().Text(loaiVe)
                                                .FontSize(7).FontColor("#334155");
                                        });
                                    });

                                    stt++;
                                }

                                if (!datVe.ChiTietDatVes.Any())
                                {
                                    list.Item().Text("Khong co ghe nao trong dat ve nay.")
                                        .Italic().FontSize(7).FontColor("#64748B");
                                }
                            });
                        });

                        // --- Tong tien ---
                        col.Item().Layers(total =>
                        {
                            total.Layer().Canvas((canvas, size) =>
                            {
                                VeKhungBoGoc(canvas, size, "#0B3D91");
                            });

                            total.PrimaryLayer().Padding(6).Row(row =>
                            {
                                row.RelativeItem().AlignMiddle().Text("TONG TIEN THANH TOAN")
                                    .SemiBold().FontSize(7).FontColor("#BFDBFE");
                                row.ConstantItem(100).AlignRight().AlignMiddle()
                                    .Text($"{datVe.Gia?.ToString("N0")} VND")
                                    .Bold().FontSize(10).FontColor("#FDE047");
                            });
                        });

                        // --- Loi cam on + chu ky + QR ---
                        col.Item().Layers(bottom =>
                        {
                            bottom.Layer().Canvas((canvas, size) =>
                            {
                                VeKhungBoGoc(canvas, size, "#FFFFFF");
                            });

                            bottom.PrimaryLayer().Padding(6).Row(row =>
                            {
                                row.RelativeItem().AlignMiddle().Column(c =>
                                {
                                    c.Spacing(1);
                                    c.Item().Text("Cam on quy khach da su dung dich vu!")
                                        .Italic().FontSize(7).FontColor("#475569");
                                    c.Item().Text("Vui long quet QR de check-in nhanh.")
                                        .Italic().FontSize(7).FontColor("#475569");
                                });

                                row.ConstantItem(84).AlignRight().Column(c =>
                                {
                                    c.Item().AlignRight().Text("Chu ky nhan vien")
                                        .SemiBold().FontSize(6).FontColor("#1E3A8A");
                                    if (signBytes != null)
                                        c.Item().AlignRight().Width(54).Height(22)
                                            .Image(signBytes).FitArea();
                                    else
                                        c.Item().Height(22);
                                });

                                row.ConstantItem(52).AlignRight().Column(c =>
                                {
                                    c.Item().AlignCenter().Text("QR CHECK-IN")
                                        .SemiBold().FontSize(5.5f).FontColor("#1E3A8A");

                                    if (qrBytes != null)
                                    {
                                        c.Item().PaddingTop(2).AlignCenter().Width(42).Height(42)
                                            .Image(qrBytes).FitArea();
                                    }
                                    else
                                    {
                                        c.Item().PaddingTop(18).AlignCenter()
                                            .Text("QR unavailable").FontSize(6).FontColor("#94A3B8");
                                    }
                                });
                            });
                        });
                    });

                    // ===== FOOTER =====
                    page.Footer()
                        .BorderTop(1).BorderColor("#DBEAFE")
                        .PaddingTop(3)
                        .Row(row =>
                        {
                            row.RelativeItem().Text($"In ngay: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(6).FontColor("#64748B");
                            row.RelativeItem().AlignCenter().Text(x =>
                            {
                                x.Span("Trang ").FontSize(6).FontColor("#64748B");
                                x.CurrentPageNumber().FontSize(6).FontColor("#64748B");
                                x.Span(" / ").FontSize(6).FontColor("#64748B");
                                x.TotalPages().FontSize(6).FontColor("#64748B");
                            });
                            row.RelativeItem().AlignRight().Text("QuanLyDatVeMayBay")
                                .FontSize(6).FontColor("#64748B");
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}


