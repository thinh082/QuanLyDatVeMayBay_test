using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyDatVeMayBay.Models.Model;
using QuanLyDatVeMayBay.Services.PhieuGiamGiaServices;

namespace QuanLyDatVeMayBay.Controllers.QuanLy
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuanLyPhieuGiamGiaController : ControllerBase
    {
        private readonly IPhieuGiamGiaService _services;

        public QuanLyPhieuGiamGiaController(IPhieuGiamGiaService services)
        {
            _services = services;
        }
        [HttpGet("GetDanhSachPhieuGiamGia")]
        public async Task<IActionResult> GetDanhSachPhieuGiamGia()
        {
            var result = await _services.GetDanhSachPhieuGiamGia();
            return Ok(result);
        }
        [HttpPost("ActivePhieuGiamGia")]
        public async Task<IActionResult> ActivePhieuGiamGia(long idPhieuGiamGia)
        {
            var result = await _services.ActivePhieuGiamGia(idPhieuGiamGia);
            return Ok(result);
        }
        [HttpPost("CapNhatPhieuGiamGia")]
        public async Task<IActionResult> CapNhatPhieuGiamGia(ThemPhieuGiamGiaModel phieuGiamGia)
        {
            var result = await _services.CapNhatPhieuGiamGia(phieuGiamGia);
            return Ok(result);
        }
    }
}
