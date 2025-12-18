using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Config;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Models.Model;
using QuanLyDatVeMayBay.Services;
using QuanLyDatVeMayBay.Services.PaypalService;
using QuanLyDatVeMayBay.Services.ThanhToanServices;
using QuanLyDatVeMayBay.Services.ThongBaoService;
using StackExchange.Redis;
using System.Text.Json;
using VNPAY.NET.Models;
namespace QuanLyDatVeMayBay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayPalController : ControllerBase
    {
        private readonly IPaypal _paypal;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IThanhToanService _thanhToanService;
        private readonly ThinhContext _context;
        private readonly ThinhService _services;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IConnectionMultiplexer _redis;
        
        public PayPalController(IPaypal paypal, IHubContext<NotificationHub> hubContext,ThinhContext thinhContext,IThanhToanService thanhToanService,ThinhService thinhService,IBackgroundTaskQueue backgroundTaskQueue, IConnectionMultiplexer redis)
        {
            _paypal = paypal;
            _hubContext = hubContext;
            _context = thinhContext;
            _thanhToanService = thanhToanService;
            _services = thinhService;
            _taskQueue = backgroundTaskQueue;
            _redis = redis;
        }
        [HttpGet("capture-order")]
        public async Task<IActionResult> payPalCallback(string token, string state)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token không hợp lệ." });

            if (string.IsNullOrEmpty(state))
                return BadRequest(new { message = "State không hợp lệ." });

            var res = await _paypal.CaptureOrder(token);
            if (!string.IsNullOrEmpty(state))
            {
                    var key = await _context.HashKeys.FirstOrDefaultAsync();
                    var hashState = Uri.UnescapeDataString(state);
                    var giaiMaState = ThinhService.Decrypt(hashState, key.PrivateKey);
                    var parts = giaiMaState.Split('|');
                    var idTaiKhoan = long.Parse(parts[1]);
                    var maThanhToan = parts[2];
                    var thanhToanCho = await _context.ThanhToanChos.FirstOrDefaultAsync(r => r.IdTaiKhoan == idTaiKhoan && r.MaThanhToanCho == maThanhToan);
                    
                    if (thanhToanCho == null)
                    {
                        return BadRequest(new { statusCode = 404, message = "Không tìm thấy giao dịch thanh toán!" });
                    }

                    // Kiểm tra loại dịch vụ - chỉ xử lý vé máy bay
                    if (thanhToanCho.IdLoaiDichVu != 2) // Không phải Flight
                    {
                        return BadRequest(new { statusCode = 400, message = "Callback này chỉ xử lý đặt vé máy bay!" });
                    }

                    // Lấy dữ liệu từ Redis
                    var db = _redis.GetDatabase();
                    var redisKey = $"DatVe_{thanhToanCho.MaThanhToanCho}";
                    var jsonData = await db.StringGetAsync(redisKey);
                    
                    if (string.IsNullOrEmpty(jsonData))
                    {
                        return BadRequest(new { statusCode = 404, message = "Giao dịch đã hết hạn hoặc không tồn tại!" });
                    }

                    var datVeRedis = JsonSerializer.Deserialize<DatVeRedisModel>(jsonData);

                    // Gọi ThanhToanService để tạo lịch sử thanh toán
                    var thanhToan = await _thanhToanService.ThanhToan(
                        thanhToanCho.IdTaiKhoan, 
                        thanhToanCho.SoTien, 
                        2, // PayPal
                        "Flight", 
                        thanhToanCho.IdCongThanhToan.Value);

                    if (thanhToan.statusCode != 200)
                    {
                        return Ok(new
                        {
                            statusCode = 500,
                            message = "Thanh toán không thành công. Vui lòng thử lại."
                        });
                    }

                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        // Tạo DatVe
                        var datVe = new DatVe
                        {
                            IdTaiKhoan = thanhToanCho.IdTaiKhoan,
                            IdChuyenBay = datVeRedis.IdChuyenBay,
                            NgayDat = DateTime.Now,
                            IdVe = null,
                            IdLichBay = datVeRedis.IdLichBay,
                            LichBayId = datVeRedis.IdLichBay,
                            Gia = datVeRedis.Gia
                        };
                        _context.DatVes.Add(datVe);
                        await _context.SaveChangesAsync();

                    // Tạo ChiTietDatVe cho từng ghế
                    foreach (var idGhe in datVeRedis.IdGheNgois)
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
                            FirstOrDefaultAsync(r => r.IdGheNgoi == idGhe && r.IdLichBay == datVeRedis.IdLichBay && r.IdGheNgoiNavigation.IdChuyenBay == datVeRedis.IdChuyenBay);
                        if (gheNgoiLichBay != null)
                        {
                            // Cập nhật trạng thái ghế trong lịch bay
                            gheNgoiLichBay.TrangThai = 2; // Đã đặt
                            _context.GheNgoiLichBays.Update(gheNgoiLichBay);
                        }
                        _context.GheNgois.Update(ghe);
                    }

                    // Xử lý phiếu giảm giá
                    if (datVeRedis.IdChiTietPhieuGiamGia.HasValue)
                        {
                            var chiTietPhieuGiamGia = await _context.ChiTietPhieuGiamGia
                                .FirstOrDefaultAsync(c => c.Id == datVeRedis.IdChiTietPhieuGiamGia);
                            if (chiTietPhieuGiamGia != null)
                            {
                                chiTietPhieuGiamGia.Active = true;
                                _context.ChiTietPhieuGiamGia.Update(chiTietPhieuGiamGia);
                            }
                        }

                        await _context.SaveChangesAsync();

                        // Gửi email và thông báo
                        var taiKhoan = await _context.TaiKhoans.FindAsync(thanhToanCho.IdTaiKhoan);
                        var email = taiKhoan.Email;
                        var tieuDe = "Xác nhận đặt vé máy bay thành công!";
                        var noiDung =
                            $"<p>Bạn đã đặt vé máy bay thành công. Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>" +
                            $"<p>Mã đặt vé: {datVe.Id}</p>" +
                            $"<p>Trân trọng,</p>" +
                            $"<p>Đội ngũ hỗ trợ khách hàng</p>";

                        var qrCodeBytes = _services.GenerateQRCodeBytes(datVe.Id.ToString());
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

                        // Gửi SignalR notification
                        await _hubContext.Clients.Group(state).SendAsync("ReceivePaymentResult_PayPal", new
                        {
                            statusCode = 200,
                            message = "Đặt vé máy bay thành công",
                            data = new
                            {
                                IdDatVe = datVe.Id,
                                SoGhe = datVeRedis.IdGheNgois.Count,
                                Gia = thanhToanCho.SoTien
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new
                        {
                            statusCode = 500,
                            message = "Đặt vé không thành công! Vui lòng thử lại.",
                            error = ex.Message
                        });
                    }
                }

                var htmlContent = @"
                    <html>
                      <head>
                        <title>Thanh toán PayPal - Đặt vé máy bay</title>
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <style>
                          body {
                            font-family: Arial, sans-serif;
                            background-color: #f5f5f5;
                            margin: 0;
                            display: flex;
                            justify-content: center;
                            align-items: center;
                            height: 100vh;
                            text-align: center;
                          }
                          .card {
                            background-color: #fff;
                            border-radius: 12px;
                            box-shadow: 0 4px 15px rgba(0,0,0,0.1);
                            padding: 2rem;
                            max-width: 400px;
                            width: 90%;
                          }
                          h1 {
                            color: #4CAF50;
                            font-size: 1.8rem;
                            margin-bottom: 1rem;
                          }
                          p {
                            color: #555;
                            font-size: 1rem;
                            margin: 0.5rem 0;
                          }
                          .info {
                            background-color: #f8f9fa;
                            padding: 1rem;
                            border-radius: 8px;
                            margin: 1rem 0;
                          }
                          @media (max-width: 480px) {
                            h1 { font-size: 1.5rem; }
                            p { font-size: 0.9rem; }
                          }
                        </style>
                      </head>
                      <body>
                        <div class=""card"">
                          <h1>✈️ Đặt vé thành công!</h1>
                          <div class=""info"">
                            <p><strong>Phương thức:</strong> PayPal</p>
                            <p><strong>Trạng thái:</strong> Đã thanh toán</p>
                          </div>
                          <p>Bạn có thể quay lại ứng dụng để tiếp tục.</p>
                          <p>Trang này sẽ tự đóng sau 5 giây...</p>
                        </div>
                        <script>
                          setTimeout(() => {
                            window.close();
                          }, 5000);
                        </script>
                      </body>
                    </html>
                    ";
                return Content(htmlContent, "text/html;charset=utf-8");
            }
        }
    }

