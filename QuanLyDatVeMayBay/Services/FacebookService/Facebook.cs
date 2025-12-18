using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Services.FacebookService.Model;

namespace QuanLyDatVeMayBay.Services.FacebookService
{
    public interface IFaceBook
    {
        Task<string> GenUrl(string state = null);
        Task<dynamic> XuLyCallBackFb(string code, string state);
    }
    public class Facebook:IFaceBook
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public Facebook(IServiceScopeFactory serviceScopeFactory)
        {
            _scopeFactory = serviceScopeFactory;
        }
        public async Task<string> GenUrl(string state = null)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ThinhContext>();
            var fbSetting = await context.FacebookSettings.FirstOrDefaultAsync();
            var scopes = "email,public_profile";
            var responseType = "code";
            var version = "v20.0";
            state ??= Guid.NewGuid().ToString();

            // Dựng URL
            var url = $"https://www.facebook.com/{version}/dialog/oauth" +
                      $"?client_id={fbSetting.AppId}" +
                      $"&redirect_uri={Uri.EscapeDataString(fbSetting.RedirectUri)}" +
                      $"&state={state}" +
                      $"&scope={Uri.EscapeDataString(scopes)}" +
                      $"&response_type={responseType}";
            return url;
        }
        public async Task<dynamic> XuLyCallBackFb(string code, string state)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ThinhContext>();
            var thinhService = scope.ServiceProvider.GetRequiredService<ThinhService>();
            var pubkey = await context.HashKeys.FirstOrDefaultAsync();
            var fbSetting = await context.FacebookSettings.FirstOrDefaultAsync();

            using var httpClient = new HttpClient();
            var tokenUrl = $"https://graph.facebook.com/v20.0/oauth/access_token" +
                               $"?client_id={fbSetting.AppId}" +
                               $"&redirect_uri={Uri.EscapeDataString(fbSetting.RedirectUri)}" +
                               $"&client_secret={fbSetting.AppSecret}" +
                               $"&code={code}";
            var tokenResponse = await httpClient.GetStringAsync(tokenUrl);
            var tokenData = JsonConvert.DeserializeObject<FacebookTokenResponse>(tokenResponse);
            if (tokenData == null || string.IsNullOrEmpty(tokenData.access_token)) return null;

            //
            var userInfor = $"https://graph.facebook.com/me" +
                              $"?fields=id,name,email,picture" +
                              $"&access_token={tokenData.access_token}";
            var userRespone = await httpClient.GetStringAsync(userInfor);
            var userData = JsonConvert.DeserializeObject<FacebookUserResponse>(userRespone);
            if (userData == null || string.IsNullOrEmpty(userData.id)) return null;

            //Lưu DB
            
            string email = userData.email;
            if (string.IsNullOrEmpty(email))
            {
                email = $"facebook_{userData.id}@facebook.temp";
            }
            try
            {
                var tk = await context.TaiKhoans.FirstOrDefaultAsync(t => t.Email == email);
                if (tk != null) 
                {
                    var token = thinhService.GenerateToken(tk.Id.ToString(), pubkey.PublicKey);
                    return new
                    {
                        statusCode = 200,
                        message = "Đăng nhập thành công",
                        token
                    };
                }
                else
                {
                    var mk = Guid.NewGuid().ToString().Substring(0, 5);
                    var hashPass = ThinhService.Encrypt(mk, pubkey.PublicKey);
                    //  Nếu user chưa tồn tại → đăng ký mới
                    var newUser = new TaiKhoan
                    {
                        Email = email,
                        MatKhau = hashPass,
                        SoDienThoai = "0000000000",
                        LoaiTaiKhoanId = 1,
                        HinhAnh = userData.picture.data.url,
                    };
                    var newRecordKhachHang = new KhachHang
                    {
                        TenKh = userData.name,
                        IdTaiKhoanNavigation = newUser,
                    };
                    context.TaiKhoans.Add(newUser);
                    await context.SaveChangesAsync();

                    // Nếu DangKyAsync không trả về user, bạn có thể query lại:
                    var taiKhoanMoi = await context.TaiKhoans.FirstOrDefaultAsync(tk => tk.Email == newUser.Email);

                    // ✅ Tạo JWT token cho user mới
                    var token = thinhService.GenerateToken(taiKhoanMoi.Id.ToString(), "");

                    return new
                    {
                        statusCode = 200,
                        message = "Đăng nhập thành công",
                        token
                    };
                }
            }
            catch(Exception ex)
            {
                return null;

            }
    }
    }
}
