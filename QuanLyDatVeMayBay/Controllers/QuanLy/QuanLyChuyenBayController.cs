using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyDatVeMayBay.Models.Model;
using QuanLyDatVeMayBay.Services.QuanLy;

namespace QuanLyDatVeMayBay.Controllers.QuanLy
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuanLyChuyenBayController : ControllerBase
    {
        private readonly IQuanLyChuyenBayService _services;
        public QuanLyChuyenBayController(IQuanLyChuyenBayService services)
        {
            _services = services;
        }
        [HttpPost("GetDanhSachChuyenBay")]  
        public async Task<IActionResult> GetDanhSachChuyenBay([FromBody] LocChuyenBayModel? filter = null)
        {
            var result = await _services.GetDanhSachChuyenBay(filter);
            return Ok(result);
        }
        [HttpGet("GetChiTietChuyenBay")]
        public async Task<IActionResult> GetChiTietChuyenBay(long idChuyenBay)
        {
            var result = await _services.GetChiTietChuyenBay(idChuyenBay);
            return Ok(result);
        }
        [HttpPost("LuuChuyenBay")]
        public async Task<IActionResult> LuuChuyenBay([FromBody] LuuChuyenBayModel model)
        {
            var result = await _services.LuuChuyenBay(model);
            return Ok(result);
        }
        
        [HttpPost("XoaChuyenBay")]
        public async Task<IActionResult> XoaChuyenBay(long idChuyenBay)
        {
            var result = await _services.XoaChuyenBay(idChuyenBay);
            return Ok(result);
        }
    }
}
