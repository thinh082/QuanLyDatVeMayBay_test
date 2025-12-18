using Azure.Core;
using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Models.Model;
using QuanLyDatVeMayBay.Services;
using QuanLyDatVeMayBay.Services.ThongBaoService;

namespace QuanLyDatVeMayBay.Services.XacThucServices
{
    public interface IXacThucTaiKhoanServices
    {
        Task<dynamic> DangKyAsync(DangKyRequest request);
        Task<dynamic> QuenMatKhauAsync(string? email);
        Task<dynamic> XacNhanOtpAsync(string ?email, string? otp);
        Task<dynamic> DoiMatKhauAsync(string? email, string ?matKhau, string ?xacNhanMatKhau);
        Task<dynamic> DangNhap(string TaiKhoan, string MatKhau);
        Task<dynamic> SetThoiGianKhoa(long idTaiKhoan);

    }

    public class XacThucTaiKhoanServices : IXacThucTaiKhoanServices
    {
        private readonly ThinhContext _context;
        private readonly ThinhService _thinhService;
        private readonly IThongBaoService _thongBaoService;
        private readonly IBackgroundTaskQueue _taskQueue;


        // Lưu OTP tạm
        private static readonly Dictionary<string, string> _otpStore = new();
        // Lưu trạng thái đã xác nhận OTP
        private static readonly HashSet<string> _otpConfirmed = new();
        private const int MAX_SAI = 5;
        private static readonly TimeSpan KHOA_30P = TimeSpan.FromMinutes(30);

        public XacThucTaiKhoanServices(ThinhContext context, ThinhService thinhService, IThongBaoService thongBaoService, IBackgroundTaskQueue taskQueue)
        {
            _context = context;
            _thinhService = thinhService;
            _thongBaoService = thongBaoService;
            _taskQueue = taskQueue;
        }
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.TaiKhoans.AnyAsync(x => x.Email == email);
        }

        public async Task<bool> SoDienThoaiExistsAsync(string soDienThoai)
        {
            return await _context.TaiKhoans.AnyAsync(x => x.SoDienThoai == soDienThoai);
        }
        // ---------------- ĐĂNG KÝ ----------------
        public async Task<dynamic> DangKyAsync(DangKyRequest request)
        {
            if (request.MatKhau != request.XacNhanMatKhau)
                return new { statusCode = 400, message = "Đăng ký thất bại: Mật khẩu và xác nhận mật khẩu không trùng khớp" };

            if (!ThinhService.IsValidPhoneNumber(request.SoDienThoai))
                return new { statusCode = 400, message = "Số điện thoại không hợp lệ (phải gồm đúng 10 chữ số)" };

            //if (!ThinhService.IsValidEmail(request.Email))
            //    return new { statusCode = 400, message = "Email không hợp lệ (phải có dạng ...@gmail.com)" };

            if (await EmailExistsAsync(request.Email))
                return new { statusCode = 400, message = "Đăng ký thất bại: Email đã được sử dụng" };

            if (await SoDienThoaiExistsAsync(request.SoDienThoai))
                return new { statusCode = 400, message = "Số điện thoại đã được sử dụng" };
            var pub = await _context.HashKeys.FirstOrDefaultAsync();
            if (pub == null || string.IsNullOrWhiteSpace(pub.PublicKey))
                return new { statusCode = 500, message = "Đăng ký thất bại: Không tìm thấy khóa mã hóa" };

            string haskpas = ThinhService.Encrypt(request.MatKhau, pub.PublicKey);
            var taiKhoan = new TaiKhoan
            {
                Email = request.Email,
                SoDienThoai = request.SoDienThoai,
                MatKhau = haskpas,
                LoaiTaiKhoanId = 1,
            };

            try
            {
                await _context.TaiKhoans.AddAsync(taiKhoan);
                await _context.SaveChangesAsync();
                await _thongBaoService.GuiThongBao(taiKhoan.Id, "Tạo tài khoản mới", "Chào mừng bạn! chúc bạn hôm nay một ngày tốt lành ",ThinhService.LoaiAnh(1));
                return new { statusCode = 200, message = "Đăng ký thành công" };
            }
            catch (Exception ex)
            {
                return new { statusCode = 500, message = "Đăng ký thất bại: " + ex.Message };
            }
        }

