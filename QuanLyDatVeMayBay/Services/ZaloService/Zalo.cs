using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Services.ZaloService.Model;
using System.Net.Http.Headers;
using System.Text.Json;
using static QRCoder.PayloadGenerator;
namespace QuanLyDatVeMayBay.Services.ZaloService
{
    public interface IZalo
    {
        Task<string> GenUrl(string state = null);
        Task<dynamic> XuLySauCallBack(string code,string state);
    }
    public class Zalo:IZalo
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly HttpClient _httpClient;
        public Zalo(IServiceScopeFactory scopeFactory, HttpClient httpClient)
        {
            _scopeFactory = scopeFactory;
            _httpClient = httpClient;
        }
        public async Task<string> GenUrl(string state = null)
        {
            var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ThinhContext>();
            var zaloSetting = await context.ZaloSettings.FirstOrDefaultAsync();
            string code_verifier = ThinhService.GenerateCodeVerifier();
            string code_challenge = ThinhService.GenerateCodeChallenge(code_verifier);
            var url = $"https://oauth.zaloapp.com/v4/permission?app_id={zaloSetting.AppId}&redirect_uri={zaloSetting.RedirectUrl}&code_challenge={code_challenge}&state={state}";
            var o2Auth = new OauthState
            {
                CodeVerifier = code_verifier,
                State = state
            };
            context.OauthStates.Add(o2Auth);
            await context.SaveChangesAsync();
            return url;
        }
        public async Task<dynamic> XuLySauCallBack(string code, string state)
        {
            var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ThinhContext>();
            var zaloSetting = await context.ZaloSettings.FirstOrDefaultAsync();
            var pubkey = await context.HashKeys.FirstOrDefaultAsync();

            var thinhService = scope.ServiceProvider.GetRequiredService<ThinhService>();
            var code_verifier = (await context.OauthStates.FirstOrDefaultAsync(s => s.State == state))?.CodeVerifier;
            var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth.zaloapp.com/v4/access_token");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("secret_key",zaloSetting.AppSceret);

            var formData = new Dictionary<string, string>
            {
                { "app_id", zaloSetting.AppId },
                { "code", code },
                { "grant_type", "authorization_code" },
                { "code_verifier", code_verifier }
            };
            request.Content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<ZaloRespone>(json);

            var url = $"https://graph.zalo.me/v2.0/me?access_token={data.access_token}&fields=id,name,picture$access_token={data.access_token}";
            var response_zaloinfor = await _httpClient.GetAsync(url);
            var json_userInfor = await response_zaloinfor.Content.ReadAsStringAsync();

            if (!response_zaloinfor.IsSuccessStatusCode)
            {
                throw new Exception($"Zalo userinfo error: {response_zaloinfor.StatusCode} - {json_userInfor}");
            }

            var userInfor = JsonConvert.DeserializeObject<ZaloUser>(json_userInfor);

            //Lưu DB         
            try
            {
                var tk = await context.TaiKhoans.FirstOrDefaultAsync(t => t.Email == $"Zalo_{userInfor.Id}@zalo.temp");
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
                        Email = $"Zalo_{userInfor.Id}@zalo.temp",
                        MatKhau = hashPass,
                        SoDienThoai = "0000000000",
                        LoaiTaiKhoanId = 1,
                        HinhAnh = userInfor.Picture?.Data?.Url??"",
                    };
                    var newRecordKhachHang = new KhachHang
                    {
                        TenKh = userInfor.Name,
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
            catch (Exception ex)
            {
                return null;

            }
        }

    }
}
