using Microsoft.AspNetCore.Mvc;
using QuanLyDatVeMayBay.Models.Model;
using QuanLyDatVeMayBay.Services.QuanLy;

namespace QuanLyDatVeMayBay.Controllers.QuanLy
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuanLyLichSuThanhToanController : ControllerBase
    {
        private readonly IQuanLyLichSuThanhToanService _service;

        public QuanLyLichSuThanhToanController(IQuanLyLichSuThanhToanService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lấy danh sách lịch sử thanh toán với filter và phân trang
        /// </summary>
        [HttpPost("GetDanhSachLichSuThanhToan")]
        public async Task<IActionResult> GetDanhSachLichSuThanhToan(
            [FromBody] LocLichSuThanhToanModel? filter = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _service.GetDanhSachLichSuThanhToan(filter, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết một giao dịch thanh toán
        /// </summary>
        [HttpGet("GetChiTietLichSuThanhToan")]
        public async Task<IActionResult> GetChiTietLichSuThanhToan([FromQuery] long id)
        {
            var result = await _service.GetChiTietLichSuThanhToan(id);
            return Ok(result);
        }

        /// <summary>
        /// Thống kê thanh toán theo phương thức (Tiền mặt, VNPay, PayPal, ...)
        /// </summary>
        [HttpGet("ThongKeTheoPhuongThucThanhToan")]
        public async Task<IActionResult> ThongKeTheoPhuongThucThanhToan(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _service.ThongKeTheoPhuongThucThanhToan(fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Thống kê thanh toán theo trạng thái (Thành công, Thất bại, Đang xử lý, ...)
        /// </summary>
        [HttpGet("ThongKeTheoTrangThai")]
        public async Task<IActionResult> ThongKeTheoTrangThai(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _service.ThongKeTheoTrangThai(fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Thống kê tổng quan: Tổng số giao dịch, Tổng tiền, Tỷ lệ thành công/thất bại
        /// </summary>
        [HttpGet("ThongKeTongQuan")]
        public async Task<IActionResult> ThongKeTongQuan(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _service.ThongKeTongQuan(fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Xuất danh sách lịch sử thanh toán ra file Excel
        /// </summary>
        [HttpPost("ExportExcel")]
        public async Task<IActionResult> ExportExcel([FromBody] LocLichSuThanhToanModel? filter = null)
        {
            var result = await _service.ExportExcel(filter);
            return Ok(result);
        }
    }
}

