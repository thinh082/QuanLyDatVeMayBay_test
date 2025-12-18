using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using QuanLyDatVeMayBay.Config;
using QuanLyDatVeMayBay.Models.Model;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Services.ChuyenBayService;
using QuanLyDatVeMayBay.Services.ThanhToanServices;
using QuanLyDatVeMayBay.Services.VnpayServices;
using QuanLyDatVeMayBay.Services.PaypalService;
using QuanLyDatVeMayBay.Services;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text.Json;
using VNPAY.NET.Models;
using VNPAY.NET.Utilities;
using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Services.VnpayServices.Enums;
using Microsoft.AspNetCore.Authorization;
using Azure.Core;
using QuanLyDatVeMayBay.Services.ThongBaoService;
using System.Linq;

namespace QuanLyDatVeMayBay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChuyenBayController : ControllerBase
    {
        private readonly IChuyenBayService _services;
        private readonly IConnectionMultiplexer _redis;
        private readonly ThinhContext _context;
        private readonly IThanhToanService _thanhToanService;
        private readonly IVnpay _vnpay;
        private readonly ThinhService _ThinhServices;
        private readonly IPaypal _paypal;
        private readonly IBackgroundTaskQueue _taskQueue;

        public ChuyenBayController(
            IChuyenBayService chuyenBayService,
            IConnectionMultiplexer redis,
            ThinhContext context,
            IThanhToanService thanhToanService,
            IVnpay vnpay,
            IPaypal paypal,
            ThinhService thinhService,
            IBackgroundTaskQueue taskQueue)
        {
            _services = chuyenBayService;
            _redis = redis;
            _context = context;
            _thanhToanService = thanhToanService;
            _vnpay = vnpay;
            _paypal = paypal;
            _ThinhServices = thinhService;
            _taskQueue = taskQueue;
        }
        [HttpPost("LayDanhSachChuyenBay")]
        public async Task<IActionResult> LayDanhSachChuyenBay([FromBody] TimChuyenBayRequest model)
        {
            var result = await _services.LayDanhSachChuyenBay(model);
            return Ok(result);
        }
        [HttpPost("DanhSachGheTheoChuyenBay")]
        public async Task<IActionResult> DanhSachGheTheoChuyenBay([FromQuery] long idLichBay, [FromQuery] long idTuyenBay)
        {
            var result = await _services.DanhSachGheTheoChuyenBay(idLichBay, idTuyenBay);

            return Ok(result);
        }
        [HttpPost("SetTrangThaiGheNgoi")]
        public async Task<IActionResult> SetTrangThaiGheNgoi([FromBody] SetGheNgoiModel setGhe)
        {
            // Lấy thông tin user từ JWT
            var idTaiKhoanClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idTaiKhoanClaim == null || !long.TryParse(idTaiKhoanClaim.Value, out long idTaiKhoan))
            {
                return Unauthorized(new { statusCode = 401, message = "Người dùng chưa đăng nhập hoặc token không hợp lệ" });
            }
            var result = await _services.SetGheNgoi(setGhe.idGheNgoi, idTaiKhoan, setGhe.idLichBay);
            return Ok(result);
        }
        [HttpPost("ReleaseSeat")]
        public async Task<IActionResult> ReleaseSeat([FromBody] SetGheNgoiModel model)
        {
            // Lấy thông tin user từ JWT
            var idTaiKhoanClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idTaiKhoanClaim == null || !long.TryParse(idTaiKhoanClaim.Value, out long idTaiKhoan))
            {
                return Unauthorized(new { statusCode = 401, message = "Người dùng chưa đăng nhập hoặc token không hợp lệ" });
            }
            var result = await _services.ReleaseSeat(
                model.idGheNgoi, idTaiKhoan, model.idLichBay);

            return Ok(result);
        }



        [HttpPost("DatVe")]
        [Authorize]
        public async Task<IActionResult> DatVe([FromBody] DatVeRequest request)
        {
            // Validation đầu vào
            if (request == null || request.IdGheNgois == null || !request.IdGheNgois.Any())
            {
                return BadRequest(new { statusCode = 400, message = "Dữ liệu không hợp lệ" });
            }

            if (request.SelectedPayment == 0)
            {
                return BadRequest(new { statusCode = 400, message = "Phương thức thanh toán không hợp lệ!" });
            }

            // Lấy thông tin user từ JWT
            var idTaiKhoanClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idTaiKhoanClaim == null || !long.TryParse(idTaiKhoanClaim.Value, out long idTaiKhoan))
            {
                return Unauthorized(new { statusCode = 401, message = "Người dùng chưa đăng nhập hoặc token không hợp lệ" });
            }

            // Validation tài khoản
            var taiKhoan = await _context.TaiKhoans.FindAsync(idTaiKhoan);
            if (taiKhoan == null)
            {
                return BadRequest(new { statusCode = 404, message = "Tài khoản không tồn tại!" });
            }

            // Validation chuyến bay
            var chuyenBay = await _context.ChuyenBays.FindAsync(request.IdChuyenBay);
            if (chuyenBay == null)
            {
                return BadRequest(new { statusCode = 404, message = "Chuyến bay không tồn tại!" });
            }

            // Validation ghế ngồi
            // Validation ghế ngồi theo IdLichBay
            var gheNgois = await _context.GheNgoiLichBays
                .Where(g => request.IdGheNgois.Contains(g.IdGheNgoi.Value)
                        && g.IdLichBay == request.IdLichBay)
                .Select(g => new
                {
                    g.IdGheNgoi,
                    g.IdLichBay,
                    g.TrangThai,
                    SoGhe = g.IdGheNgoiNavigation.SoGhe
                })
                .ToListAsync();

            if (gheNgois.Count != request.IdGheNgois.Count)
            {
                return BadRequest(new { statusCode = 400, message = "Một số ghế không tồn tại hoặc không thuộc lịch bay này!" });
            }

            // Kiểm tra ghế đã được đặt chưa
            var gheDaDat = gheNgois.Where(g => g.TrangThai == 2).ToList();
            if (gheDaDat.Any())
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    message = $"Ghế {string.Join(", ", gheDaDat.Select(g => g.SoGhe))} đã được đặt!"
                });
            }


            var db = _redis.GetDatabase();
            var maThanhToan = ThinhService.SinhMaNgauNhien("ThanhToanCho");

            // Tạo model Redis
            var datVeRedis = new DatVeRedisModel
            {
                IdTaiKhoan = idTaiKhoan,
                IdChuyenBay = request.IdChuyenBay,
                IdTuyenBay = request.IdTuyenBay,
                IdGheNgois = request.IdGheNgois,
                IdLichBay = request.IdLichBay,
                IdChiTietPhieuGiamGia = request.IdChiTietPhieuGiamGia,
                Gia = request.Gia,
                SelectedPayment = request.SelectedPayment,
                CreatedAt = DateTime.Now,
                MaThanhToanCho = maThanhToan
            };

            // Lưu vào Redis với TTL 15 phút
            var redisKey = $"DatVe_{maThanhToan}";
            var jsonData = JsonSerializer.Serialize(datVeRedis);
            await db.StringSetAsync(redisKey, jsonData, TimeSpan.FromMinutes(15));

            // Tạo ThanhToanCho
            var thanhToanCho = new ThanhToanCho
            {
                MaThanhToanCho = maThanhToan,
                IdTaiKhoan = idTaiKhoan,
                SoTien = request.Gia,
                IdLoaiDichVu = 2,
                IdCongThanhToan = request.SelectedPayment == 2 ? 1 : 2,
                IdDichVu = request.IdChuyenBay, 
                IdChiTietPhieuGiamGia = request.IdChiTietPhieuGiamGia,
                IdTrangThai = 1 
            };

            _context.ThanhToanChos.Add(thanhToanCho);
            await _context.SaveChangesAsync();

            // Xử lý thanh toán theo selectedPayment
            if (request.SelectedPayment == 2) 
            {
                try
                {
                    var key = await _context.HashKeys.FirstOrDefaultAsync();
                    var setting = _context.Vnpaysettings.FirstOrDefault();
                    if (setting == null)
                    {
                        return BadRequest(new { statusCode = 500, message = "Chưa cấu hình VNPAY trong hệ thống" });
                    }

                    _vnpay.Initialize(setting.TmnCode, setting.HashSecret, setting.CallBackUrl, setting.BaseUrl);

                    var ipAddress = NetworkHelper.GetIpAddress(HttpContext);
                    var state = "FIXCUNGDATA";
                    var requestVnpay = new PaymentRequest
                    {
                        PaymentId = thanhToanCho.Id,
                        Money = (double)request.Gia,
                        Description = "Thanh toán vé máy bay!",
                        IpAddress = ipAddress,
                        CreatedDate = DateTime.Now,
                        Currency = Currency.VND,
                        Language = DisplayLanguage.Vietnamese
                    };
                    var paymentUrl = _vnpay.GetPaymentUrl(requestVnpay);
                    return Ok(new
                    {
                        statusCode = 201,
                        message = "Tạo đơn hàng thành công!",
                        url = paymentUrl,
                        state = state
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { statusCode = 500, message = ex.Message });
                }
            }
            else if (request.SelectedPayment == 3) 
            {
                try
                {
                    var key = await _context.HashKeys.FirstOrDefaultAsync();
                    if (key == null)
                    {
                        return BadRequest(new { statusCode = 500, message = "Chưa cấu hình mã hóa trong hệ thống" });
                    }

                    var state = Guid.NewGuid().ToString("N") + "|" + idTaiKhoan.ToString() + "|" + maThanhToan;
                    var hashState = ThinhService.Encrypt(state, key.PublicKey);
                    var safeState = Uri.EscapeDataString(hashState);
                    var res = await _paypal.TaoDonHang(request.Gia / 25000, safeState);
                    return Ok(new
                    {
                        statusCode = 202,
                        message = "Tạo đơn hàng thành công!",
                        url = res,
                        state = hashState
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Lỗi khi tạo đơn hàng PayPal: {ex.Message}");
                    Console.WriteLine($"🔍 StackTrace: {ex.StackTrace}");
                    return BadRequest(new { statusCode = 500, message = ex.Message });
                }
            }
            else // Thanh toán trực tiếp
            {
                var thanhToanResult = await _thanhToanService.ThanhToan(idTaiKhoan, request.Gia, 1, "Flight", 1);
                if (thanhToanResult.statusCode != 200)
                {
                    return BadRequest(new { statusCode = 500, message = "Thanh toán không thành công. Vui lòng thử lại." });
                }

                // Xử lý đặt vé thành công
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Tạo DatVe
                    var datVe = new DatVe
                    {
                        IdTaiKhoan = idTaiKhoan,
                        IdChuyenBay = request.IdChuyenBay,
                        LichBayId = request.IdLichBay,
                        NgayDat = DateTime.Now,
                        IdVe = null,
                        TrangThai = "đã đặt",
                        Gia = request.Gia
                    };
                    _context.DatVes.Add(datVe);
                    await _context.SaveChangesAsync();

                    // Tạo ChiTietDatVe cho từng ghế
                    foreach (var idGhe in request.IdGheNgois)
                    {
                        var ghe = await _context.GheNgois.FindAsync(idGhe);
                        var chiTietDatVe = new ChiTietDatVe
                        {
                            IdDatVe = datVe.Id,
                            IdGheNgoi = idGhe,
                            //Gia = ghe.Gia ?? 0
                        };
                        _context.ChiTietDatVes.Add(chiTietDatVe);
                        var gheNgoiLichBay = await _context.GheNgoiLichBays.
                            FirstOrDefaultAsync(r=>r.IdGheNgoi == idGhe && r.IdLichBay == request.IdLichBay && r.IdGheNgoiNavigation.IdChuyenBay == request.IdChuyenBay);
                        if(gheNgoiLichBay != null)
                        {
                            // Cập nhật trạng thái ghế trong lịch bay
                            gheNgoiLichBay.TrangThai = 2; // Đã đặt
                            _context.GheNgoiLichBays.Update(gheNgoiLichBay);
                        }
                        _context.GheNgois.Update(ghe);
                    }

                    // Xử lý phiếu giảm giá
                    if (request.IdChiTietPhieuGiamGia.HasValue)
                    {
                        var chiTietPhieuGiamGia = await _context.ChiTietPhieuGiamGia
                            .FirstOrDefaultAsync(c => c.Id == request.IdChiTietPhieuGiamGia);
                        if (chiTietPhieuGiamGia != null)
                        {
                            chiTietPhieuGiamGia.Active = true;
                            _context.ChiTietPhieuGiamGia.Update(chiTietPhieuGiamGia);
                        }
                    }

                    await _context.SaveChangesAsync();
                    var email = taiKhoan.Email;
                    var tieuDe = "Xác nhận đặt vé máy bay thành công!";
                    var noiDung =
                        $"<p>Bạn đã đặt vé máy bay thành công. Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>" +
                        $"<p>Mã đặt vé: {datVe.Id}</p>" +
                        $"<p>Trân trọng,</p>" +
                        $"<p>Đội ngũ hỗ trợ khách hàng</p>";

                    var qrCodeBytes = _ThinhServices.GenerateQRCodeBytes(datVe.Id.ToString());
                    _taskQueue.QueueBackgroundWorkItem(async (sp, ct) =>
                    {
                        var emailService = sp.GetRequiredService<ThinhService>();
                        var thongBaoService = sp.GetRequiredService<IThongBaoService>();

                        var emailTask = emailService.GuiEmail_WithQRCoder(email, tieuDe, noiDung, qrCodeBytes);
                        var convertQr = Convert.ToBase64String(qrCodeBytes);
                        var thongBaoTask = thongBaoService.GuiThongBao(
                            thanhToanCho.IdTaiKhoan,
                            "Bạn đã đặt vé máy bay thành công. Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!",
                            "Xác nhận đặt vé máy bay thành công!",
                            "data:image/png;base64," + convertQr
                        );

                        await Task.WhenAll(emailTask, thongBaoTask);
                    });
                    await transaction.CommitAsync();

                    // Xóa khỏi Redis
                    await db.KeyDeleteAsync(redisKey);

                    return Ok(new
                    {
                        statusCode = 200,
                        message = "Đặt vé thành công! Vui lòng kiểm tra email để nhận thông tin vé.",
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { statusCode = 500, message = "Đặt vé không thành công! Vui lòng thử lại.", error = ex.Message });
                }
            }
        }
        [HttpPost("HuyDatVe")]
        public async Task<IActionResult> HuyDatVe([FromQuery] long idDatVe, [FromQuery] string lyDoHuy)
        {
            var result = await _services.HuyVe(idDatVe, lyDoHuy);
            return Ok(result);
        }
        [HttpPost("CheckIn")]
        public async Task<IActionResult> CheckIn(string id)
        {
            var result = await _services.CheckIn(id);
            return Ok(result);
        }
    }
}
