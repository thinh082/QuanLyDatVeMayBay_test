using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Models.Model;
using QuanLyDatVeMayBay.Services.GoogleService.Model;
using QuanLyDatVeMayBay.Services.XacThucServices;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace QuanLyDatVeMayBay.Services.GoogleService
{
    public interface IGoogle
    {
        Task<string> GenUrl(string state = null);
        Task<GoogleTokenResponse> ExchangeCodeForTokenAsync(string code);
        Task<GoogleUserInfo> GetUserInfoAsync(string accessToken);
        Task<dynamic> GoogleCallback(string code);
    }
    public class Google: IGoogle
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceScopeFactory _scopeFactory;
        public Google(IConfiguration configuration, HttpClient httpClient,IServiceScopeFactory serviceScopeFactory)
        {
            _httpClient = httpClient;
            _scopeFactory = serviceScopeFactory;
        }
        public async Task<string> GenUrl(string state = null)
        {
            using var scope_ser = _scopeFactory.CreateScope();
            var context = scope_ser.ServiceProvider.GetRequiredService<ThinhContext>();
            var ggSetting = await context.GoogleSettings.FirstOrDefaultAsync();
            
            var scope = "openid email profile";

            var url = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={ggSetting.ClientId}" +
                $"&redirect_uri={ggSetting.RedirectUrl}" +
                $"&response_type=code" +
                $"&scope={Uri.EscapeDataString(scope)}" +
                $"&access_type=offline" +
                $"&prompt=consent"+
                (state != null ?$"&state={state}":"");
            return url;
                
        }

        public async Task<GoogleTokenResponse> ExchangeCodeForTokenAsync(string code)
        {
            using var scope_ser = _scopeFactory.CreateScope();
            var context = scope_ser.ServiceProvider.GetRequiredService<ThinhContext>();
            var ggSetting = await context.GoogleSettings.FirstOrDefaultAsync();

            var body = new Dictionary<string, string>
        {
            {"code", code},
            {"client_id", ggSetting.ClientId},
            {"client_secret", ggSetting.ClientSecret},
            {"redirect_uri", ggSetting.RedirectUrl},
            {"grant_type", "authorization_code"}
        };

            var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(body));
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GoogleTokenResponse>(json);
        }

        public async Task<GoogleUserInfo> GetUserInfoAsync(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GoogleUserInfo>(json);
        }
        public async Task<dynamic> GoogleCallback(string code)
        {
            //  Đổi code lấy token
            var tokenResponse = await ExchangeCodeForTokenAsync(code);
            var userInfo = await GetUserInfoAsync(tokenResponse.access_token);

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ThinhContext>();
            var thinhService = scope.ServiceProvider.GetRequiredService<ThinhService>();

            //  Kiểm tra xem user đã tồn tại chưa
            var existingUser = await context.TaiKhoans.FirstOrDefaultAsync(tk => tk.Email == userInfo.email);
            var pubkey = await context.HashKeys.FirstOrDefaultAsync();
            if (existingUser != null)
            {
                // Tạo JWT token nội bộ
                var token = thinhService.GenerateToken(existingUser.Id.ToString(), "");

                return new
                {
                    statusCode = 200,
                    message = "Đăng nhập thành công",
                    token = token
                };
            }
            else
            {
                var mk = Guid.NewGuid().ToString().Substring(0, 5);
                var hashPass = ThinhService.Encrypt(mk, pubkey.PublicKey);
                //  Nếu user chưa tồn tại → đăng ký mới
                var newUser = new TaiKhoan
                {
                    Email = userInfo.email,
                    MatKhau = hashPass,
                    SoDienThoai = "0000000000",
                    LoaiTaiKhoanId = 1,
                    HinhAnh = userInfo.picture,
                };

                var newKhachHang = new KhachHang
                {
                    TenKh = userInfo.name,
                    IdTaiKhoanNavigation = newUser 
                };

                context.KhachHangs.Add(newKhachHang);
                await context.SaveChangesAsync();


                // Nếu DangKyAsync không trả về user, bạn có thể query lại:
                var taiKhoanMoi = await context.TaiKhoans.FirstOrDefaultAsync(tk => tk.Email == newUser.Email);

                // ✅ Tạo JWT token cho user mới
                var token = thinhService.GenerateToken(taiKhoanMoi.Id.ToString(), "");

                return new
                {
                    statusCode = 200,
                    message = "Đăng ký + đăng nhập thành công",
                    token = token
                };
            }
        }

    }
}
