using QuanLyDatVeMayBay.Models.Entities;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Security.Cryptography;
using System.Text;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace QuanLyDatVeMayBay.Services
{
    public class ThinhService
    {
        private readonly ThinhContext _context;
        private readonly IAmazonS3 _s3Client;
        private readonly AwsSetting _awsSetting;
        private readonly IConfiguration _config;
        public ThinhService(ThinhContext thinhContext, AwsSetting awsSetting, IAmazonS3 amazonS3, IConfiguration config)
        {
            _context = thinhContext;
            _awsSetting = awsSetting;
            _s3Client = amazonS3;
            _config = config;
        }
        public async Task<dynamic> GuiEmail(string Email, string TieuDe, string NoiDung)
        {
            EmailSetting? emailSetting = await _context.EmailSettings.AsNoTracking().FirstOrDefaultAsync();
            try
            {
                // lấy thông tin email gửi
                var email = emailSetting?.SmtpUsername;
                var password = emailSetting?.SmtpPassword;
                var host = "smtp.gmail.com";
                var port = 587;

                using var smtpClient = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(email, password)
                };

                var message = new MailMessage(email!, Email, TieuDe, NoiDung);
                message.IsBodyHtml = true;
                await smtpClient.SendMailAsync(message);

                return new
                {
                    statusCode = 200,
                    message = "Gửi đường dẫn thay đổi mật khẩu vui lòng kiểm tra Email để thực hiện thay đổi mật khẩu!"
                };
            }
            catch (Exception ex)
            {
                // Log ra console (có thể thay bằng logger hoặc lưu DB)
                Console.WriteLine($"[GuiEmail] Lỗi gửi email: {ex}");

                return new
                {
                    statusCode = 500,
                    message = $"Gửi thất bại! Lý do: {ex.Message}"
                };
            }
        }

        public async Task<dynamic> GuiEmail_WithQRCoder(string Email, string TieuDe, string NoiDungHtmlWithCid, byte[] qrCodeBytes)
        {
            var emailSetting = _context.EmailSettings.AsNoTracking().FirstOrDefault();
            try
            {
                var emailFrom = emailSetting.SmtpUsername;
                var password = emailSetting.SmtpPassword;
                var host = "smtp.gmail.com";
                var port = 587;

                using var smtpClient = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(emailFrom, password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailFrom),
                    Subject = TieuDe,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(Email);

                // Gắn nội dung HTML với cid
                var htmlView = AlternateView.CreateAlternateViewFromString(NoiDungHtmlWithCid, null, MediaTypeNames.Text.Html);

                // Đính ảnh QR dạng linked resource
                var imageResource = new LinkedResource(new MemoryStream(qrCodeBytes), MediaTypeNames.Image.Jpeg)
                {
                    ContentId = "MyQrCode",
                    TransferEncoding = TransferEncoding.Base64
                };
                htmlView.LinkedResources.Add(imageResource);

                mailMessage.AlternateViews.Add(htmlView);

                await smtpClient.SendMailAsync(mailMessage);

                return new
                {
                    statusCode = 200,
                    message = "Email đã gửi thành công"
                };
            }
            catch (Exception)
            {
                return new
                {
                    statusCode = 500,
                    message = "Gửi thất bại!"
                };
            }
        }
        public static string Encrypt(string plainText, string base64PublicKey)
        {
            byte[] publicKeyBytes = Convert.FromBase64String(base64PublicKey);
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
                byte[] encrypted = rsa.Encrypt(Encoding.UTF8.GetBytes(plainText), RSAEncryptionPadding.OaepSHA256);
                return Convert.ToBase64String(encrypted);
            }
        }

        public static string Decrypt(string encryptedData, string base64PrivateKey)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
            byte[] privateKeyBytes = Convert.FromBase64String(base64PrivateKey);
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
                byte[] decrypted = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);
                return Encoding.UTF8.GetString(decrypted);
            }
        }

        //public string GenerateBase64QRCode(string content)
        //{
        //    var key = _context.HashKeys.AsNoTracking().FirstOrDefault();
        //    byte[] encryptedContent = Encrypt(content, key.PublicKey);
        //    string encryptedContentBase64 = Convert.ToBase64String(encryptedContent);
        //    using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        //    using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(encryptedContentBase64, QRCodeGenerator.ECCLevel.Q))
        //    using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        //    {
        //        byte[] qrCodeImage = qrCode.GetGraphic(20);
        //        string base64String = Convert.ToBase64String(qrCodeImage);
        //        return $"data:image/png;base64,{base64String}";
        //    }

        //}
        //public string GenerateBase64(string content)
        //{
        //    using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        //    using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q))
        //    using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        //    {
        //        byte[] qrCodeImage = qrCode.GetGraphic(20);
        //        string base64String = Convert.ToBase64String(qrCodeImage);
        //        return $"data:image/png;base64,{base64String}";
        //    }

        ////}
        public byte[] GenerateQRCodeBytes(string content)
        {
            var key = _context.HashKeys.AsNoTracking().FirstOrDefault();
            string encryptedContent = Encrypt(content, key.PublicKey);
            string encryptedContentBase64 = encryptedContent;
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(encryptedContentBase64, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                return qrCode.GetGraphic(20);
            }
        }
        //public string GiaiMaQRCode(string base64Data)
        //{
        //    byte[] encryptedBytes = Convert.FromBase64String(base64Data);
        //    var key = _context.HashKeys.AsNoTracking().FirstOrDefault();
        //    if (key == null) throw new Exception("Không tìm thấy khóa.");
        //    return Decrypt(encryptedBytes, key.PrivateKey); ;
        //}
        // ---------------- HELPER ----------------


        public static bool IsValidPhoneNumber(string soDienThoai)
        {
            return !string.IsNullOrEmpty(soDienThoai)
                   && soDienThoai.All(char.IsDigit)
                   && soDienThoai.Length == 10;
        }

        public static bool IsValidEmail(string email)
        {
            return !string.IsNullOrEmpty(email)
                   && email.Contains("@");
        }
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var fileTransferUtility = new TransferUtility(_s3Client);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = fileName,
                BucketName = _awsSetting.BucketName,
                ContentType = contentType,
            };

            await fileTransferUtility.UploadAsync(uploadRequest);

            return $"https://{_awsSetting.BucketName}.s3.{_awsSetting.Region}.amazonaws.com/{fileName}";
        } 
        public string GenerateToken(string userId, string role)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),  
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public static string SinhMaNgauNhien(string TenBang)
        {
            return $"{TenBang}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
        public static string GenerateCodeVerifier()
        {
            var randomBytes = new byte[32];
            RandomNumberGenerator.Fill(randomBytes);
            return Base64UrlEncode(randomBytes);
        }
        public static string GenerateCodeChallenge(string codeVerifier)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
            return Base64UrlEncode(hashBytes);
        }
        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
        public static string LoaiAnh(int i)
        {
            switch (i)
            {
                case 1:
                    return "https://thinh082-images-project.s3.ap-southeast-2.amazonaws.com/Hinh1.png";              
                default:
                    return "application/octet-stream"; 
            }
        }
        public static class TimeZoneHelper
        {
            // Convert UTC sang giờ sân bay (VD: Hanoi UTC+7)
            public static DateTime ToLocal(DateTime utcTime, string airportTimeZoneId)
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(airportTimeZoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
            }

            // Convert giờ sân bay sang UTC
            public static DateTime ToUtc(DateTime localTime, string airportTimeZoneId)
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(airportTimeZoneId);
                return TimeZoneInfo.ConvertTimeToUtc(localTime, tz);
            }
        }
    }
}