        // ---------------- QUÊN MẬT KHẨU ----------------
        public async Task<dynamic> QuenMatKhauAsync(string? email)
        {
            if (string.IsNullOrEmpty(email) )
                return new { statusCode = 400, message = "Email không được rỗng" };

            if (!ThinhService.IsValidEmail(email))
                return new { statusCode = 400, message = "Email không hợp lệ" };


            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.Email == email);
            if (taiKhoan == null)
                return new { statusCode = 404, message = "Không tìm thấy tài khoản" };



            var otp = new Random().Next(100000, 999999).ToString();
            _otpStore[email] = otp;

            _taskQueue.QueueBackgroundWorkItem(async (sp, ct) =>
            {
                var emailService = sp.GetRequiredService<ThinhService>();

                var emailTask = emailService.GuiEmail(email, "Mã OTP lấy lại mật khẩu", $"Mã OTP của bạn là: <b>{otp}</b>");

                await Task.WhenAll(emailTask);
            });
            return new { statusCode = 200, message = "Đã gửi OTP qua email" };
        }

        // ---------------- XÁC NHẬN OTP ----------------
        public Task<dynamic> XacNhanOtpAsync(string ?email, string? otp)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
                return Task.FromResult<dynamic>(new { statusCode = 400, message = "Email và OTP không được rỗng" });

            if (_otpStore.TryGetValue(email, out var storedOtp) && storedOtp == otp)
            {
                _otpStore.Remove(email);
                _otpConfirmed.Add(email); // Đánh dấu đã xác nhận OTP
                return Task.FromResult<dynamic>(new { statusCode = 200, message = "Xác nhận OTP thành công" });
            }

            return Task.FromResult<dynamic>(new { statusCode = 400, message = "OTP không đúng hoặc đã hết hạn" });
        }

        // ---------------- ĐỔI MẬT KHẨU ----------------
        public async Task<dynamic> DoiMatKhauAsync(string ?email, string ? matKhau, string ?xacNhanMatKhau)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(matKhau) || string.IsNullOrEmpty(xacNhanMatKhau))
                return new { statusCode = 400, message = "Email, mật khẩu và xác nhận mật khẩu không được rỗng" };

            if (matKhau != xacNhanMatKhau)
                return new { statusCode = 400, message = "Mật khẩu và xác nhận mật khẩu không trùng khớp" };

            if (!_otpConfirmed.Contains(email))
                return new { statusCode = 403, message = "Bạn chưa xác nhận OTP hoặc OTP chưa hợp lệ" };

            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.Email == email);
            if (taiKhoan == null)
                return new { statusCode = 404, message = "Không tìm thấy tài khoản" };
            var pub = await _context.HashKeys.FirstOrDefaultAsync();
            if (pub == null || string.IsNullOrWhiteSpace(pub.PublicKey))
                return new { statusCode = 500, message = "Không tìm thấy khóa mã hóa" };
            string haspas = ThinhService.Encrypt(matKhau, pub.PublicKey);
            taiKhoan.MatKhau = haspas;

            try
            {
                _context.Update(taiKhoan);
                await _context.SaveChangesAsync();
                _otpConfirmed.Remove(email); // reset sau khi đổi mật khẩu
                return new { statusCode = 200, message = "Đổi mật khẩu thành công" };
            }
            catch (Exception ex)
            {
                return new { statusCode = 500, message = "Đổi mật khẩu thất bại: " + ex.Message };
            }
        }


        // ================== ĐĂNG NHẬP ==================
        public async Task<dynamic> DangNhap(string TaiKhoan, string MatKhauBase64Rsa)
        {
            // 1) Validate
            if (string.IsNullOrWhiteSpace(TaiKhoan) || string.IsNullOrWhiteSpace(MatKhauBase64Rsa))
                return new { statusCode = 500, message = "Thông tin không được để trống" };
            //
            if (!ThinhService.IsValidEmail(TaiKhoan))
            {
                return new
                {
                    statusCode = 500,
                    message = "Email không hợp lệ!"
                };
            }
            // 2) Tìm tài khoản theo Email hoặc SĐT
            var tk = await _context.TaiKhoans
                .FirstOrDefaultAsync(x => x.Email == TaiKhoan || x.SoDienThoai == TaiKhoan);
            if (tk == null)
                return new { statusCode = 500, message = "Tài khoản không tồn tại" };

            // 3) Đang bị khóa?
            if (tk.ThoiGianKhoaMatKhau.HasValue && tk.ThoiGianKhoaMatKhau.Value > DateTime.Now)
            {
                var conLai = tk.ThoiGianKhoaMatKhau.Value - DateTime.Now;
                return new
                {
                    statusCode = 500,
                    message = $"Tài khoản đang bị khóa. Thử lại sau {Math.Ceiling(conLai.TotalMinutes)} phút."
                };
            }

            // 4) Giải mã mật khẩu bằng PRIVATE KEY (PKCS#8 Base64) – OAEP-SHA256
            var privateKey = await _context.HashKeys
                .AsNoTracking()
                .Select(k => k.PrivateKey)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(privateKey))
                return new { statusCode = 500, message = "Không tìm thấy khóa giải mã" };

            string mkPlain;
            try
            {
                mkPlain = ThinhService.Decrypt(tk.MatKhau, privateKey);
            }
            catch
            {
                await SetThoiGianKhoa(tk.Id);
                return new { statusCode = 500, message = "Mật khẩu không hợp lệ (giải mã thất bại)" };
            }

            // 5) So sánh mật khẩu
            if (mkPlain != MatKhauBase64Rsa)
            {
                await SetThoiGianKhoa(tk.Id);
                return new { statusCode = 500, message = "Tài khoản/mật khẩu không đúng" };
            }

            // 6) Reset bộ đếm/khóa   ok
            tk.SoLanNhapSaiMatKhau = 0;
            tk.ThoiGianKhoaMatKhau = null;
            await _context.SaveChangesAsync();

            // 7 tạo token
            string idTaiKhoan = tk.Id.ToString();
            var token = _thinhService.GenerateToken(idTaiKhoan, "user");

            // 8) Trả kết quả kèm data
            return new
            {
                statusCode = 200,
                message = "Đăng nhập thành công",
                token = token,
                loaiTaiKhoan = tk.LoaiTaiKhoanId
            };
        }

        // ================== SET THỜI GIAN KHÓA ==================
        public async Task<dynamic> SetThoiGianKhoa(long idTaiKhoan)
        {
            var tk = await _context.TaiKhoans.FirstOrDefaultAsync(x => x.Id == idTaiKhoan);
            if (tk == null)
                return new { statusCode = 500, message = "Không tìm thấy tài khoản" };

            // Hết hạn khóa cũ -> reset
            if (tk.ThoiGianKhoaMatKhau.HasValue && tk.ThoiGianKhoaMatKhau.Value <= DateTime.Now)
            {
                tk.ThoiGianKhoaMatKhau = null;
                tk.SoLanNhapSaiMatKhau = 0;
            }

            tk.SoLanNhapSaiMatKhau += 1;
            if (tk.SoLanNhapSaiMatKhau >= MAX_SAI)
                tk.ThoiGianKhoaMatKhau = DateTime.Now.Add(KHOA_30P);

            await _context.SaveChangesAsync();
            return new { statusCode = 200, message = "Cập nhật thành công" };
        }
    }
}
 

    