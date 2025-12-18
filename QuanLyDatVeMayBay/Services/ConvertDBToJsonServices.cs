using Microsoft.EntityFrameworkCore;
using QuanLyDatVeMayBay.Models.Entities;
using Newtonsoft.Json;

public class ConvertDBToJsonServices
{
    private readonly ThinhContext _context;
    private readonly IWebHostEnvironment _hostingEnvironment;


    public ConvertDBToJsonServices(ThinhContext context, IWebHostEnvironment hostingEnvironment)
    {
        _context = context;
        _hostingEnvironment = hostingEnvironment;
    }

    // Convert Tỉnh
    public async Task ConvertTinhToJson()
    {
        var data = await _context.Tinhs
            .Select(x => new
            {
                id = x.IdTinh.ToString(),
                ten = x.TenTinh
            })
            .OrderBy(x => x.ten)
            .ToListAsync();

        string json = JsonConvert.SerializeObject(data);
        string folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Models", "Config", "data");
        Directory.CreateDirectory(folderPath);
        string filePath = Path.Combine(folderPath, "Tinh.json");
        await File.WriteAllTextAsync(filePath, json);
    }
    // Convert Quận/Huyện
    public async Task ConvertTienNghiToJson()
    {
        var data = await _context.TienNghis
            .Select(x => new
            {
                id = x.Id.ToString(),
                ten = x.TenTienIch,
            })
            .OrderBy(x => x.ten)
            .ToListAsync();

        string json = JsonConvert.SerializeObject(data);
        string folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Models", "Config", "data");
        Directory.CreateDirectory(folderPath);
        string filePath = Path.Combine(folderPath, "TienNghi.json");
        await File.WriteAllTextAsync(filePath, json);
    }

    // Convert Quận/Huyện
    public async Task ConvertQuanToJson()
    {
        var data = await _context.Quans
            .Select(x => new
            {
                id = x.IdQuan.ToString(),
                ten = x.TenQuan,
                idTinh = x.IdTinh.ToString()
            })
            .OrderBy(x => x.ten)
            .ToListAsync();

        string json = JsonConvert.SerializeObject(data);
        string folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Models", "Config", "data");
        Directory.CreateDirectory(folderPath);
        string filePath = Path.Combine(folderPath, "Quan.json");
        await File.WriteAllTextAsync(filePath, json);
    }

    // Convert Phường/Xã
    public async Task ConvertPhuongToJson()
    {
        var data = await _context.Phuongs
            .Select(x => new
            {
                id = x.IdPhuong.ToString(),
                ten = x.TenPhuong,
                idQuan = x.IdQuan.ToString()
            })
            .OrderBy(x => x.ten)
            .ToListAsync();

        string json = JsonConvert.SerializeObject(data);
        string folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Models", "Config", "data");
        Directory.CreateDirectory(folderPath);
        string filePath = Path.Combine(folderPath, "Phuong.json");
        await File.WriteAllTextAsync(filePath, json);
    }
    // Convert Quốc tịch
    public async Task ConvertQuocTichToJson()
    {
        var data = await _context.QuocTiches
            .Select(x => new
            {
                id = x.Id.ToString(),
                ten = x.QuocTich1,
            })
            .OrderBy(x => x.ten)
            .ToListAsync();
        string json = JsonConvert.SerializeObject(data);
        string folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Models", "Config", "data");
        Directory.CreateDirectory(folderPath);
        string filePath = Path.Combine(folderPath, "QuocTich.json");
        await File.WriteAllTextAsync(filePath, json);
    }
    // Convert Chuyến 
    public async Task ConvertSanBayToJson()
    {
        var data = await _context.SanBays
            .Select(x => new
            {
                Ma = x.MaIata
            })
            .OrderBy(x => x.Ma)
            .ToListAsync();
        string json = JsonConvert.SerializeObject(data);
        string folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Models", "Config", "data");
        Directory.CreateDirectory(folderPath);
        string filePath = Path.Combine(folderPath, "SanBay.json");
        await File.WriteAllTextAsync(filePath, json);
    }
    // Convert HangBay
    public async Task ConvertHangBayToJson()
    {
        var data = await _context.HangBays
            .Select(x => new
            {
                id = x.Id.ToString(),
                ten = x.TenHang
            })
            .OrderBy(x => x.ten)
            .ToListAsync();
        string json = JsonConvert.SerializeObject(data);
        string folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Models", "Config", "data");
        Directory.CreateDirectory(folderPath);
        string filePath = Path.Combine(folderPath, "HangBay.json");
        await File.WriteAllTextAsync(filePath, json);
    }
}
