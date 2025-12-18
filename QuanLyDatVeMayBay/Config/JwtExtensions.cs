using Microsoft.AspNetCore.Authentication.JwtBearer;
using QuanLyDatVeMayBay.Models.Entities;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace QuanLyDatVeMayBay.Config
{
    public static class JwtExtensions
    {
        public static IServiceCollection AddJwtAuthenticate(this IServiceCollection services, IConfiguration _config)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // ⚠️ dev có thể tắt, production nên bật
                options.SaveToken = true;             // lưu token vào HttpContext.User
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    ClockSkew = TimeSpan.Zero // bỏ delay 5 phút mặc định
                };
            });

            services.AddAuthorization();

            return services;
        }
    }

}
