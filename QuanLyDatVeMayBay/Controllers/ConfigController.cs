using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Attributes;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Services;
using StackExchange.Redis;

namespace QuanLyDatVeMayBay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly ConvertDBToJsonServices _convertDBToJsonServices;
        public ConfigController(ConvertDBToJsonServices convertDBToJsonServices)
        {
            _convertDBToJsonServices = convertDBToJsonServices;
        }
        [HttpGet("convert-tinh-to-json")]
        public async Task<IActionResult> ConvertTinhToJson()
        {
            await _convertDBToJsonServices.ConvertTinhToJson();
            return Ok(new { message = "Convert Tỉnh to JSON thành công!" });
        }
        [HttpGet("convert-quan-to-json")]
        public async Task<IActionResult> ConvertQuanToJson()
        {
            await _convertDBToJsonServices.ConvertQuanToJson();
            return Ok(new { message = "Convert Quận/Huyện to JSON thành công!" });
        }
        [HttpGet("convert-phuong-to-json")]
        public async Task<IActionResult> ConvertPhuongToJson()
        {
            await _convertDBToJsonServices.ConvertPhuongToJson();
            return Ok(new { message = "Convert Phường/Xã to JSON thành công!" });
        }
        [HttpGet("convert-quocTinh")]
        public async Task<IActionResult> ConvertDuongToJson()
        {
            await _convertDBToJsonServices.ConvertQuocTichToJson();
            return Ok(new { message = "Convert Đường to JSON thành công!" });
        }
        [HttpGet("convert-TienNghi-to-json")]
        public async Task<IActionResult> ConvertTienNghiToJson()
        {
            await _convertDBToJsonServices.ConvertTienNghiToJson();
            return Ok(new { message = "Convert Tiện Nghi to JSON thành công!" });
        }
        [HttpGet("convert-hangBay-to-json")]
        public async Task<IActionResult> ConvertQuocTichToJson()
        {
            await _convertDBToJsonServices.ConvertHangBayToJson();
            return Ok(new { message = "Convert Quốc Tịch to JSON thành công!" });
        }
        [HttpGet("convert-sanbay-to-json")]
        public async Task<IActionResult> ConvertSanBayToJson()
        {
            await _convertDBToJsonServices.ConvertSanBayToJson();
            return Ok(new { message = "Convert Quốc Tịch to JSON thành công!" });
        }
        [HttpGet("redis-test")]
        public async Task<IActionResult> TestRedis([FromServices] IConnectionMultiplexer redis)
        {
            var db = redis.GetDatabase();
            await db.StringSetAsync("testKey", "Redis đang hoạt động!");
            var value = await db.StringGetAsync("testKey");
            return Ok(value.ToString());
        }

        [RequireCccd]
        [HttpPost("middlewareCCCD")]
        public async Task<IActionResult> MiddlewareCCCD()
        {
            
            return Ok(new { statusCode = 200,message = "ok" });
        }
    }
        
}
