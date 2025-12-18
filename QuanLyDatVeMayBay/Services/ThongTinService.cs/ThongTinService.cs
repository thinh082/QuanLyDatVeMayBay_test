using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Models.Model;

namespace QuanLyDatVeMayBay.Services.ThongTinService.cs
{
    public interface IThongTinService
    {
        Task<dynamic> ThongTinCoBan(long idTaiKhoan);
        Task<dynamic> UpLoadAvt(IFormFile file, long idTaiKhoan);
        Task<dynamic> ThongTinCapNhat(long IdTaiKhoan);
        Task<dynamic> CapNhatThongTin(CapNhatThongTinModel model, long IdTaiKhoan);
        Task<dynamic> ThongTinCCCD(long IdTaiKhoan);
        Task<dynamic> CapNhatCCCD(CapNhatCCCDModel model, long idTaiKhoan);
        Task<dynamic> ThongTinPassport(long IdTaiKhoan);
		Task<dynamic> CapNhatPassport(CapNhatPassportModel model, long idTaiKhoan);
    }
    public class ThongTinService: IThongTinService
    {
        private readonly ThinhContext _context;
        private readonly ThinhService _thinhServices;
        public ThongTinService(ThinhContext thinhContext, ThinhService thinhServices)
        {
            _context = thinhContext;
            _thinhServices = thinhServices;
        }
        public async Task<dynamic> ThongTinCoBan(long idTaiKhoan)
        {
            if(idTaiKhoan <= 0)
            {
                return new
                {
                    Success = false,
                    Message = "ID tài khoản không hợp lệ"
                };
            }
            var taiKhoan = await _context.TaiKhoans.FindAsync(idTaiKhoan);
            if (taiKhoan == null)
            {
                return new
                {
                    Success = false,
                    Message = "Tài khoản không tồn tại"
                };
            }
            var ThongTin = await _context.KhachHangs.FirstOrDefaultAsync(r=>r.IdTaiKhoan == taiKhoan.Id);            
            var khachHang = new
            {
                TenKh = ThongTin?.TenKh,
                Email = taiKhoan.Email,
                HinhAnh = taiKhoan.HinhAnh,
                isEmail = taiKhoan.IsEmail,
                idTaiKhoan = taiKhoan.Id,
            };
            return new
            {
                statusCode = 200,
                message = "Lấy thông tin cơ bản thành công",
                data = khachHang
            };
        }
        public async Task<dynamic> ThongTinCapNhat(long IdTaiKhoan)
        {
            if(IdTaiKhoan <= 0)
            {
                return new
                {
                    StatusCode = 500,
                    Message = "ID tài khoản không hợp lệ"
                };
            }
           var thongTin = await _context.TaiKhoans.Where(r=>r.Id == IdTaiKhoan)
                .Select(r => new
                {
                    r.Email,
                    r.SoDienThoai,
                    r.HinhAnh,
                    TenKh = r.KhachHang.TenKh ?? "",
                    DiaChi = r.KhachHang.DiaChi ?? "",
                    IdPhuong = r.KhachHang.IdPhuong ?? 0,
                    IdQuan = r.KhachHang.IdQuan ?? 0,
                    IdTinh = r.KhachHang.IdTinh ?? 0,
                    GioiTinh = r.KhachHang.GioiTinh ?? 0 ,
                    QuocTich = r.KhachHang.IdQuocTich??0
                }).FirstOrDefaultAsync();
            if (thongTin == null)
            {
                return new
                {
                    StatusCode = 500,
                    Message = "Không tìm thấy thông tin khách hàng"
                };
            }
            string? soNha = thongTin?.DiaChi?.Split(" - ")[0];
            return new
            {
                StatusCode = 200,
                Message = "Lấy thông tin khách hàng thành công",
                data = new
                {
                    thongTin?.Email,
                    thongTin?.SoDienThoai,
                    thongTin?.HinhAnh,
                    thongTin?.TenKh,
                    SoNha = soNha ?? "",
                    thongTin?.IdPhuong,
                    thongTin?.IdQuan,
                    thongTin?.IdTinh,
                    GioiTinh = thongTin?.GioiTinh,
                    IdQuocTich = thongTin?.QuocTich
                }
            };

        }
        public async Task<dynamic> ThongTinCCCD(long IdTaiKhoan)
        {
            var thongTin = await _context.TaiKhoans.Where(r => r.Id == IdTaiKhoan)
                .Select(r => new
                {
                    TenKh = r.KhachHang.KhachHangCccd.TenTrenCccd??"",
                    r.KhachHang.GioiTinh,
                    r.KhachHang.KhachHangCccd.SoCccd,
                    r.KhachHang.KhachHangCccd.NoiThuongTru,
                    r.KhachHang.KhachHangCccd.QueQuan,
                    r.KhachHang.IdQuocTich,
                }).FirstOrDefaultAsync();
            if (thongTin == null)
            {
                return new
                {
                    statusCode = 500,
                    Message = "Không tìm thấy thông tin khách hàng"
                };
            }
            return new
            {
                statusCode = 200,
                message ="Lấy thông tin CCCD thành công",
                data = thongTin
            };
        }
        public async Task<dynamic> CapNhatThongTin(CapNhatThongTinModel model,long IdTaiKhoan)
        {
            if (model == null) 
            {
                return new
                {
                    statusCode = 500,
                    Message = "Dữ liệu không hợp lệ"
                };
            }
            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(r => r.Id == IdTaiKhoan);
            if (taiKhoan == null)
            {
                return new
                {
                    statusCode = 500,
                    Message = "Tài khoản không tồn tại"
                };
            }
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(r => r.IdTaiKhoan == IdTaiKhoan);
            if (khachHang == null)
            {
               var kh = new KhachHang
               {
                   IdTaiKhoan = IdTaiKhoan,
                   TenKh = model.TenKh,
                   DiaChi = $"{model.SoNha} - {model.IdPhuong} - {model.IdQuan} - {model.IdTinh}",
                   GioiTinh = model.GioiTinh,
                   IdPhuong = model.IdPhuong,
                   IdQuan = model.IdQuan,
                   IdTinh = model.IdTinh,
                   IdQuocTich = model.IdQuocTich
               };
                _context.KhachHangs.Add(kh);
                await _context.SaveChangesAsync();
                return new
                {
                    statusCode = 200,
                    Message = "Cập nhật thông tin thành công"
                };
            }
            var trans = _context.Database.BeginTransaction();
            try
            {
                taiKhoan.Email = model.Email;
                taiKhoan.SoDienThoai = model.SoDienThoai;
                _context.TaiKhoans.Update(taiKhoan);
                khachHang.TenKh = model.TenKh;
                var phuong = await _context.Phuongs.FirstOrDefaultAsync(p => p.IdPhuong == model.IdPhuong);
                var quan = await _context.Quans.FirstOrDefaultAsync(q => q.IdQuan == model.IdQuan);
                var tinh = await _context.Tinhs.FirstOrDefaultAsync(t => t.IdTinh == model.IdTinh);

                khachHang.DiaChi = $"{model.SoNha} - {phuong?.TenPhuong} - {quan?.TenQuan} - {tinh?.TenTinh}";
                khachHang.IdPhuong = model.IdPhuong;
                khachHang.IdQuan = model.IdQuan;
                khachHang.IdTinh = model.IdTinh;
                khachHang.IdQuocTich = model.IdQuocTich;
                khachHang.GioiTinh = model.GioiTinh;
                _context.KhachHangs.Update(khachHang);
                await _context.SaveChangesAsync();
                trans.Commit();
                return new
                {
                    statusCode = 200,
                    Message = "Cập nhật thông tin thành công"
                };
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return new
                {
                    statusCode = 500,
                    Message = "Lỗi trong quá trình cập nhật thông tin"
                };
            }
        }
        public async Task<dynamic> CapNhatCCCD(CapNhatCCCDModel model, long idTaiKhoan)
        {
            if(model == null)
            {
                return new
                {
                    statusCode = 500,
                    Message = "Dữ liệu không hợp lệ"
                };
            }
            var taiKhoan = await _context.KhachHangs.Where(r => r.IdTaiKhoan == idTaiKhoan).FirstOrDefaultAsync();
            if (taiKhoan == null)
            {
                return new
                {
                    statusCode = 500,
                    Message = "Không tìm thấy thông tin khách hàng"
                };
            }
            var thongTin = await _context.KhachHangCccds.Where(r => r.IdKhachHang == taiKhoan.Id).FirstOrDefaultAsync();
            if (thongTin == null)
            {
                var giayTo = new KhachHangCccd
                {
                    IdKhachHang = taiKhoan.Id,
                    SoCccd = model.SoCCCD,
                    TenTrenCccd = model.TenTrenCCCD,
                    NoiThuongTru = model.NoiThuongTru,
                    QueQuan = model.QueQuan
                };
                _context.KhachHangCccds.Add(giayTo);
                await _context.SaveChangesAsync();
                return new
                {
                    statusCode = 200,
                    Message = "Cập nhật thông tin CCCD thành công"
                };
            }
            else
            {
                thongTin.SoCccd = model.SoCCCD;
                thongTin.TenTrenCccd = model.TenTrenCCCD;
                thongTin.NoiThuongTru = model.NoiThuongTru;
                thongTin.QueQuan = model.QueQuan;

                _context.KhachHangCccds.Update(thongTin);
                await _context.SaveChangesAsync();

                return new
                {
                    statusCode = 200,
                    Message = "Cập nhật thông tin CCCD thành công"
                };
            }
            
        }
        public async Task<dynamic> UpLoadAvt(IFormFile file,long idTaiKhoan) 
        {
            if (file == null || file.Length == 0)
                return new
                {
                    statuCode = 500,
                    Message = "Không có file để upload."
                };
            if (idTaiKhoan <= 0)
            {
                return new
                {
                    statuCode = 500,
                    Message = "Lỗi tài khoản."
                };
            }
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            using var stream = file.OpenReadStream();
            var url = await _thinhServices.UploadFileAsync(stream, fileName, file.ContentType);
            if (string.IsNullOrEmpty(url))
            {
                return new
                {
                    statuCode = 500,
                    Message = "Lỗi trong quá trình upload!."
                };
            }
            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(r => r.Id == idTaiKhoan);
            if (taiKhoan == null)
            {
                return new
                {
                    statuCode = 500,
                    Message = "Lỗi không tìm thấy khách hàng!."
                };
            }
            taiKhoan.HinhAnh = url;
            _context.TaiKhoans.Update(taiKhoan);
            _context.SaveChanges();
            return new
            {
                statusCode = 200,
                Message = "Upload thành công",
                Url = url
            };
        }
        public async Task<dynamic> ThongTinPassport(long IdTaiKhoan)
        {
            var thongTin = await _context.TaiKhoans.Where(r => r.Id == IdTaiKhoan)
                .Select(r => new
                {
                    TenKh = r.KhachHang.TenKh ?? "",
                    r.KhachHang.GioiTinh,
                    r.KhachHang.KhachHangPassports.FirstOrDefault().SoPassport,
                    r.KhachHang.KhachHangPassports.FirstOrDefault().TenTrenPassport,
                    r.KhachHang.KhachHangPassports.FirstOrDefault().NoiCap,
                    r.KhachHang.KhachHangPassports.FirstOrDefault().NgayCap,
                    r.KhachHang.KhachHangPassports.FirstOrDefault().NgayHetHan,
                    r.KhachHang.KhachHangPassports.FirstOrDefault().QuocTich,
                    r.KhachHang.KhachHangPassports.FirstOrDefault().LoaiPassport,
                    r.KhachHang.KhachHangPassports.FirstOrDefault().GhiChu,
                    r.KhachHang.IdQuocTich,
                }).FirstOrDefaultAsync();
            if (thongTin == null)
            {
                return new
                {
                    statusCode = 500,
                    Message = "Không tìm thấy thông tin khách hàng"
                };
            }
            return new
            {
                statusCode = 200,
                message = "Lấy thông tin Passport thành công",
                data = thongTin
            };
        }
		public async Task<dynamic> CapNhatPassport(CapNhatPassportModel model, long idTaiKhoan)
		{
			if (model == null)
			{
				return new
				{
					statusCode = 500,
					Message = "Dữ liệu không hợp lệ"
				};
			}
			var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(r => r.IdTaiKhoan == idTaiKhoan);
			if (khachHang == null)
			{
				return new
				{
					statusCode = 500,
					Message = "Không tìm thấy thông tin khách hàng"
				};
			}
			var passport = await _context.KhachHangPassports.Where(p => p.IdKhachHang == khachHang.Id).FirstOrDefaultAsync();
			if (passport == null)
			{
				var newPassport = new KhachHangPassport
				{
					IdKhachHang = khachHang.Id,
					SoPassport = model.SoPassport ?? string.Empty,
					TenTrenPassport = model.TenTrenPassport,
					NoiCap = model.NoiCap,
					NgayCap = model.NgayCap.HasValue ? DateOnly.FromDateTime(model.NgayCap.Value) : null,
					NgayHetHan = model.NgayHetHan.HasValue ? DateOnly.FromDateTime(model.NgayHetHan.Value) : null,
					QuocTich = model.QuocTich,
					LoaiPassport = model.LoaiPassport,
					GhiChu = model.GhiChu
				};
				_context.KhachHangPassports.Add(newPassport);
				await _context.SaveChangesAsync();
				return new
				{
					statusCode = 200,
					Message = "Cập nhật thông tin Passport thành công"
				};
			}
			passport.SoPassport = model.SoPassport ?? passport.SoPassport;
			passport.TenTrenPassport = model.TenTrenPassport;
			passport.NoiCap = model.NoiCap;
			passport.NgayCap = model.NgayCap.HasValue ? DateOnly.FromDateTime(model.NgayCap.Value) : null;
			passport.NgayHetHan = model.NgayHetHan.HasValue ? DateOnly.FromDateTime(model.NgayHetHan.Value) : null;
			passport.QuocTich = model.QuocTich;
			passport.LoaiPassport = model.LoaiPassport;
			passport.GhiChu = model.GhiChu;

			_context.KhachHangPassports.Update(passport);
			await _context.SaveChangesAsync();
			return new
			{
				statusCode = 200,
				Message = "Cập nhật thông tin Passport thành công"
			};
		}
        

    }
}
