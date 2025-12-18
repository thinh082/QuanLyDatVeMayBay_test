using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.PDFDocument.Docs;
using QuanLyDatVeMayBay.Services;
using QuanLyDatVeMayBay.Services.PaypalService;
using QuanLyDatVeMayBay.Services.ThanhToanServices;
using QuanLyDatVeMayBay.Services.ThongBaoService;
using QuanLyDatVeMayBay.Services.VnpayServices;
using QuanLyDatVeMayBay.Services.VnpayServices.Enums;
using QuestPDF.Fluent;
using System.Security.Claims;
using VNPAY.NET.Models;
using VNPAY.NET.Utilities;

namespace QuanLyDatVeMayBay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelController : ControllerBase
    {
        private readonly ThinhContext _context;
        private readonly ThinhService _services;
        private readonly IThanhToanService _thanhToanService;
        private readonly IThongBaoService _thongBaoService;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IVnpay _vnpay;
        private readonly IPaypal _paypal;
        public HotelController(ThinhContext thinhContext, ThinhService thinhService, IThongBaoService thongBaoService, IBackgroundTaskQueue backgroundTaskQueue, IThanhToanService thanhToanService, IVnpay vnpay,IPaypal paypal)
        {
            _context = thinhContext;
            _services = thinhService;
            _thongBaoService = thongBaoService;
            _taskQueue = backgroundTaskQueue;
            _thanhToanService = thanhToanService;
            _vnpay = vnpay;
            _paypal = paypal;
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
        [HttpGet("GetAllHotel")]
        public IActionResult GetAllHotel()
        {
            var hotels = _context.Phongs.Select(r => new
            {
                r.Id,
                r.TenPhong,
                r.MoTa,
                r.Gia,
                r.Hinh,
            });
            return Ok(new
            {
                statusCode = 200,
                message = "Lấy danh sách khách sạn thành công!",
                data = hotels
            });
        }
        [HttpPost("GetHotelById")]
        public async Task<IActionResult> GetHotelById(long id)
        {
            var hotel = await _context.Phongs.FindAsync(id);
            if (hotel == null)
            {
                return Ok(new
                {
                    statusCode = 500,
                    message = "Khách sạn không tồn tại!"
                });
            }
            return Ok(new
            {
                statusCode = 200,
                message = "Lấy thông tin khách sạn thành công!",
                data = new
                {
                    hotel.Id,
                    hotel.TenPhong,
                    hotel.MoTa,
                    hotel.Gia,
                    hotel.Hinh,
                }
            });
        }
        [HttpPost]
        public async Task<dynamic> CheckPhong(long idTaiKhoan, long idPhong)
        {
            var checkDatPhong = await _context.DatPhongs
               .FirstOrDefaultAsync(r => r.IdTaiKhoan == idTaiKhoan && r.IdPhong == idPhong && r.TrangThai == 1);
            if (checkDatPhong != null)
            {
                return new
                {
                    statusCode = 500,
                    message = "Bạn đã đặt khách sạn này rồi.bạn có muốn đặt lại phòng này không?"
                };
            }
            return new
            {
                statusCode = 200,
                message = "Bạn có thể đặt khách sạn này!"
            };
        }
        //[HttpGet("create-payment")]
        //public IActionResult CreatePayment(double money, string description)
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
        //        return Created(paymentUrl, paymentUrl);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}
        [HttpPost("DatHotel")]
        [Authorize]
        public async Task<IActionResult> DatHotel(long idPhong, long idChiTietPhieuGiamGia, decimal gia, int selectedPayment, bool force = false)
        {
            var key = await _context.HashKeys.FirstOrDefaultAsync();
            if (selectedPayment == 0)
            {
                return Ok(new
                {
                    statusCode = 400,
                    message = "Phương thức thanh toán không hợp lệ!"
                });
            }
            
            var idTaiKhoanClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idTaiKhoanClaim == null || !long.TryParse(idTaiKhoanClaim.Value, out long idTaiKhoan))
            {
                return Unauthorized(new
                {
                    statusCode = 401,
                    message = "Người dùng chưa đăng nhập hoặc token không hợp lệ"
                });
            }
            if (idTaiKhoan <= 0)
            {
                return Ok(new
                {
                    statusCode = 500,
                    message = "ID tài khoản không hợp lệ!"
                });
            }
            var taiKhoan = await _context.TaiKhoans.FindAsync(idTaiKhoan);
            if (taiKhoan == null)
            {
                return Ok(new
                {
                    statusCode = 500,
                    message = "Tài khoản không tồn tại!"
                });
            }
            var phong = await _context.Phongs.FindAsync(idPhong);
            if (phong == null)
            {
                return Ok(new
                {
                    statusCode = 400,
                    message = "Khách sạn không tồn tại!"
                });
            }
            if (force == false)
            {
                var checkPhong = await CheckPhong(idTaiKhoan, idPhong);
                if (checkPhong.statusCode == 500)
                {
                    return Ok(new
                    {
                        statusCode = 502,
                        message = checkPhong.message
                    });
                }
            }
            if (selectedPayment == 2)
            {
                var maThanhToan = ThinhService.SinhMaNgauNhien("ThanhToanCho");
                var thanhToanCho = new ThanhToanCho
                {
                    MaThanhToanCho = maThanhToan,
                    IdTaiKhoan = idTaiKhoan,
                    SoTien = gia,
                    IdLoaiDichVu = 1, // hotel
                    IdCongThanhToan = 1, // PAYPAL
                    IdDichVu = idPhong,
                    IdChiTietPhieuGiamGia = idChiTietPhieuGiamGia,
                };
                _context.ThanhToanChos.Add(thanhToanCho);
                await _context.SaveChangesAsync();
                try
                {
                    var ipAddress = NetworkHelper.GetIpAddress(HttpContext);
                    var state = "FIXCUNGDATA";                   
                    var request = new PaymentRequest
                    {
                        PaymentId = thanhToanCho.Id , //DateTime.Now.Ticks,
                        Money = (double)gia,
                        Description = "Thanh toán khách sạn!",
                        IpAddress = ipAddress,
                        CreatedDate = DateTime.Now,
                        Currency = Currency.VND,
                        Language = DisplayLanguage.Vietnamese
                    };
                    var paymentUrl = _vnpay.GetPaymentUrl(request);
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
                    return BadRequest(new { message = ex.Message });
                }
            }else if(selectedPayment == 3)
            {
                string maThanhToan = ThinhService.SinhMaNgauNhien("ThanhToanCho");
                var thanhToanCho = new ThanhToanCho
                {
                    MaThanhToanCho = maThanhToan,
                    IdTaiKhoan = idTaiKhoan,
                    SoTien = gia,
                    IdLoaiDichVu = 1, // hotel
                    IdCongThanhToan = 2, // PAYPAL
                    IdDichVu = idPhong,
                    IdChiTietPhieuGiamGia = idChiTietPhieuGiamGia,
                };
                _context.ThanhToanChos.Add(thanhToanCho);
                await _context.SaveChangesAsync();

                try
                {
                    var state = Guid.NewGuid().ToString("N") +"|"+ idTaiKhoan.ToString()+"|"+maThanhToan;
                    var hashState = ThinhService.Encrypt(state, key.PublicKey);
                    var safeState = Uri.EscapeDataString(hashState);
                    var res = await _paypal.TaoDonHang(gia / 25000, safeState);
                    return Ok(new
                    {
                        statusCode = 202,
                        message = "Tạo đơn hàng thành công!",
                        url = res,
                        state = hashState
                    });
                }catch(Exception ex)
                {
                    Console.WriteLine($"❌ Lỗi khi tạo đơn hàng PayPal: {ex.Message}");
                    Console.WriteLine($"🔍 StackTrace: {ex.StackTrace}");
                    return BadRequest(new { message = ex.Message });
                }
            }

            var thanhToanResult = await _thanhToanService.ThanhToan(idTaiKhoan, gia, 1, "Hotel",1);
            if (thanhToanResult.statusCode != 200)
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
                var datPhong = new DatPhong
                {
                    IdTaiKhoan = idTaiKhoan,
                    IdPhong = idPhong,
                    NgayDat = DateTime.Now,
                    TrangThai = 1,
                    MaDatPhong = ThinhService.SinhMaNgauNhien("DatPhong")
                };
                _context.DatPhongs.Add(datPhong);
                var chiTietPhieuGiamGia = await _context.ChiTietPhieuGiamGia
                    .FirstOrDefaultAsync(c => c.IdTaiKhoan == idTaiKhoan && c.Id == idChiTietPhieuGiamGia);
                if (chiTietPhieuGiamGia != null)
                {
                    chiTietPhieuGiamGia.Active = true;
                    _context.ChiTietPhieuGiamGia.Update(chiTietPhieuGiamGia);
                }
                
                await _context.SaveChangesAsync();
                var email = taiKhoan.Email;
                var tieuDe = "Xác nhận đặt khách sạn thành công!";
                var noiDung =
                    $"<p>Bạn đã đặt khách sạn thành công. Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>" +
                    $"<p>Trân trọng,</p>" +
                    $"<p>Đội ngũ hỗ trợ khách hàng</p>";

                var qrCodeBytes = _services.GenerateQRCodeBytes(datPhong.MaDatPhong);
                _taskQueue.QueueBackgroundWorkItem(async (sp, ct) =>
                {
                    var emailService = sp.GetRequiredService<ThinhService>();
                    var thongBaoService = sp.GetRequiredService<IThongBaoService>();

                    var emailTask = emailService.GuiEmail_WithQRCoder(email, tieuDe, noiDung, qrCodeBytes);
                    var convertQr = Convert.ToBase64String(qrCodeBytes);
                    var thongBaoTask = thongBaoService.GuiThongBao(
                        idTaiKhoan,
                        "Bạn đã đặt khách sạn thành công. Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi! , Trân trọng , Đội ngũ hỗ trợ khách hàng",
                        "Xác nhận đặt khách sạn thành công!",
                        "data:image/png;base64," + convertQr
                    );

                    await Task.WhenAll(emailTask, thongBaoTask);
                });
                await transaction.CommitAsync();
                return Ok(new
                {
                    statusCode = 200,
                    message = "Đặt khách sạn thành công! Vui lòng kiểm tra email để nhận thông tin đặt khách sạn.",
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Ok(new
                {
                    statusCode = 500,
                    message = "Đặt khách sạn không thành công! Vui lòng thử lại.",
                    error = ex.Message
                });
            }


        }
        [HttpPost("InBill")]
        public async Task<IActionResult> InBill(long IdChiDinh)
        {

            var document = new PDFBill();
            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;
            var fileName = $"PhieuChiDinh_{IdChiDinh}.pdf";
            return File(stream.ToArray(), "application/pdf", fileName);
        }
    }
}