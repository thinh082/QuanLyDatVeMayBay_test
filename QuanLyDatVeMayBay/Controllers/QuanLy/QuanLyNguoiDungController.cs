using Microsoft.AspNetCore.Mvc;
using QuanLyDatVeMayBay.Models.Model;
using QuanLyDatVeMayBay.Services.QuanLy;

namespace QuanLyDatVeMayBay.Controllers.QuanLy
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuanLyNguoiDungController : ControllerBase
    {
        private readonly IQuanLyNguoiDungService _services;

        public QuanLyNguoiDungController(IQuanLyNguoiDungService services)
        {
            _services = services;
        }

        [HttpPost("GetDanhSachNguoiDung")]
        public async Task<IActionResult> GetDanhSachNguoiDung([FromBody] LocNguoiDungModel? filter = null)
        {
            var result = await _services.GetDanhSachNguoiDung(filter);
            return Ok(result);
        }

        [HttpGet("GetChiTietNguoiDung")]
        public async Task<IActionResult> GetChiTietNguoiDung(long idTaiKhoan)
        {
            var result = await _services.GetChiTietNguoiDung(idTaiKhoan);
            return Ok(result);
        }

        [HttpPost("CapNhatNguoiDung")]
        public async Task<IActionResult> CapNhatNguoiDung([FromBody] CapNhatNguoiDungModel model)
        {
            var result = await _services.CapNhatNguoiDung(model);
            return Ok(result);
        }

        [HttpPost("XoaNguoiDung")]
        public async Task<IActionResult> XoaNguoiDung(long idTaiKhoan)
        {
            var result = await _services.XoaNguoiDung(idTaiKhoan);
            return Ok(result);
        }
    }
}

