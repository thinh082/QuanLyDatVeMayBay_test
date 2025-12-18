using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Services.ThongBaoService;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuanLyDatVeMayBay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThongBaoController : ControllerBase
    {
        private readonly ThinhContext _context;
        private readonly IThongBaoService _services;
        public ThongBaoController(ThinhContext context, IThongBaoService thongBaoService)
        {
            _context = context;
            _services = thongBaoService;
        }
        [HttpPost("ThongBao")]
        public async Task<IActionResult> GetThongBao()
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
            var thongBaos = await _services.LayThongBao(idTaiKhoan);
            return Ok(thongBaos);
        }        
        [HttpPost("ChiTietThongBao")]
        public async Task<IActionResult> ChiTietThongBao(long idThongBao)
        {
            var result = await _services.ChiTietThongBao(idThongBao);
            return Ok(result);
        }
        [HttpPost("XoaThongBao")]
        public async Task<IActionResult> XoaThongBao( long idThongBao)
        {
            var result = await _services.XoaThongBao(idThongBao);
            return Ok(result);
        }
    }
}
