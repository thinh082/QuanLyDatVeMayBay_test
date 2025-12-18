using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyDatVeMayBay.Services.QuanLy;

namespace QuanLyDatVeMayBay.Controllers.QuanLy
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuanLyDoanhThuController : ControllerBase
    {
        private readonly IQuanLyDoanhThuServices _quanLyDoanhThuServices;
        public QuanLyDoanhThuController(IQuanLyDoanhThuServices quanLyDoanhThuServices)
        {
            _quanLyDoanhThuServices = quanLyDoanhThuServices;
        }
        [HttpGet("DoanhThuTheoNgay")]
        public async Task<IActionResult> DoanhThuTheoNgay([FromQuery] string ngay)
        {
            var result = await _quanLyDoanhThuServices.DoanhThuTheoNgay(ngay);
            return Ok(result);
        }
        [HttpGet("DoanhThuTheoThang")]
        public async Task<IActionResult> DoanhThuTheoThang([FromQuery] string thang)
        {
            var result = await _quanLyDoanhThuServices.DoanhThuTheoThang(thang);
            return Ok(result);
        }
        [HttpGet("DoanhThuTheoNam")]
        public async Task<IActionResult> DoanhThuTheoNam([FromQuery] string nam)
        {
            var result = await _quanLyDoanhThuServices.DoanhThuTheoNam(nam);
            return Ok(result);
        }
    }
}
