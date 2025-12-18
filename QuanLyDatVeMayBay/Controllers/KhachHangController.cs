using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyDatVeMayBay.Models.Model;
using QuanLyDatVeMayBay.Services;
using QuanLyDatVeMayBay.Services.ThongTinService.cs;
using System.Security.Claims;

namespace QuanLyDatVeMayBay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class KhachHangController : ControllerBase
    {
        private readonly IThongTinService _services;
        
        public KhachHangController(IThongTinService thongTinService)
        {
            _services = thongTinService;
        }
        [HttpPost("ThongTinCoBan")]
        public async Task<IActionResult> ThongTinCoBan()
        {
            var idTaiKhoanClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if(idTaiKhoanClaim == null || !long.TryParse(idTaiKhoanClaim.Value, out long idTaiKhoan))
            {
                return Unauthorized(new
                {
                    statusCode = 401,
                    message = "Người dùng chưa đăng nhập hoặc token không hợp lệ"
                });
            }
            var result = await _services.ThongTinCoBan(idTaiKhoan);
            return Ok(result);
        }
        [HttpPost("UpAvatar")]
        public async Task<IActionResult> UpAvatar(IFormFile file)
        {
            var idTaiKhoanClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idTaiKhoanClaim == null || !long.TryParse(idTaiKhoanClaim.Value, out long idTaiKhoan))
            {
                return Unauthorized(new
                {
                    statusCode = 401,
                    message = "Người dùng chưa đăng nhập hoặc token không hợp lệ"
                });
            }
            var result = await _services.UpLoadAvt(file, idTaiKhoan);
            return Ok(result);
        }
        [HttpPost("ThongTinCapNhat")]
        public async Task<IActionResult> ThongTinCapNhat()
        {
            var idTaiKhoanClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idTaiKhoanClaim == null || !long.TryParse(idTaiKhoanClaim.Value, out long idTaiKhoan))
            {
                return Unauthorized(new
                {
                    statusCode = 401,
                    message = "Người dùng chưa đăng nhập hoặc token không hợp lệ"
                });
            }
            var result = await _services.ThongTinCapNhat(idTaiKhoan);
            return Ok(result);
        }
        [HttpPost("CapNhatThongTin")]
        public async Task<IActionResult> CapNhatThongTin([FromBody] CapNhatThongTinModel model)
        {
            var idTaiKhoanClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idTaiKhoanClaim == null || !long.TryParse(idTaiKhoanClaim.Value, out long idTaiKhoan))
            {
                return Unauthorized(new
                {
                    statusCode = 401,
                    message = "Người dùng chưa đăng nhập hoặc token không hợp lệ"
                });
            }
            var result = await _services.CapNhatThongTin(model,idTaiKhoan);
            return Ok(result);
        }
        [HttpPost("ThongTinCCCD")]
        public async Task<IActionResult> ThongTinCCCD()
        {
            var idTaiKhoanClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idTaiKhoanClaim == null || !long.TryParse(idTaiKhoanClaim.Value, out long idTaiKhoan))
            {
                return Unauthorized(new
                {
                    statusCode = 401,
                    message = "Người dùng chưa đăng nhập hoặc token không hợp lệ"
                });
            }
            var result = await _services.ThongTinCCCD(idTaiKhoan);
            return Ok(result);
        }
        [HttpPost("CapNhatCCCD")]
        public async Task<IActionResult> CapNhatCCCD([FromBody] CapNhatCCCDModel model)
        {
            var idTaiKhoanClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idTaiKhoanClaim == null || !long.TryParse(idTaiKhoanClaim.Value, out long idTaiKhoan))
            {
                return Unauthorized(new
                {
                    statusCode = 401,
                    message = "Người dùng chưa đăng nhập hoặc token không hợp lệ"
                });
            }
            var result = await _services.CapNhatCCCD(model, idTaiKhoan);
            return Ok(result);
        }
        [HttpGet("ThongTinPassport")]
        public async Task<IActionResult> ThongTinPassport()
        {
            var idTaiKhoanClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idTaiKhoanClaim == null || !long.TryParse(idTaiKhoanClaim.Value, out long idTaiKhoan))
            {
                return Unauthorized(new
                {
                    statusCode = 401,
                    message = "Người dùng chưa đăng nhập hoặc token không hợp lệ"
                });
            }
            var result = await _services.ThongTinPassport(idTaiKhoan);
            return Ok(result);
        }
        [HttpPost("CapNhatPassport")]
        public async Task<IActionResult> CapNhatPassport([FromBody] CapNhatPassportModel model)
        {
            var idTaiKhoanClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idTaiKhoanClaim == null || !long.TryParse(idTaiKhoanClaim.Value, out long idTaiKhoan))
            {
                return Unauthorized(new
                {
                    statusCode = 401,
                    message = "Người dùng chưa đăng nhập hoặc token không hợp lệ"
                });
            }
            var result = await _services.CapNhatPassport(model, idTaiKhoan);
            return Ok(result);
        }
    }
}
