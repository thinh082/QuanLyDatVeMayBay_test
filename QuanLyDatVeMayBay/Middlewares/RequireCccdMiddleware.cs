using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Attributes;
using QuanLyDatVeMayBay.Models.Entities;
using System.Security.Claims;

namespace QuanLyDatVeMayBay.Middlewares
{
    public class RequireCccdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;

        public RequireCccdMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var requireAttr = endpoint?.Metadata.GetMetadata<RequireCccdAttribute>();
            if (requireAttr == null)
            {
                await _next(context);
                return;
            }

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                await WriteError(context, 401, "Bạn chưa đăng nhập!");
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ThinhContext>();

            if (!await HasUpdatedThongTinCaNhan(db, userId))
                await WriteError(context, 403, "Bạn cần cập nhật thông tin cá nhân trước khi tiếp tục!");
            else if (!await HasUpdatedCCCD(db, userId))
                await WriteError(context, 403, "Bạn cần cập nhật CCCD!");
            else if (!await HasUpdatedPassport(db, userId))
                await WriteError(context, 403, "Bạn cần cập nhật Passport!");            
            else
                await _next(context);
        }

        private async Task<bool> HasUpdatedThongTinCaNhan(ThinhContext db, string userId) =>
            await db.TaiKhoans.AnyAsync(r => r.Id == long.Parse(userId) && r.KhachHang.DiaChi != null);

        private async Task<bool> HasUpdatedCCCD(ThinhContext db, string userId) =>
            await db.TaiKhoans.AnyAsync(r => r.Id == long.Parse(userId) && !string.IsNullOrEmpty(r.KhachHang.KhachHangCccd.SoCccd));

        private async Task<bool> HasUpdatedPassport(ThinhContext db, string userId) =>
    await db.TaiKhoans
        .Where(r => r.Id == long.Parse(userId))
        .AnyAsync(r => r.KhachHang.KhachHangPassports.Any());



        private async Task WriteError(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(new { statusCode, message });
        }

    }
}
