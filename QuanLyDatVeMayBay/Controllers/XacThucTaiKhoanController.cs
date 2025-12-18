using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using QuanLyDatVeMayBay.Config;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Models.Model;
using QuanLyDatVeMayBay.Services;
using QuanLyDatVeMayBay.Services.FacebookService;
using QuanLyDatVeMayBay.Services.GoogleService;
using QuanLyDatVeMayBay.Services.XacThucServices;
using QuanLyDatVeMayBay.Services.ZaloService;
using System.Security.Claims;

namespace QuanLyDatVeMayBay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class XacThucTaiKhoanController : ControllerBase
    {
        private readonly IXacThucTaiKhoanServices _services;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IGoogle _google;
        private readonly IFaceBook _facebook;
        private readonly IZalo _zalo;

        public XacThucTaiKhoanController(IXacThucTaiKhoanServices services,IGoogle google,IHubContext<NotificationHub> hubContext,IFaceBook faceBook,IZalo zalo)
        {
            _services = services;
            _google = google;
            _hubContext = hubContext;
            _facebook = faceBook;
            _zalo = zalo;
        }

        [HttpPost("DangKy")]
        public async Task<IActionResult> DangKy([FromBody] DangKyRequest request)
        {
            
                if (!ModelState.IsValid)
                {
                    return StatusCode(400, new
                    {
                        statusCode = 400,
                        message = "Dữ liệu không hợp lệ",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }
                var result = await _services.DangKyAsync(request);
            return Ok(result);

        }
        
     [HttpPost("QuenMatKhau")]
        public async Task<IActionResult> QuenMatKhau(string? email)
        {
            var result = await _services.QuenMatKhauAsync(email);
            return Ok(result);
        }

        // --------- XÁC NHẬN OTP ---------
        [HttpPost("XacNhanOtp")]
        public async Task<IActionResult> XacNhanOtp([FromQuery] string ?email, [FromQuery] string ?otp)
        {
            var result = await _services.XacNhanOtpAsync(email, otp);
            return Ok(result);
        }
        [HttpPost("DoiMatKhau")]
        public async Task<IActionResult> DoiMatKhau([FromQuery] string ?email, [FromQuery] string ?matKhau, [FromQuery] string? xacNhanMatKhau)
        {
            var result = await _services.DoiMatKhauAsync(email, matKhau, xacNhanMatKhau);
            return Ok(result);
        }
        [HttpPost("dangnhap")]
        public async Task<IActionResult> DangNhap([FromBody] DangNhapRequest request)
        {
            var result = await _services.DangNhap(request.TaiKhoan, request.MatKhau);
           
            return Ok(result);
        }

        // API set thời gian khóa
        [HttpPost("set-thoigian-khoa")]
        public async Task<IActionResult> SetThoiGianKhoa([FromBody] int idTaiKhoan)
        {
            var result = await _services.SetThoiGianKhoa(idTaiKhoan);
            return Ok(result);
        }
        [HttpGet("Create_url_google")]
        public async Task<IActionResult> CreateUrlGoogle( string? state)
        {
            var url = await _google.GenUrl(state);
            if (url == null) return StatusCode(500, new
            {
                statusCode = 500,
                message = "Lỗi hệ thống"
            });
            return Ok(new
            {
                statusCode = 200,
                message = "Thành công",
                url = url
            });
        }
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(string code, string? state =null)
        {
            var result = await _google.GoogleCallback(code);
            if (!string.IsNullOrEmpty(state))
            {
               await _hubContext.Clients.Group(state).SendAsync("ReceiveGoogleAuthResult", new
               { result });
            }
            var htmlContent = @"
<!DOCTYPE html>
<html lang='vi'>
  <head>
    <meta charset='UTF-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0' />
    <title>Đăng nhập Google thành công</title>
    <style>
      * {
        box-sizing: border-box;
        margin: 0;
        padding: 0;
      }

      body {
        font-family: 'Segoe UI', Roboto, Arial, sans-serif;
        background: linear-gradient(135deg, #f9fafb, #eef2ff);
        display: flex;
        justify-content: center;
        align-items: center;
        height: 100vh;
        padding: 16px;
      }

      .card {
        background-color: #fff;
        border-radius: 16px;
        box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
        padding: 24px 20px;
        max-width: 360px;
        width: 100%;
        text-align: center;
        animation: fadeIn 0.5s ease;
      }

      h1 {
        color: #16a34a;
        font-size: 1.3rem;
        margin-bottom: 8px;
      }

      p {
        color: #555;
        font-size: 0.95rem;
        line-height: 1.4;
      }

      @keyframes fadeIn {
        from { opacity: 0; transform: translateY(10px); }
        to { opacity: 1; transform: translateY(0); }
      }

      @media (max-width: 480px) {
        h1 { font-size: 1.2rem; }
        p { font-size: 0.9rem; }
      }
    </style>
  </head>
  <body>
    <div class='card'>
      <h1>🎉 Đăng nhập Google thành công!</h1>
      <p>Bạn có thể quay lại ứng dụng để tiếp tục.</p>
      <p>Trang này sẽ tự đóng sau 3 giây...</p>
    </div>
    <script>
      setTimeout(() => window.close(), 3000);
    </script>
  </body>
</html>";


            return Content(htmlContent, "text/html;charset=utf-8");
        }
        [HttpGet("Create_url_facebook")]
        public async Task<IActionResult> CreateUrlFacebook(string? state)
        {
            var result = await _facebook.GenUrl(state);
            return Ok(new
            {
                statusCode = 200,
                message = "Thành công",
                url = result
            });
        }
        [HttpGet("facebook-callback")]
        public async Task<IActionResult> FacebookCallback(string code, string? state = null)
        {
            var result = await _facebook.XuLyCallBackFb(code, state);
            if (!string.IsNullOrEmpty(state))
            {
                await _hubContext.Clients.Group(state).SendAsync("ReceiveFacebookAuthResult", new
                { result });
            }
            var htmlContent = @"
<!DOCTYPE html>
<html lang='vi'>
  <head>
    <meta charset='UTF-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0' />
    <title>Đăng nhập Google thành công</title>
    <style>
      * {
        box-sizing: border-box;
        margin: 0;
        padding: 0;
      }

      body {
        font-family: 'Segoe UI', Roboto, Arial, sans-serif;
        background: linear-gradient(135deg, #f9fafb, #eef2ff);
        display: flex;
        justify-content: center;
        align-items: center;
        height: 100vh;
        padding: 16px;
      }

      .card {
        background-color: #fff;
        border-radius: 16px;
        box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
        padding: 24px 20px;
        max-width: 360px;
        width: 100%;
        text-align: center;
        animation: fadeIn 0.5s ease;
      }

      h1 {
        color: #16a34a;
        font-size: 1.3rem;
        margin-bottom: 8px;
      }

      p {
        color: #555;
        font-size: 0.95rem;
        line-height: 1.4;
      }

      @keyframes fadeIn {
        from { opacity: 0; transform: translateY(10px); }
        to { opacity: 1; transform: translateY(0); }
      }

      @media (max-width: 480px) {
        h1 { font-size: 1.2rem; }
        p { font-size: 0.9rem; }
      }
    </style>
  </head>
  <body>
    <div class='card'>
      <h1>🎉 Đăng nhập Facebook thành công!</h1>
      <p>Bạn có thể quay lại ứng dụng để tiếp tục.</p>
      <p>Trang này sẽ tự đóng sau 3 giây...</p>
    </div>
    <script>
      setTimeout(() => window.close(), 3000);
    </script>
  </body>
</html>";


            return Content(htmlContent, "text/html;charset=utf-8");
        }
        [HttpGet("Create_url_zalo")]
        public async Task<IActionResult> CreateUrlZalo(string? state)
        {
            if(state == null) state = Guid.NewGuid().ToString("N");
            var url = await _zalo.GenUrl(state);
            if (url == null) return StatusCode(500, new
            {
                statusCode = 500,
                message = "Lỗi hệ thống"
            });
            return Ok(new
            {
                statusCode = 200,
                message = "Thành công",
                url = url
            });
        }
        [HttpGet("zalo-callback")]
        public async Task<IActionResult> ZaloCallback(string code, string state)
        {
            var result = await _zalo.XuLySauCallBack(code, state);
            if (!string.IsNullOrEmpty(state))
            {
                await _hubContext.Clients.Group(state).SendAsync("ReceiveZaloAuthResult", new
                { result });
            }
            var htmlContent = @"
<!DOCTYPE html>
<html lang='vi'>
  <head>
    <meta charset='UTF-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0' />
    <title>Đăng nhập Google thành công</title>
    <style>
      * {
        box-sizing: border-box;
        margin: 0;
        padding: 0;
      }

      body {
        font-family: 'Segoe UI', Roboto, Arial, sans-serif;
        background: linear-gradient(135deg, #f9fafb, #eef2ff);
        display: flex;
        justify-content: center;
        align-items: center;
        height: 100vh;
        padding: 16px;
      }

      .card {
        background-color: #fff;
        border-radius: 16px;
        box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
        padding: 24px 20px;
        max-width: 360px;
        width: 100%;
        text-align: center;
        animation: fadeIn 0.5s ease;
      }

      h1 {
        color: #16a34a;
        font-size: 1.3rem;
        margin-bottom: 8px;
      }

      p {
        color: #555;
        font-size: 0.95rem;
        line-height: 1.4;
      }

      @keyframes fadeIn {
        from { opacity: 0; transform: translateY(10px); }
        to { opacity: 1; transform: translateY(0); }
      }

      @media (max-width: 480px) {
        h1 { font-size: 1.2rem; }
        p { font-size: 0.9rem; }
      }
    </style>
  </head>
  <body>
    <div class='card'>
      <h1>🎉 Đăng nhập Zalo thành công!</h1>
      <p>Bạn có thể quay lại ứng dụng để tiếp tục.</p>
      <p>Trang này sẽ tự đóng sau 3 giây...</p>
    </div>
    <script>
      setTimeout(() => window.close(), 3000);
    </script>
  </body>
</html>";


            return Content(htmlContent, "text/html;charset=utf-8");
        }
    }



}
