using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Config;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Models.Model;
using QuanLyDatVeMayBay.Services;
using QuanLyDatVeMayBay.Services.ThanhToanServices;
using QuanLyDatVeMayBay.Services.ThongBaoService;
using QuanLyDatVeMayBay.Services.VnpayServices;
using QuanLyDatVeMayBay.Services.VnpayServices.Enums;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using VNPAY.NET.Models;
using VNPAY.NET.Utilities;

namespace QuanLyDatVeMayBay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VnpayController : ControllerBase
    {
        private readonly IVnpay _vnpay;
        private readonly ThinhContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IThanhToanService _thanhToanService;
        private readonly ThinhService _services;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IConnectionMultiplexer _redis;

        public VnpayController(IVnpay vnpay,ThinhContext thinhContext, IHubContext<NotificationHub> hubContext,IThanhToanService thanhToanService,ThinhService thinhService,IBackgroundTaskQueue backgroundTaskQueue, IConnectionMultiplexer redis)
        {
            _vnpay = vnpay;
            _context = thinhContext;
            _hubContext = hubContext;
            _thanhToanService = thanhToanService;
            _services = thinhService;
            _taskQueue = backgroundTaskQueue;
            _redis = redis;
            var setting = _context.Vnpaysettings.FirstOrDefault();
            if (setting == null)
            {
                throw new Exception("Chưa cấu hình VNPAY trong hệ thống");
            }
            _vnpay.Initialize(setting.TmnCode,
                setting.HashSecret,
                setting.CallBackUrl,
                setting.BaseUrl);
        }        
        ///
        //[HttpGet("create-payment")]
        //public IActionResult CreatePayment(double money,string description)
        //{
        //    try
        //    {
        //        var ipAddress = NetworkHelper.GetIpAddress(HttpContext);
        //        var request = new PaymentRequest
        //        {
        //            PaymentId = DateTime.Now.Ticks,
        //            Money = money,
        //            Description = description,
        //            IpAddress = ipAddress,
        //            CreatedDate = DateTime.Now,
        //            Currency = Currency.VND,
        //            Language = DisplayLanguage.Vietnamese
        //        };
        //        var paymentUrl = _vnpay.GetPaymentUrl(request);
        //        return Created(paymentUrl,paymentUrl);
        //    } catch(Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}
        [HttpGet("IpnAction")]
        public IActionResult IpnAction()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                    if(paymentResult.IsSuccess)
                    {
                        //Thanh toán thành công
                        //Cập nhật trạng thái đơn hàng trong hệ thống
                        return Ok(new { message = "Thanh toán thành công"});
                    }
                    else
                    {
                        //Thanh toán không thành công
                        return BadRequest(new { message = "Thanh toán thất bại" });
                    }
                }catch(Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }   
            }
            return NotFound("Không tìm thấy thông tin thanh toán!");
        }
        [HttpGet("callback")]
        public async Task<IActionResult> Callback()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                    var thanhToanCho = await _context.ThanhToanChos.FindAsync(paymentResult.PaymentId);
                    
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
                            2, // VnPay
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
                                IdLichBay = datVeRedis.IdLichBay,
                                LichBayId = datVeRedis.IdLichBay,
                                NgayDat = DateTime.Now,
                                IdVe = null,
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
                            var state = "FIXCUNGDATA";
                            if (!string.IsNullOrEmpty(state))
                            {
                                await _hubContext.Clients.Group(state).SendAsync("ReceivePaymentResult", new
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

                            var htmlContent = @"
                        <html>
                          <head>
                            <title>Thanh toán VNPAY - Đặt vé máy bay</title>
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
                                <p><strong>Mã đặt vé:</strong> " + datVe.Id + @"</p>
                                <p><strong>Số ghế:</strong> " + datVeRedis.IdGheNgois.Count + @"</p>
                                <p><strong>Tổng tiền:</strong> " + thanhToanCho.SoTien.ToString("N0") + @" VNĐ</p>
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
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();

                        // Lấy thông tin chi tiết lỗi
                        var detailedError = new StringBuilder();
                        detailedError.AppendLine($"[Error Message] {ex.Message}");
                        detailedError.AppendLine($"[Stack Trace] {ex.StackTrace}");

                        // Duyệt qua inner exception (nếu có)
                        var inner = ex.InnerException;
                        int depth = 1;
                        while (inner != null)
                        {
                            detailedError.AppendLine($"[Inner Exception {depth}] {inner.Message}");
                            detailedError.AppendLine(inner.StackTrace);
                            inner = inner.InnerException;
                            depth++;
                        }

                        // ⚙️ Ghi log ra file hoặc console (tuỳ môi trường)
                        Console.WriteLine("=== ERROR LOG (Đặt vé) ===");
                        Console.WriteLine(detailedError.ToString());

                        // 🚀 Trả kết quả thân thiện cho client
                        return Ok(new
                        {
                            statusCode = 500,
                            message = "Đặt vé không thành công! Vui lòng thử lại.",
#if DEBUG
                            // ⚠️ Trong môi trường dev bạn có thể trả chi tiết
                            detailedError = detailedError.ToString()
#endif
                        });
                    }

                }
                catch (Exception ex)
                {
                    return BadRequest(new { statusCode = 500, message = ex.Message });
                }
            }
            return NotFound("Không tìm thấy thông tin thanh toán!");
        }
    }
}
