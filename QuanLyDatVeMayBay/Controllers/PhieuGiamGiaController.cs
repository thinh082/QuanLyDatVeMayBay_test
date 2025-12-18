using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Models.Model;
using QuanLyDatVeMayBay.Services.PhieuGiamGiaServices;
using QuanLyDatVeMayBay.Services.XacThucServices;
using System.Security.Claims;

namespace QuanLyDatVeMayBay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhieuGiamGiaController : ControllerBase
    {
        private readonly IPhieuGiamGiaService _services;

        public PhieuGiamGiaController(IPhieuGiamGiaService services)
        {
            _services = services;
        }
        [HttpGet("LayToanBoPhieuGiamGia")]
        public async Task<IActionResult> LayToanBoPhieuGiamGia()
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
            var result = await _services.LayToanBoPhieuGiamGia(idTaiKhoan);
            return Ok(result);
        }
        [HttpPost("TimKiemMaGiamGia")]
        public async Task<IActionResult> TimKiemMaGiamGia(string maGiamGia)
        {
            var result = await _services.TimKiemMaGiamGia(maGiamGia);
            return Ok(result);
        }
        [HttpPost("ApplyVoucher")]
        [Authorize]
        public async Task<IActionResult> ApplyVoucher(long idMaGiamGia)
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
            var result = await _services.ApplyVoucher(idTaiKhoan, idMaGiamGia);
            return Ok(result);
        }
        [HttpPost("LayDanhSachChiTietPhieuGiamGia")]
        public async Task<IActionResult> LayDanhSachChiTietPhieuGiamGia()
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
            var result = await _services.LayDanhSachChiTietPhieuGiamGia(idTaiKhoan);
            return Ok(result);
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
