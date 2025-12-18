using Microsoft.AspNetCore.Mvc;
using QuanLyDatVeMayBay.Models.Model;
using QuanLyDatVeMayBay.Services.QuanLy;

namespace QuanLyDatVeMayBay.Controllers.QuanLy
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuanLyVeController : ControllerBase
    {
        private readonly IQuanLyVeService _service;
        public QuanLyVeController(IQuanLyVeService service)
        {
            _service = service;
        }

        // Lấy danh sách + lọc vé
        [HttpPost("GetDanhSachDatVe")]
        public async Task<IActionResult> GetDanhSachDatVe([FromBody] LocDatVeAdminModel? filter = null)
        {
            var result = await _service.GetDanhSachDatVe(filter);
            return Ok(result);
        }

        // Xem chi tiết vé
        [HttpGet("GetChiTietDatVe")]
        public async Task<IActionResult> GetChiTietDatVe(long idDatVe)
        {
            var result = await _service.GetChiTietDatVe(idDatVe);
            return Ok(result);
        }

        // Cập nhật trạng thái vé (không cần lý do)
        [HttpPost("CapNhatTrangThai")]
        public async Task<IActionResult> CapNhatTrangThai([FromBody] CapNhatTrangThaiVeModel model)
        {
            var result = await _service.CapNhatTrangThai(model);
            return Ok(result);
        }

        // In chi tiết vé ra PDF
        [HttpGet("InChiTietVe")]
        public async Task<IActionResult> InChiTietVe(long idDatVe)
        {
            var pdfBytes = await _service.InChiTietVe(idDatVe);
            if (pdfBytes == null)
                return NotFound(new { statusCode = 404, message = "Không tìm thấy vé" });

            return File(pdfBytes, "application/pdf", $"Ve_{idDatVe}.pdf");
        }
    }
}

