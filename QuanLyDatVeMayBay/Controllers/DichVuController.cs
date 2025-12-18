using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyDatVeMayBay.Services.DichVuService;
using System.Security.Claims;

namespace QuanLyDatVeMayBay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DichVuController : ControllerBase
    {
        private readonly IDichVuService _service;
        public DichVuController(IDichVuService dichVuService)
        {
            _service = dichVuService;
        }
        [HttpGet("LayDanhSachDichVu")]
        public async Task<IActionResult> LayDanhSachDichVu()
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
            if (idTaiKhoan <= 0)
            {
                return Ok(new
                {
                    statusCode = 500,
                    message = "ID tài khoản không hợp lệ!"
                });
            }
            var result = await _service.LayDanhSachDichVu(idTaiKhoan);
            return Ok(result);
        }
        [HttpGet("ChiTietDichVu")]
        public async Task<IActionResult> ChiTietDichVu([FromQuery] long idDichVu, [FromQuery] int loaiDichVu)
        {
            if (idDichVu <= 0 || (loaiDichVu != 1 && loaiDichVu != 2))
            {
                return Ok(new
                {
                    statusCode = 500,
                    message = "Tham số không hợp lệ!"
                });
            }
            var result = await _service.ChiTietDichVu(idDichVu, loaiDichVu);
            return Ok(result);
        }
    }
}
