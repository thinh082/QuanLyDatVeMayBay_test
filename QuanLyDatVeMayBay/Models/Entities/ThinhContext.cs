using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class ThinhContext : DbContext
{
    public ThinhContext()
    {
    }

    public ThinhContext(DbContextOptions<ThinhContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AiChatBot> AiChatBots { get; set; }

    public virtual DbSet<Airport> Airports { get; set; }

    public virtual DbSet<ApiLog> ApiLogs { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<AwsSetting> AwsSettings { get; set; }

    public virtual DbSet<BangGiaVe> BangGiaVes { get; set; }

    public virtual DbSet<ChiTietDanhGium> ChiTietDanhGia { get; set; }

    public virtual DbSet<ChiTietDatVe> ChiTietDatVes { get; set; }

    public virtual DbSet<ChiTietPhieuGiamGium> ChiTietPhieuGiamGia { get; set; }

    public virtual DbSet<ChucNang> ChucNangs { get; set; }

    public virtual DbSet<ChuyenBay> ChuyenBays { get; set; }

    public virtual DbSet<DatPhong> DatPhongs { get; set; }

    public virtual DbSet<DatVe> DatVes { get; set; }

    public virtual DbSet<DieuKienGiamGium> DieuKienGiamGia { get; set; }

    public virtual DbSet<EmailSetting> EmailSettings { get; set; }

    public virtual DbSet<FacebookSetting> FacebookSettings { get; set; }

    public virtual DbSet<GheNgoi> GheNgois { get; set; }

    public virtual DbSet<GheNgoiLichBay> GheNgoiLichBays { get; set; }

    public virtual DbSet<GoogleSetting> GoogleSettings { get; set; }

    public virtual DbSet<HangBay> HangBays { get; set; }

    public virtual DbSet<HashKey> HashKeys { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<KhachHangCccd> KhachHangCccds { get; set; }

    public virtual DbSet<KhachHangPassport> KhachHangPassports { get; set; }

    public virtual DbSet<LichBay> LichBays { get; set; }

    public virtual DbSet<LichSuThanhToan> LichSuThanhToans { get; set; }

    public virtual DbSet<LoaiHoatDong> LoaiHoatDongs { get; set; }

    public virtual DbSet<LoaiPhieuGiamGium> LoaiPhieuGiamGia { get; set; }

    public virtual DbSet<LoaiPhuongThucThanhToan> LoaiPhuongThucThanhToans { get; set; }

    public virtual DbSet<LoaiTaiKhoan> LoaiTaiKhoans { get; set; }

    public virtual DbSet<LoaiVe> LoaiVes { get; set; }

    public virtual DbSet<MaLoiVnPay> MaLoiVnPays { get; set; }

    public virtual DbSet<OauthState> OauthStates { get; set; }

    public virtual DbSet<Paypalsetting> Paypalsettings { get; set; }

    public virtual DbSet<PhanQuyenChucNang> PhanQuyenChucNangs { get; set; }

    public virtual DbSet<PhieuGiamGium> PhieuGiamGia { get; set; }

    public virtual DbSet<Phong> Phongs { get; set; }

    public virtual DbSet<Phuong> Phuongs { get; set; }

    public virtual DbSet<Quan> Quans { get; set; }

    public virtual DbSet<QuocTich> QuocTiches { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<SanBay> SanBays { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<ThanhToanCho> ThanhToanChos { get; set; }

    public virtual DbSet<ThanhToanVnpay> ThanhToanVnpays { get; set; }

    public virtual DbSet<ThongBao> ThongBaos { get; set; }

    public virtual DbSet<TienNghi> TienNghis { get; set; }

    public virtual DbSet<TienNghiChuyenMayBay> TienNghiChuyenMayBays { get; set; }

    public virtual DbSet<Tinh> Tinhs { get; set; }

    public virtual DbSet<TrangThaiGheNgoi> TrangThaiGheNgois { get; set; }

    public virtual DbSet<TrangThaiThanhToan> TrangThaiThanhToans { get; set; }

    public virtual DbSet<VeMayBay> VeMayBays { get; set; }

    public virtual DbSet<Vnpaysetting> Vnpaysettings { get; set; }

    public virtual DbSet<ZaloSetting> ZaloSettings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:Connection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AiChatBot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AI_ChatB__3214EC07784CA4A6");

            entity.ToTable("AI_ChatBot");

            entity.Property(e => e.CauHoi).HasMaxLength(500);
            entity.Property(e => e.CauTraLoi).HasMaxLength(500);
        });

        modelBuilder.Entity<Airport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Airport__3214EC07A3001C84");

            entity.ToTable("Airport");

            entity.HasIndex(e => e.Code, "UQ__Airport__A25C5AA7726A7FB3").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.TimeZoneId).HasMaxLength(50);
        });

        modelBuilder.Entity<ApiLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ApiLog__3214EC07828CE64D");

            entity.ToTable("ApiLog");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Method).HasMaxLength(10);
            entity.Property(e => e.Path).HasMaxLength(255);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AuditLog__3214EC0774464688");

            entity.ToTable("AuditLog");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.HanhDong).HasMaxLength(50);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.LogAction)
                .HasMaxLength(255)
                .HasColumnName("Log_Action");
            entity.Property(e => e.TenBang).HasMaxLength(100);
        });

        modelBuilder.Entity<AwsSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AWS_Sett__3214EC07EAB5FAC0");

            entity.ToTable("AWS_Setting");

            entity.Property(e => e.AccessKey)
                .HasMaxLength(255)
                .HasColumnName("Access_key");
            entity.Property(e => e.BucketName)
                .HasMaxLength(255)
                .HasColumnName("Bucket_Name");
            entity.Property(e => e.Profile).HasMaxLength(10);
            entity.Property(e => e.Region).HasMaxLength(100);
            entity.Property(e => e.SecretKey)
                .HasMaxLength(255)
                .HasColumnName("Secret_Key");
            entity.Property(e => e.ServiceUrl).HasColumnName("Service_Url");
        });

        modelBuilder.Entity<BangGiaVe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BangGiaV__3214EC071384DB74");

            entity.ToTable("BangGiaVe");

            entity.Property(e => e.Gia).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdLichBayNavigation).WithMany(p => p.BangGiaVes)
                .HasForeignKey(d => d.IdLichBay)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Gia_LichBay");

            entity.HasOne(d => d.IdLoaiVeNavigation).WithMany(p => p.BangGiaVes)
                .HasForeignKey(d => d.IdLoaiVe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Gia_LoaiVe");
        });

        modelBuilder.Entity<ChiTietDanhGium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChiTietD__3214EC07147169ED");

            entity.Property(e => e.ThoiGianDanhGia).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdTaiKhoanNavigation).WithMany(p => p.ChiTietDanhGia)
                .HasForeignKey(d => d.IdTaiKhoan)
                .HasConstraintName("FK_DanhGia_TaiKhoan");
        });

        modelBuilder.Entity<ChiTietDatVe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChiTietD__3214EC07F341258C");

            entity.ToTable("ChiTietDatVe");

            entity.HasIndex(e => new { e.IdDatVe, e.IdGheNgoi }, "UQ_DatVe_Ghe").IsUnique();

            entity.HasOne(d => d.IdDatVeNavigation).WithMany(p => p.ChiTietDatVes)
                .HasForeignKey(d => d.IdDatVe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDa__IdDat__70DDC3D8");

            entity.HasOne(d => d.IdGheNgoiNavigation).WithMany(p => p.ChiTietDatVes)
                .HasForeignKey(d => d.IdGheNgoi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDa__IdGhe__71D1E811");
        });

        modelBuilder.Entity<ChiTietPhieuGiamGium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChiTietP__3214EC07DF79AD13");

            entity.HasOne(d => d.IdMaGiamGiaNavigation).WithMany(p => p.ChiTietPhieuGiamGia)
                .HasForeignKey(d => d.IdMaGiamGia)
                .HasConstraintName("FK_ChiTietGiamGia_PhieuGiamGia");

            entity.HasOne(d => d.IdTaiKhoanNavigation).WithMany(p => p.ChiTietPhieuGiamGia)
                .HasForeignKey(d => d.IdTaiKhoan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietPh__IdTai__08B54D69");
        });

        modelBuilder.Entity<ChucNang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChucNang__3214EC073D7ECFA9");

            entity.ToTable("ChucNang");

            entity.HasIndex(e => e.MaChucNang, "UQ__ChucNang__B26DC2567C27153E").IsUnique();

            entity.Property(e => e.MaChucNang).HasMaxLength(255);
            entity.Property(e => e.MoTa).HasMaxLength(255);
        });

        modelBuilder.Entity<ChuyenBay>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChuyenBa__3214EC072AA3815F");

            entity.ToTable("ChuyenBay");

            entity.Property(e => e.MaSanBayDen)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaSanBayDi)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.IdHangBayNavigation).WithMany(p => p.ChuyenBays)
                .HasForeignKey(d => d.IdHangBay)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChuyenBay__IdHan__5535A963");

            entity.HasOne(d => d.MaSanBayDenNavigation).WithMany(p => p.ChuyenBayMaSanBayDenNavigations)
                .HasForeignKey(d => d.MaSanBayDen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChuyenBay__MaSan__571DF1D5");

            entity.HasOne(d => d.MaSanBayDiNavigation).WithMany(p => p.ChuyenBayMaSanBayDiNavigations)
                .HasForeignKey(d => d.MaSanBayDi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChuyenBay__MaSan__5629CD9C");
        });

        modelBuilder.Entity<DatPhong>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DatPhong__3214EC07ED720022");

            entity.ToTable("DatPhong");

            entity.Property(e => e.MaDatPhong).HasMaxLength(255);
            entity.Property(e => e.NgayDat).HasColumnType("datetime");

            entity.HasOne(d => d.IdPhongNavigation).WithMany(p => p.DatPhongs)
                .HasForeignKey(d => d.IdPhong)
                .HasConstraintName("FK_DatPhong_Phong");

            entity.HasOne(d => d.IdTaiKhoanNavigation).WithMany(p => p.DatPhongs)
                .HasForeignKey(d => d.IdTaiKhoan)
                .HasConstraintName("FK_DatPhong_TaiKhoan");
        });

        modelBuilder.Entity<DatVe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DatVe__3214EC07ABE07748");

            entity.ToTable("DatVe");

            entity.Property(e => e.Gia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IdVe).HasColumnName("idVe");
            entity.Property(e => e.LyDoHuy).HasMaxLength(255);
            entity.Property(e => e.NgayDat).HasColumnType("datetime");
            entity.Property(e => e.NgayHuy).HasColumnType("datetime");
            entity.Property(e => e.TrangThai).HasMaxLength(30);

            entity.HasOne(d => d.IdChuyenBayNavigation).WithMany(p => p.DatVes)
                .HasForeignKey(d => d.IdChuyenBay)
                .HasConstraintName("FK_DatVe_ChuyenBay");

            entity.HasOne(d => d.IdTaiKhoanNavigation).WithMany(p => p.DatVes)
                .HasForeignKey(d => d.IdTaiKhoan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DatVe__IdTaiKhoa__656C112C");

            entity.HasOne(d => d.IdVeNavigation).WithMany(p => p.DatVes)
                .HasForeignKey(d => d.IdVe)
                .HasConstraintName("FK_DatVe_VeMayBay");

            entity.HasOne(d => d.LichBay).WithMany(p => p.DatVes)
                .HasForeignKey(d => d.LichBayId)
                .HasConstraintName("FK_LichBay_DatVe");
        });

        modelBuilder.Entity<DieuKienGiamGium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DieuKien__3214EC0769766D1D");

            entity.Property(e => e.DieuKien).HasMaxLength(255);
        });

        modelBuilder.Entity<EmailSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EmailSet__3214EC07F3B9A725");

            entity.ToTable("EmailSetting");

            entity.Property(e => e.SenderEmail).HasMaxLength(255);
            entity.Property(e => e.SmtpUsername).HasMaxLength(255);
        });

        modelBuilder.Entity<FacebookSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Facebook__3214EC07269DAEB0");

            entity.ToTable("FacebookSetting");

            entity.Property(e => e.AppId).HasMaxLength(255);
            entity.Property(e => e.AppSecret).HasMaxLength(255);
        });

        modelBuilder.Entity<GheNgoi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GheNgoi__3214EC071D19A5E5");

            entity.ToTable("GheNgoi");

            entity.HasIndex(e => new { e.IdChuyenBay, e.SoGhe }, "UQ_GheNgoi").IsUnique();

            entity.Property(e => e.SoGhe).HasMaxLength(10);

            entity.HasOne(d => d.IdChuyenBayNavigation).WithMany(p => p.GheNgois)
                .HasForeignKey(d => d.IdChuyenBay)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GheNgoi__IdChuye__6C190EBB");

            entity.HasOne(d => d.IdLoaiVeNavigation).WithMany(p => p.GheNgois)
                .HasForeignKey(d => d.IdLoaiVe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GheNgoi__IdLoaiV__6D0D32F4");
        });

        modelBuilder.Entity<GheNgoiLichBay>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GheNgoi___3214EC07514EB5FF");

            entity.ToTable("GheNgoi_LichBay");

            entity.HasOne(d => d.IdGheNgoiNavigation).WithMany(p => p.GheNgoiLichBays)
                .HasForeignKey(d => d.IdGheNgoi)
                .HasConstraintName("FK_GheNgoi_LichBay_GheNgoi");

            entity.HasOne(d => d.IdLichBayNavigation).WithMany(p => p.GheNgoiLichBays)
                .HasForeignKey(d => d.IdLichBay)
                .HasConstraintName("FK_GheNgoi_LichBay_LichBay");
        });

        modelBuilder.Entity<GoogleSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GoogleSe__3214EC074754D219");

            entity.ToTable("GoogleSetting");

            entity.Property(e => e.ClientId).HasColumnName("Client_id");
            entity.Property(e => e.ClientSecret).HasColumnName("Client_secret");
        });

        modelBuilder.Entity<HangBay>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HangBay__3214EC076D532997");

            entity.ToTable("HangBay");

            entity.Property(e => e.TenHang).HasMaxLength(100);
        });

        modelBuilder.Entity<HashKey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Hash_Key__3214EC07BC4EC7E6");

            entity.ToTable("Hash_Key");

            entity.Property(e => e.PrivateKey).HasColumnName("Private_Key");
            entity.Property(e => e.PublicKey).HasColumnName("Public_Key");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KhachHan__3214EC07E7B08776");

            entity.ToTable("KhachHang");

            entity.HasIndex(e => e.IdTaiKhoan, "UQ_KhachHang_IdTaiKhoan").IsUnique();

            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.TenKh)
                .HasMaxLength(255)
                .HasColumnName("TenKH");

            entity.HasOne(d => d.IdPhuongNavigation).WithMany(p => p.KhachHangs)
                .HasForeignKey(d => d.IdPhuong)
                .HasConstraintName("FK_KhachHang_Phuong");

            entity.HasOne(d => d.IdQuanNavigation).WithMany(p => p.KhachHangs)
                .HasForeignKey(d => d.IdQuan)
                .HasConstraintName("FK_KhachHang_Quan");

            entity.HasOne(d => d.IdQuocTichNavigation).WithMany(p => p.KhachHangs)
                .HasForeignKey(d => d.IdQuocTich)
                .HasConstraintName("FK_KhachHang_QuocTich");

            entity.HasOne(d => d.IdTaiKhoanNavigation).WithOne(p => p.KhachHang)
                .HasForeignKey<KhachHang>(d => d.IdTaiKhoan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KhachHang_TaiKhoan");

            entity.HasOne(d => d.IdTinhNavigation).WithMany(p => p.KhachHangs)
                .HasForeignKey(d => d.IdTinh)
                .HasConstraintName("FK_KhachHang_Tinh");
        });

        modelBuilder.Entity<KhachHangCccd>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KhachHan__3214EC075238890C");

            entity.ToTable("KhachHang_CCCD");

            entity.HasIndex(e => e.IdKhachHang, "UQ_KhachHang_IdKhachHang").IsUnique();

            entity.Property(e => e.NgayCap).HasColumnType("datetime");
            entity.Property(e => e.SoCccd)
                .HasMaxLength(30)
                .HasColumnName("SoCCCD");
            entity.Property(e => e.TenTrenCccd)
                .HasMaxLength(255)
                .HasColumnName("TenTrenCCCD");

            entity.HasOne(d => d.IdKhachHangNavigation).WithOne(p => p.KhachHangCccd)
                .HasForeignKey<KhachHangCccd>(d => d.IdKhachHang)
                .HasConstraintName("FK_KhachHang_KhachHangCCCD");
        });

        modelBuilder.Entity<KhachHangPassport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KhachHan__3214EC07DE1FBD1C");

            entity.ToTable("KhachHang_Passport");

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.LoaiPassport).HasMaxLength(50);
            entity.Property(e => e.NoiCap).HasMaxLength(100);
            entity.Property(e => e.QuocTich).HasMaxLength(50);
            entity.Property(e => e.SoPassport).HasMaxLength(20);
            entity.Property(e => e.TenTrenPassport).HasMaxLength(100);

            entity.HasOne(d => d.IdKhachHangNavigation).WithMany(p => p.KhachHangPassports)
                .HasForeignKey(d => d.IdKhachHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KhachHang_Passport_KhachHang");
        });

        modelBuilder.Entity<LichBay>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LichBay__3214EC07FB17D352");

            entity.ToTable("LichBay");

            entity.Property(e => e.ThoiGianBay).HasComputedColumnSql("(datediff(minute,[ThoiGianOSanBayDi_UTC],[ThoiGianOSanBayDen_UTC]))", false);
            entity.Property(e => e.ThoiGianOsanBayDenUtc)
                .HasColumnType("datetime")
                .HasColumnName("ThoiGianOSanBayDen_UTC");
            entity.Property(e => e.ThoiGianOsanBayDiUtc)
                .HasColumnType("datetime")
                .HasColumnName("ThoiGianOSanBayDi_UTC");

            entity.HasOne(d => d.IdTuyenBayNavigation).WithMany(p => p.LichBays)
                .HasForeignKey(d => d.IdTuyenBay)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LichBay_TuyenBay");
        });

        modelBuilder.Entity<LichSuThanhToan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LichSuTh__3214EC07D6DBDAE7");

            entity.ToTable("LichSuThanhToan");

            entity.HasIndex(e => e.MaThanhToan, "UQ__LichSuTh__D4B258453077B2D3").IsUnique();

            entity.Property(e => e.LoaiDichVu).HasMaxLength(50);
            entity.Property(e => e.MaThanhToan).HasMaxLength(50);
            entity.Property(e => e.NgayThanhToan).HasColumnType("datetime");
            entity.Property(e => e.SoTien).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdPhuongThucThanhToanNavigation).WithMany(p => p.LichSuThanhToans)
                .HasForeignKey(d => d.IdPhuongThucThanhToan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichSuTha__IdPhu__7B5B524B");

            entity.HasOne(d => d.IdTaiKhoanNavigation).WithMany(p => p.LichSuThanhToans)
                .HasForeignKey(d => d.IdTaiKhoan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichSuTha__IdTai__797309D9");

            entity.HasOne(d => d.TrangThai).WithMany(p => p.LichSuThanhToans)
                .HasForeignKey(d => d.TrangThaiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichSuTha__Trang__7A672E12");
        });

        modelBuilder.Entity<LoaiHoatDong>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LoaiHoat__3214EC0795FC0ED7");

            entity.ToTable("LoaiHoatDong");

            entity.Property(e => e.TenHoatDong).HasMaxLength(100);
        });

        modelBuilder.Entity<LoaiPhieuGiamGium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LoaiPhie__3214EC076F50BB64");

            entity.Property(e => e.LoaiGiam).HasMaxLength(255);
        });

        modelBuilder.Entity<LoaiPhuongThucThanhToan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LoaiPhuo__3214EC072B50F3CB");

            entity.ToTable("LoaiPhuongThucThanhToan");

            entity.Property(e => e.TenPhuongThuc).HasMaxLength(100);
        });

        modelBuilder.Entity<LoaiTaiKhoan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LoaiTaiK__3214EC072ECE7F1B");

            entity.ToTable("LoaiTaiKhoan");

            entity.Property(e => e.TenLoai).HasMaxLength(50);
        });

        modelBuilder.Entity<LoaiVe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LoaiVe__3214EC072C6933BE");

            entity.ToTable("LoaiVe");

            entity.Property(e => e.TenLoaiVe).HasMaxLength(50);
        });

        modelBuilder.Entity<MaLoiVnPay>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PK__MaLoiVnP__A25C5AA6CA684D0F");

            entity.ToTable("MaLoiVnPay");

            entity.Property(e => e.Code).ValueGeneratedNever();
            entity.Property(e => e.MoTa).HasMaxLength(255);
        });

        modelBuilder.Entity<OauthState>(entity =>
        {
            entity.HasKey(e => e.State).HasName("PK__OAuthSta__BA803DACBCF39392");

            entity.ToTable("OAuthState");

            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.CodeVerifier).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Paypalsetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PAYPALSE__3214EC07DDE04521");

            entity.ToTable("PAYPALSETTING");

            entity.Property(e => e.BaseUrl).HasMaxLength(100);
            entity.Property(e => e.ClientId).HasMaxLength(255);
            entity.Property(e => e.CreateAt).HasColumnType("datetime");
            entity.Property(e => e.Enviroment).HasMaxLength(100);
        });

        modelBuilder.Entity<PhanQuyenChucNang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PhanQuye__3214EC0749A4E8A8");

            entity.ToTable("PhanQuyen_ChucNang");

            entity.Property(e => e.QuyenSua)
                .HasDefaultValue(false)
                .HasColumnName("Quyen_Sua");
            entity.Property(e => e.QuyenThem)
                .HasDefaultValue(false)
                .HasColumnName("Quyen_Them");
            entity.Property(e => e.QuyenXem)
                .HasDefaultValue(false)
                .HasColumnName("Quyen_Xem");
            entity.Property(e => e.QuyenXoa)
                .HasDefaultValue(false)
                .HasColumnName("Quyen_Xoa");

            entity.HasOne(d => d.IdChucNangNavigation).WithMany(p => p.PhanQuyenChucNangs)
                .HasForeignKey(d => d.IdChucNang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhanQuyen__IdChu__398D8EEE");

            entity.HasOne(d => d.IdTaiKhoanNavigation).WithMany(p => p.PhanQuyenChucNangs)
                .HasForeignKey(d => d.IdTaiKhoan)
                .HasConstraintName("FK_PhanQuyen_ChucNang_TaiKhoan");
        });

        modelBuilder.Entity<PhieuGiamGium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PhieuGia__3214EC0764DB2CD6");

            entity.HasIndex(e => e.MaGiamGia, "UQ__PhieuGia__EF9458E517CA7288").IsUnique();

            entity.Property(e => e.GiaTriGiam).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MaGiamGia)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.NoiDung).HasMaxLength(100);

            entity.HasOne(d => d.IdDieuKienGiamNavigation).WithMany(p => p.PhieuGiamGia)
                .HasForeignKey(d => d.IdDieuKienGiam)
                .HasConstraintName("FK_PhieuGiamGia_DieuKien");

            entity.HasOne(d => d.IdLoaiGiamGiaNavigation).WithMany(p => p.PhieuGiamGia)
                .HasForeignKey(d => d.IdLoaiGiamGia)
                .HasConstraintName("FK_PhieuGiamGia_LoaiPhieuGiam");
        });

        modelBuilder.Entity<Phong>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Phong__3214EC0754BD9CE2");

            entity.ToTable("Phong");

            entity.Property(e => e.Gia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TenPhong).HasMaxLength(255);
        });

        modelBuilder.Entity<Phuong>(entity =>
        {
            entity.HasKey(e => e.IdPhuong).HasName("PK__Phuong__8861D42246CB5EE7");

            entity.ToTable("Phuong");

            entity.HasIndex(e => new { e.TenPhuong, e.IdQuan }, "UQ_Phuong_Ten").IsUnique();

            entity.Property(e => e.TenPhuong).HasMaxLength(100);

            entity.HasOne(d => d.IdQuanNavigation).WithMany(p => p.Phuongs)
                .HasForeignKey(d => d.IdQuan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Phuong_Quan");
        });

        modelBuilder.Entity<Quan>(entity =>
        {
            entity.HasKey(e => e.IdQuan).HasName("PK__Quan__9005E789EBE76FC9");

            entity.ToTable("Quan");

            entity.HasIndex(e => new { e.TenQuan, e.IdTinh }, "UQ_Quan_Ten").IsUnique();

            entity.Property(e => e.TenQuan).HasMaxLength(100);

            entity.HasOne(d => d.IdTinhNavigation).WithMany(p => p.Quans)
                .HasForeignKey(d => d.IdTinh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Quan_Tinh");
        });

        modelBuilder.Entity<QuocTich>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__QuocTich__3214EC07ECCE980B");

            entity.ToTable("QuocTich");

            entity.Property(e => e.QuocTich1)
                .HasMaxLength(100)
                .HasColumnName("QuocTich");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RefreshT__3214EC0760312425");

            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.RefreshToken1)
                .HasMaxLength(200)
                .HasColumnName("RefreshToken");

            entity.HasOne(d => d.IdTaiKhoanNavigation).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.IdTaiKhoan)
                .HasConstraintName("FK_RefreshTokens_TaiKhoan");
        });

        modelBuilder.Entity<SanBay>(entity =>
        {
            entity.HasKey(e => e.MaIata).HasName("PK__SanBay__4A351DED081F3D91");

            entity.ToTable("SanBay");

            entity.Property(e => e.MaIata)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaIATA");
            entity.Property(e => e.IanaTimeZoneId).HasMaxLength(100);
            entity.Property(e => e.QuocGia).HasMaxLength(100);
            entity.Property(e => e.Ten).HasMaxLength(100);
            entity.Property(e => e.ThanhPho).HasMaxLength(100);
            entity.Property(e => e.TimeZoneId).HasMaxLength(50);
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaiKhoan__3214EC07486C2527");

            entity.ToTable("TaiKhoan");

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Otp)
                .HasMaxLength(10)
                .HasColumnName("OTP");
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
            entity.Property(e => e.SoLanNhapSaiMatKhau).HasDefaultValue(0);
            entity.Property(e => e.ThoiGianKhoaMatKhau).HasColumnType("datetime");

            entity.HasOne(d => d.LoaiTaiKhoan).WithMany(p => p.TaiKhoans)
                .HasForeignKey(d => d.LoaiTaiKhoanId)
                .HasConstraintName("FK__TaiKhoan__LoaiTa__3F466844");
        });

        modelBuilder.Entity<ThanhToanCho>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ThanhToa__3214EC07D4559004");

            entity.ToTable("ThanhToanCho");

            entity.Property(e => e.IdChiTietPhieuGiamGia).HasColumnName("idChiTietPhieuGiamGia");
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<ThanhToanVnpay>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ThanhToa__3214EC07CA0183DC");

            entity.ToTable("ThanhToanVNPAY");

            entity.Property(e => e.BankCode).HasMaxLength(20);
            entity.Property(e => e.BankTranNo).HasMaxLength(50);
            entity.Property(e => e.CardType).HasMaxLength(20);
            entity.Property(e => e.PayDate).HasColumnType("datetime");
            entity.Property(e => e.SecureHash).HasMaxLength(256);
            entity.Property(e => e.TransactionNo).HasMaxLength(50);

            entity.HasOne(d => d.ResponseCodeNavigation).WithMany(p => p.ThanhToanVnpays)
                .HasForeignKey(d => d.ResponseCode)
                .HasConstraintName("FK__ThanhToan__Respo__02084FDA");
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ThongBao__3214EC07CB7F0937");

            entity.ToTable("ThongBao");

            entity.Property(e => e.NgayTao).HasColumnType("datetime");
            entity.Property(e => e.NoiDung).HasMaxLength(255);
            entity.Property(e => e.TieuDe).HasMaxLength(255);
        });

        modelBuilder.Entity<TienNghi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TienNghi__3214EC07FBD5FA1C");

            entity.ToTable("TienNghi");

            entity.HasIndex(e => e.MaTienIch, "UQ__TienNghi__4697D8EBC965CB5D").IsUnique();

            entity.Property(e => e.MaTienIch)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenTienIch).HasMaxLength(50);
        });

        modelBuilder.Entity<TienNghiChuyenMayBay>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TienNghi__3214EC07549F11DD");

            entity.ToTable("TienNghiChuyenMayBay");

            entity.HasOne(d => d.IdChuyenBayNavigation).WithMany(p => p.TienNghiChuyenMayBays)
                .HasForeignKey(d => d.IdChuyenBay)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TienNghiC__IdChu__619B8048");

            entity.HasOne(d => d.IdTienNghiNavigation).WithMany(p => p.TienNghiChuyenMayBays)
                .HasForeignKey(d => d.IdTienNghi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TienNghiC__IdTie__628FA481");
        });

        modelBuilder.Entity<Tinh>(entity =>
        {
            entity.HasKey(e => e.IdTinh).HasName("PK__Tinh__9E3A39ECD0994B56");

            entity.ToTable("Tinh");

            entity.HasIndex(e => e.TenTinh, "UQ_Tinh_Ten").IsUnique();

            entity.Property(e => e.TenTinh).HasMaxLength(100);
        });

        modelBuilder.Entity<TrangThaiGheNgoi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TrangTha__3214EC078327F236");

            entity.ToTable("TrangThaiGheNgoi");

            entity.Property(e => e.TenTrangThai).HasMaxLength(255);
        });

        modelBuilder.Entity<TrangThaiThanhToan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TrangTha__3214EC074230134D");

            entity.ToTable("TrangThaiThanhToan");

            entity.Property(e => e.TenTrangThai).HasMaxLength(50);
        });

        modelBuilder.Entity<VeMayBay>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VeMayBay__3214EC07DD3038CC");

            entity.ToTable("VeMayBay");

            entity.HasIndex(e => e.MaVe, "UQ__VeMayBay__2725100E0E6BCCD8").IsUnique();

            entity.Property(e => e.Gia).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MaVe).HasMaxLength(50);

            entity.HasOne(d => d.IdChuyenBayNavigation).WithMany(p => p.VeMayBays)
                .HasForeignKey(d => d.IdChuyenBay)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VeMayBay__IdChuy__5AEE82B9");

            entity.HasOne(d => d.LoaiVe).WithMany(p => p.VeMayBays)
                .HasForeignKey(d => d.LoaiVeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VeMayBay__LoaiVe__5BE2A6F2");
        });

        modelBuilder.Entity<Vnpaysetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VNPAYSET__3214EC0797532C28");

            entity.ToTable("VNPAYSETTING");

            entity.Property(e => e.BaseUrl).HasMaxLength(100);
            entity.Property(e => e.CallBackUrl).HasMaxLength(100);
            entity.Property(e => e.CreateAt).HasColumnType("datetime");
            entity.Property(e => e.Enviroment).HasMaxLength(100);
            entity.Property(e => e.TmnCode).HasMaxLength(255);
        });

        modelBuilder.Entity<ZaloSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ZaloSett__3214EC07CCF64461");

            entity.ToTable("ZaloSetting");

            entity.Property(e => e.AppId).HasMaxLength(255);
            entity.Property(e => e.AppSceret).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
