using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyDatVeMayBay.Services.ThanhToanServices;
using System.Security.Claims;

namespace QuanLyDatVeMayBay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThanhToanController : ControllerBase
    {
        private readonly IThanhToanService _service;
        public ThanhToanController(IThanhToanService thanhToanService)
        {
            _service = thanhToanService;
        }
        [HttpGet("GetThanhToan")]
        public async Task<IActionResult> GetThanhToan()
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
            var result = await _service.GetThanhToan(idTaiKhoan);
            return Ok(result);
        }
    }
}
