using Amazon.Runtime;
using Amazon.S3;
using Amazon;
using Microsoft.EntityFrameworkCore;
using MyApp;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Services;
using QuanLyDatVeMayBay.Services.PhieuGiamGiaServices;
using QuanLyDatVeMayBay.Services.ThongTinService.cs;
using QuanLyDatVeMayBay.Services.XacThucServices;
using QuanLyDatVeMayBay.Services.ThongBaoService;
using QuanLyDatVeMayBay.Config;
using Microsoft.OpenApi.Models;
using QuanLyDatVeMayBay.Services.DichVuService;
using QuanLyDatVeMayBay.Services.ThanhToanServices;
using QuanLyDatVeMayBay.Services.VnpayServices;
using QuanLyDatVeMayBay.Middlewares;
using QuanLyDatVeMayBay.Services.GoogleService;
using QuanLyDatVeMayBay.Services.FacebookService;
using QuanLyDatVeMayBay.Services.ZaloService;
using QuanLyDatVeMayBay.Services.PaypalService;
using QuanLyDatVeMayBay.Services.ChuyenBayService;
using QuanLyDatVeMayBay.Services.QuanLy;
using StackExchange.Redis;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
builder.Services.AddDbContext<ThinhContext>(c =>
        c.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));
builder.Services.AddScoped<ThinhService>();
builder.Services.AddScoped<IPhieuGiamGiaService,PhieuGiamGiaSerivce>();
builder.Services.AddScoped<IThongTinService, ThongTinService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
         builder =>
         {
             builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed(_ => true);
         });
});
builder.Services.AddSignalR();
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379")
);
builder.Services.AddSingleton<RedisService>();
builder.Services.AddScoped<IXacThucTaiKhoanServices, XacThucTaiKhoanServices>();
builder.Services.AddHttpClient<IGoogle, Google>();
builder.Services.AddScoped<IFaceBook,Facebook>();
builder.Services.AddScoped<IZalo, Zalo>();
builder.Services.AddScoped<IThongBaoService, ThongBaoService>();
builder.Services.AddScoped<IDichVuService, DichVuService>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddScoped<IThanhToanService,ThanhToanService>();
builder.Services.AddScoped<IVnpay, Vnpay>();
builder.Services.AddHttpClient<IPaypal, Paypal>();
builder.Services.AddScoped<IChuyenBayService, ChuyenBayService>();
builder.Services.AddScoped<IQuanLyDoanhThuServices, QuanLyDoanhThuServices>();
builder.Services.AddScoped<IQuanLyChuyenBayService, QuanLyChuyenBayService>();
builder.Services.AddScoped<IQuanLyNguoiDungService, QuanLyNguoiDungService>();
builder.Services.AddScoped<IQuanLyVeService, QuanLyVeService>();
builder.Services.AddScoped<IQuanLyThongKeService, QuanLyThongKeService>();
builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddJwtAuthenticate(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddScoped<IAmazonS3>(sp =>
{
    var db = sp.GetRequiredService<ThinhContext>();
    var awsConfig = AwsHelper.LoadAwsS3Settings(db);

    var credentials = new BasicAWSCredentials(awsConfig.AccessKey, awsConfig.SecretKey);
    return new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(awsConfig.Region));
});
builder.Services.AddScoped<ConvertDBToJsonServices>();

builder.Services.AddScoped(sp =>
{
    var db = sp.GetRequiredService<ThinhContext>();
    return AwsHelper.LoadAwsS3Settings(db);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Thêm cấu hình cho Bearer Token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Nhập token theo dạng: Bearer {your token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RequireCccdMiddleware>();
app.UseStaticFiles(); // Cho phép truy cập các tệp tĩnh từ wwwroot
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notify");
app.Run();
