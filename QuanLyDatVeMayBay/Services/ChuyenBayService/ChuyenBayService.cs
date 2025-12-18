using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Config;
using QuanLyDatVeMayBay.Models.Entities;
using QuanLyDatVeMayBay.Models.Model;
using StackExchange.Redis;
using System.Globalization;
using System.Text.Json;

namespace QuanLyDatVeMayBay.Services.ChuyenBayService
{
    public interface IChuyenBayService
    {
        Task<dynamic> LayDanhSachChuyenBay(TimChuyenBayRequest model);
        Task<dynamic> DanhSachGheTheoChuyenBay(long IdLichBay, long IdChuyenBay);
        Task<dynamic> SetGheNgoi(List<long> idGheNgoi, long idTaiKhoan, long idLichBay);
        Task<dynamic> ReleaseSeat(List<long> idGheNgoi, long idTaiKhoan, long idLichBay);
        Task<dynamic> HuyVe(long idDatVe, string lyDoHuy);
         Task<dynamic> CheckIn(string id);
    }
    public class ChuyenBayService : IChuyenBayService
    {
        private readonly ThinhContext _context;
        private readonly RedisService _redis;
        private readonly IHubContext<NotificationHub> _hub;
        public ChuyenBayService(ThinhContext context, RedisService redis, IHubContext<NotificationHub> hub)
        {
            _context = context;
            _redis = redis;
            _hub = hub;
        }
        public async Task<dynamic> LayDanhSachChuyenBay(TimChuyenBayRequest model)
        {
            if (model == null)
                return new { statusCode = false, message = "Dữ liệu tìm chuyến bay không hợp lệ" };

            if (!DateTime.TryParseExact(model.NgayDi, "dd-MM-yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var ngayDiLocal))
                return new { statusCode = false, message = "Ngày đi không hợp lệ" };

            var sanBayDi = await _context.SanBays.FindAsync(model.MaSanBayDi);
            var sanBayDen = await _context.SanBays.FindAsync(model.MaSanBayDen);
            if (sanBayDi == null || sanBayDen == null)
                return new { statusCode = false, message = "Sân bay không tồn tại" };

            var start_UTC_ngayDi = ThinhService.TimeZoneHelper.ToUtc(ngayDiLocal.Date, sanBayDi.TimeZoneId);
            var end_UTC_ngayDi = ThinhService.TimeZoneHelper.ToUtc(ngayDiLocal.Date.AddDays(1).AddTicks(-1), sanBayDi.TimeZoneId);

            var selectedAmenities = model.idTienNghi.Split(',');

            var flightsDi = await _context.ChuyenBays
                .Where(cb =>
                    cb.MaSanBayDi == model.MaSanBayDi &&
                    cb.MaSanBayDen == model.MaSanBayDen &&
                    cb.IdHangBayNavigation.Id == model.IdHangBay &&
                    cb.GheNgois.Any(g => g.IdLoaiVe == model.IdLoaiVe) &&
                    cb.LichBays.Any(lb => lb.ThoiGianOsanBayDiUtc >= start_UTC_ngayDi &&
                                           lb.ThoiGianOsanBayDiUtc <= end_UTC_ngayDi &&
                                           lb.BangGiaVes.Any(bg =>
                                                bg.IdLoaiVe == model.IdLoaiVe &&
                                                bg.Gia >= model.GiaMin &&
                                                bg.Gia <= model.GiaMax
                                           )
                                           ) &&     
                    cb.TienNghiChuyenMayBays.Any(tn => selectedAmenities.Contains(tn.IdTienNghi.ToString()))
                )
                .GroupBy(cb => new
                {
                    cb.IdHangBayNavigation.Id,
                    cb.IdHangBayNavigation.TenHang,
                    cb.MaSanBayDi,
                    TenSanBayDi = cb.MaSanBayDiNavigation.Ten,
                    cb.MaSanBayDen,
                    TenSanBayDen = cb.MaSanBayDenNavigation.Ten
                })
                .Select(g => new
                {
                    HangBay = new
                    {
                        Id = g.Key.Id,
                        TenHang = g.Key.TenHang
                    },
                    SanBayDi = new
                    {
                        MaSanBayDi = g.Key.MaSanBayDi.Trim(),
                        Ten = g.Key.TenSanBayDi
                    },
                    SanBayDen = new
                    {
                        MaSanBayDen = g.Key.MaSanBayDen.Trim(),
                        Ten = g.Key.TenSanBayDen
                    },
                    LichBay = g.SelectMany(cb => cb.LichBays
                        .Where(lb => lb.ThoiGianOsanBayDiUtc >= start_UTC_ngayDi &&
                                     lb.ThoiGianOsanBayDiUtc <= end_UTC_ngayDi))
                    .OrderBy(lb => lb.ThoiGianOsanBayDiUtc)
                        .Select(lb => new
                        {
                            lb.Id,
                            lb.IdTuyenBay,
                            lb.ThoiGianOsanBayDiUtc,
                            lb.ThoiGianOsanBayDenUtc,
                            lb.ThoiGianBay,
                            Gia = lb.BangGiaVes
                                .Where(bg => bg.IdLoaiVe == model.IdLoaiVe)
                                .Select(bg => bg.Gia)
                                .FirstOrDefault(),
                            soLuongGhe = lb.GheNgoiLichBays
                                .Where(g=>g.IdGheNgoiNavigation.IdLoaiVe == model.IdLoaiVe 
                                && g.TrangThai !=2
                                && g.IdGheNgoiNavigation.IdChuyenBay == lb.IdTuyenBay).Count(),
                            TenTienIch = lb.IdTuyenBayNavigation.TienNghiChuyenMayBays
                                .Where(tn => selectedAmenities.Contains(tn.IdTienNghi.ToString()))
                                .Select(tn => tn.IdTienNghiNavigation.TenTienIch)
                        }).
                        ToList()
                })
                .ToListAsync();

            return new
            {
                statusCode = 200,
                message = "Lấy danh sách chuyến bay thành công",
                data = flightsDi
            };
        }
    
        public async Task<dynamic> DanhSachGheTheoChuyenBay(long IdLichBay,long IdChuyenBay)
        {
            if (IdLichBay <= 0)
                return new { statusCode = 500, message = "Dữ liệu không hợp lệ" };

            string redisKey = $"FlightSeats:{IdLichBay}";
            List<GheNgoiDto> danhSachGhe;


            var redisData = await _redis.Db.HashGetAllAsync(redisKey);
            if (redisData.Any())
            {
                danhSachGhe = redisData
                    .Select(x => JsonSerializer.Deserialize<GheNgoiDto>(x.Value))
                    .ToList();
            }
            else
            {
                danhSachGhe = await _context.GheNgoiLichBays
                    .AsNoTracking()
                    .Where(r => r.IdLichBay == IdLichBay && r.IdGheNgoiNavigation.IdChuyenBay == IdChuyenBay)
                    .Select(r => new GheNgoiDto
                    {
                        Id = r.IdGheNgoi.Value,
                        IdChuyenBay = r.IdGheNgoiNavigation.IdChuyenBay,
                        IdLoaiVe = r.IdGheNgoiNavigation.IdLoaiVe,
                        SoGhe = r.IdGheNgoiNavigation.SoGhe,
                        IdTrangThai = r.TrangThai,
                    })
                    .ToListAsync();

                foreach (var ghe in danhSachGhe)
                {
                    await _redis.Db.HashSetAsync(redisKey, ghe.Id.ToString(), JsonSerializer.Serialize(ghe));
                }

                await _redis.Db.KeyExpireAsync(redisKey, TimeSpan.FromHours(1));
            }

            var gheLoai1 = danhSachGhe.Where(g => g.IdLoaiVe == 1).ToList();
            var gheLoai3 = danhSachGhe.Where(g => g.IdLoaiVe == 3).ToList();
            var gheLoai4 = danhSachGhe.Where(g => g.IdLoaiVe == 4).ToList();

            return new
            {
                statusCode = 200,
                message = "Lấy danh sách ghế ngồi thành công",
                data = new
                {
                    LoaiVe1 = gheLoai1,
                    LoaiVe3 = gheLoai3,
                    LoaiVe4 = gheLoai4
                }
            };
        }        
        public async Task<dynamic> SetGheNgoi(List<long> idGheNgoi, long idTaiKhoan, long idLichBay)
        {
            if (idGheNgoi == null || !idGheNgoi.Any() || idTaiKhoan <= 0)
                return new { statusCode = 400, message = "Dữ liệu không hợp lệ" };

            var lockedSeats = new List<long>();
            var failedSeats = new List<long>();
            string redisHashKey = $"FlightSeats:{idLichBay}";

            try
            {
                foreach (var idGhe in idGheNgoi)
                {
                    string lockKey = $"SeatHold:{idGhe}";

                    var seatJson = await _redis.Db.HashGetAsync(redisHashKey, idGhe.ToString());
                    if (seatJson.IsNullOrEmpty)
                    {
                        failedSeats.Add(idGhe);
                        continue;
                    }

                    var seat = JsonSerializer.Deserialize<GheNgoiDto>(seatJson);
                    if (seat.IdTrangThai != 0)
                    {
                        failedSeats.Add(idGhe);
                        continue;
                    }

                    //Tạo thông tin giữ ghế
                    var holdInfo = new SeatHoldInfo
                    {
                        UserId = idTaiKhoan,
                        HoldTime = DateTime.UtcNow,
                        ExpireAt = DateTime.UtcNow.AddMinutes(5)
                    };

                    string lockVal = JsonSerializer.Serialize(holdInfo);

                    bool isLocked = await _redis.Db.StringSetAsync(
                        lockKey,
                        lockVal,
                        TimeSpan.FromMinutes(5),
                        When.NotExists
                    );

                    if (!isLocked)
                    {
                        failedSeats.Add(idGhe);
                    }
                    else
                    {

                        seat.IdTrangThai = 2; 
                        await _redis.Db.HashSetAsync(redisHashKey, idGhe.ToString(), JsonSerializer.Serialize(seat));

                        lockedSeats.Add(idGhe);
                    }
                }

                if (failedSeats.Any())
                {
                    foreach (var seatId in lockedSeats)
                    {
                        await _redis.Db.KeyDeleteAsync($"SeatHold:{seatId}");
                        var seatJson = await _redis.Db.HashGetAsync(redisHashKey, seatId.ToString());
                        if (!seatJson.IsNullOrEmpty)
                        {
                            var seat = JsonSerializer.Deserialize<GheNgoiDto>(seatJson);
                            seat.IdTrangThai = 1;
                            await _redis.Db.HashSetAsync(redisHashKey, seatId.ToString(), JsonSerializer.Serialize(seat));
                        }
                    }

                    return new
                    {
                        statusCode = 409,
                        message = "Một số ghế đã bị người khác giữ.",
                        failedSeats
                    };
                }

                await _hub.Clients.Group($"Flight_{idLichBay}")
                .SendAsync("ReceiveGheNgoiUpdate", new
                { 
                    Seats = lockedSeats,
                    IdTrangThai = 2,
                    UserId = idTaiKhoan,
                    TimeRemaining = 300
                });

                return new
                {
                    statusCode = 200,
                    message = "Giữ ghế thành công.",
                    lockedSeats
                };
            }
            catch (Exception ex)
            {
                foreach (var seatId in lockedSeats)
                {
                    await _redis.Db.KeyDeleteAsync($"SeatHold:{seatId}");

                    var seatJson = await _redis.Db.HashGetAsync(redisHashKey, seatId.ToString());
                    if (!seatJson.IsNullOrEmpty)
                    {
                        var seat = JsonSerializer.Deserialize<GheNgoiDto>(seatJson);
                        seat.IdTrangThai = 1;
                        await _redis.Db.HashSetAsync(redisHashKey, seatId.ToString(), JsonSerializer.Serialize(seat));
                    }
                }

                Console.WriteLine($"Lỗi SetGheNgoi: {ex.Message}\n{ex.StackTrace}");

                return new
                {
                    statusCode = 500,
                    message = "Có lỗi xảy ra khi giữ ghế. Vui lòng thử lại.",
                    error = ex.Message
                };
            }
        }

        public async Task<dynamic> ReleaseSeat(List<long> idGheNgoi, long idTaiKhoan, long idLichBay)
        {
            if (idGheNgoi == null || !idGheNgoi.Any())
                return new { statusCode = 400, message = "Danh sách ghế không hợp lệ" };

            var releasedSeats = new List<long>();
            string redisHashKey = $"FlightSeats:{idLichBay}";

            try
            {
                foreach (var idGhe in idGheNgoi)
                {
                    string lockKey = $"SeatHold:{idGhe}";

                    var holdJson = await _redis.Db.StringGetAsync(lockKey);
                    if (string.IsNullOrEmpty(holdJson))
                        continue; 

                    var holdInfo = JsonSerializer.Deserialize<SeatHoldInfo>(holdJson);

                    if (holdInfo.UserId != idTaiKhoan)
                        continue;

                    await _redis.Db.KeyDeleteAsync(lockKey);

                    var seatJson = await _redis.Db.HashGetAsync(redisHashKey, idGhe.ToString());
                    if (!seatJson.IsNullOrEmpty)
                    {
                        var seat = JsonSerializer.Deserialize<GheNgoiDto>(seatJson);
                        seat.IdTrangThai = 0;
                        await _redis.Db.HashSetAsync(redisHashKey, idGhe.ToString(), JsonSerializer.Serialize(seat));
                        releasedSeats.Add(idGhe);
                    }
                }

                if (releasedSeats.Any())
                {
                    await _hub.Clients.Group($"Flight_{idLichBay}")
                        .SendAsync("ReceiveGheNgoiUpdate", new  
                        {
                            Seats = releasedSeats,
                            IdTrangThai = 1,
                            UserId = idTaiKhoan,  
                            TimeRemaining = 0
                        });
                }

                return new
                {
                    statusCode = 200,
                    message = "Trả ghế thành công.",
                    releasedSeats
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi ReleaseSeat: {ex.Message}\n{ex.StackTrace}");

                return new
                {
                    statusCode = 500,
                    message = "Có lỗi xảy ra khi trả ghế.",
                    error = ex.Message
                };
            }
        }
        public async Task<dynamic> HuyVe(long idDatVe, string lyDoHuy)
        {
            if (idDatVe <= 0)
                return new { statusCode = 400, message = "Dữ liệu không hợp lệ" };

            try
            {
                var datVe = await _context.DatVes.FindAsync(idDatVe);
                if (datVe == null)
                    return new { statusCode = 404, message = "Đặt vé không tồn tại" };

                if (datVe.TrangThai == "đã hủy")
                    return new { statusCode = 400, message = "Vé đã được hủy trước đó" };

                if (datVe.TrangThai == "đã checkin")
                    return new { statusCode = 400, message = "Vé đã check-in, không thể hủy" };

                var lichBay = await _context.LichBays.FindAsync(datVe.LichBayId);
                if (lichBay == null)
                    return new { statusCode = 404, message = "Lịch bay không tồn tại" };

                // Check time — UTC vs UTC
                if (lichBay.ThoiGianOsanBayDiUtc <= DateTime.UtcNow.AddHours(24))
                    return new { statusCode = 400, message = "Còn dưới 24h trước giờ bay, không thể hủy vé" };

                datVe.TrangThai = "đã hủy";
                datVe.LyDoHuy = lyDoHuy;
                datVe.NgayHuy = DateTime.UtcNow;
                //_context.DatVes.Update(datVe);

                var datVeChiTiet = await _context.ChiTietDatVes.Where(r => r.IdDatVe == idDatVe).ToListAsync();

                foreach (var item in datVeChiTiet)
                {
                    var ghe = await _context.GheNgoiLichBays.Where(c => c.IdLichBay == lichBay.Id && c.IdGheNgoi == item.IdGheNgoi).FirstOrDefaultAsync();
                    if (ghe != null)
                    {
                        ghe.TrangThai = 0;
                        _context.GheNgoiLichBays.Update(ghe);

                        // Redis update
                        string redisKey = $"FlightSeats:{ghe.IdLichBay}";
                        var seatJson = await _redis.Db.HashGetAsync(redisKey, ghe.IdGheNgoi.ToString());
                        if (!seatJson.IsNullOrEmpty)
                        {
                            var seat = JsonSerializer.Deserialize<GheNgoiDto>(seatJson);
                            seat.IdTrangThai = 0;
                            await _redis.Db.HashSetAsync(redisKey, ghe.Id.ToString(), JsonSerializer.Serialize(seat));
                        }

                        await _hub.Clients.Group($"Flight_{ghe.IdLichBay}")
                            .SendAsync("ReceiveGheNgoiUpdate", new
                            {
                                Seats = new List<long> { ghe.Id },
                                IdTrangThai = 0,
                                UserId = 0,
                                TimeRemaining = 0
                            });
                    }
                }

                await _context.SaveChangesAsync();
                return new { statusCode = 200, message = "Hủy vé thành công" };
            }
            catch (Exception ex)
            {
                return new { statusCode = 500, message = "Có lỗi xảy ra khi hủy vé", error = ex.Message };
            }
        }
        public async Task<dynamic> CheckIn(string id)
        {
            if(id == null || id == "")
                return new { statusCode = 400, message = "Dữ liệu không hợp lệ" };
            try
            {
                var key = await _context.HashKeys.FirstOrDefaultAsync();
                var idDatVeString = ThinhService.Decrypt(id, key.PrivateKey);
                long idDatVe = long.Parse(idDatVeString);
                var datVe = await _context.DatVes.FirstOrDefaultAsync(r => r.Id == idDatVe);
                var ngayBay = await _context.LichBays.FirstOrDefaultAsync(r => r.Id == datVe.LichBayId);
                if (datVe == null)
                    return new { statusCode = 404, message = "Đặt vé không tồn tại" };
                //if(ngayBay == null)
                //    return new { statusCode = 404, message = "Lịch bay không tồn tại" };
                //if (ngayBay.ThoiGianOsanBayDiUtc <= DateTime.Now)
                //{
                //    return new { statusCode = 400, message = "Chưa đến ngày bay, không thể check-in" };
                //}
                datVe.TrangThai = "đã checkin";
                _context.DatVes.Update(datVe);
                await _context.SaveChangesAsync();
                return new { statusCode = 200, message = "Check-in thành công" };
            }
            catch (Exception ex)
            {
                return new { statusCode = 500, message = "Có lỗi xảy ra khi check-in", error = ex.Message };
            }
        }
    }
}
