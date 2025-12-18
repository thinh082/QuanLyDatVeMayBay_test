using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Models.Model;

namespace QuanLyDatVeMayBay.Services.ThongBaoService
{
    public interface IThongBaoService
    {
         Task GuiThongBao(long idTaiKhoan, string noiDung, string TieuDe, string? image);
        Task<List<ThongBaoModel>> LayThongBao(long idTaiKhoan);
        Task<dynamic> IsRead(long idThongBao);
        Task<dynamic> ChiTietThongBao(long idThongBao);
        Task<dynamic> XoaThongBao(long idThongBao);
    }
    public class ThongBaoService: IThongBaoService
    {
        private readonly ThinhContext _context;
        public ThongBaoService(ThinhContext context)
        {
            _context = context;
        }
        public async Task GuiThongBao(long idTaiKhoan,string noiDung,string TieuDe,string? image)
        {
            var thongBao = new ThongBao
            {
                NoiDung = noiDung,
                IdTaiKhoan = idTaiKhoan,
                NgayTao = DateTime.Now,
                TieuDe = TieuDe,
                IsRead = false,
                HinhAnh = image
            };
            _context.ThongBaos.Add(thongBao);
            _context.SaveChanges();
        }
        public async Task<dynamic> ChiTietThongBao(long idThongBao)
        {
            var thongBao = await _context.ThongBaos.FirstOrDefaultAsync(r => r.Id == idThongBao);
            if (thongBao == null)
            {
                return new { statusCode = 500, Message = "Thông báo không tồn tại" };
            }
            ThongBaoModel thongBaoModel = new ThongBaoModel
            {
                Id = thongBao.Id,
                TieuDe = thongBao.TieuDe,
                NoiDung = thongBao.NoiDung,
                NgayTao = thongBao.NgayTao,
                HinhAnh = thongBao.HinhAnh
            };
            thongBao.IsRead = true;
            await _context.SaveChangesAsync();
            return new { statusCode = 200, Message = "Lấy thông báo thành công", data = thongBaoModel };
        }
        public async Task<List<ThongBaoModel>> LayThongBao(long idTaiKhoan)
        {
            var thongBaos = await _context.ThongBaos.Where(tb => tb.IdTaiKhoan == idTaiKhoan).OrderByDescending(r=>r.NgayTao).ToListAsync();
            List<ThongBaoModel> tHongBaoModel = new List<ThongBaoModel>();
            for(int i = 0; i < thongBaos.Count; i++)
            {
                var thongBaoModel = new ThongBaoModel
                {
                    Id = thongBaos[i].Id,
                    IdTaiKhoan = thongBaos[i].IdTaiKhoan,
                    TieuDe = thongBaos[i].TieuDe,
                    NoiDung = thongBaos[i].NoiDung,
                    IsRead = thongBaos[i].IsRead
                };
                tHongBaoModel.Add(thongBaoModel);
            }
            return tHongBaoModel;

        }
        public async Task<dynamic> IsRead(long idThongBao)
        {
            var thongBao = await _context.ThongBaos.FirstOrDefaultAsync(r=>r.Id == idThongBao);
            if (thongBao == null)
            {
                return new { statusCode = 500, Message = "Thông báo không tồn tại" };
            }
            thongBao.IsRead = true;
            await _context.SaveChangesAsync();
            return new { statusCode = 200, Message = "Đã đọc thông báo" };
        }
        public async Task<dynamic> XoaThongBao(long idThongBao)
        {
            if (idThongBao == 0)
            {
                return new { statusCode = 500, Message = "Id thông báo không hợp lệ" };
            }
            var thongBao = await _context.ThongBaos.FirstOrDefaultAsync(r => r.Id == idThongBao);
            if (thongBao == null)
            {
                return new { statusCode = 500, Message = "Thông báo không tồn tại" };
            }
            _context.ThongBaos.Remove(thongBao);
            await _context.SaveChangesAsync();
            return new
            {
                statusCode = 200,
                Message = "Xóa thông báo thành công"
            };
        }
    }

}
