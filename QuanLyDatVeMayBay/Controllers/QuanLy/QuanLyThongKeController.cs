using Microsoft.AspNetCore.Mvc;
using QuanLyDatVeMayBay.Services.QuanLy;

namespace QuanLyDatVeMayBay.Controllers.QuanLy
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuanLyThongKeController : ControllerBase
    {
        private readonly IQuanLyThongKeService _service;

        public QuanLyThongKeController(IQuanLyThongKeService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lấy KPI tổng quan cho dashboard
        /// </summary>
        [HttpGet("GetDashboardKpi")]
        public async Task<IActionResult> GetDashboardKpi()
        {
            var result = await _service.GetDashboardKpi();
            return Ok(result);
        }

        /// <summary>
        /// Lấy doanh thu theo ngày trong khoảng thời gian (mặc định 30 ngày gần nhất)
        /// </summary>
        [HttpGet("DoanhThuTheoNgayRange")]
        public async Task<IActionResult> DoanhThuTheoNgayRange([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            var result = await _service.DoanhThuTheoNgayRange(from, to);
            return Ok(result);
        }

        /// <summary>
        /// Lấy số vé theo ngày trong khoảng thời gian (mặc định 30 ngày gần nhất)
        /// </summary>
        [HttpGet("VeTheoNgayRange")]
        public async Task<IActionResult> VeTheoNgayRange([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            var result = await _service.VeTheoNgayRange(from, to);
            return Ok(result);
        }
    }
}

