using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QuanLyTreEmAPI.Models;

public partial class QuanLyTreEmContext : DbContext
{
    public QuanLyTreEmContext()
    {
    }

    public QuanLyTreEmContext(DbContextOptions<QuanLyTreEmContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChiPhiSuKien> ChiPhiSuKiens { get; set; }

    public virtual DbSet<ChiTietChiPhiSuKien> ChiTietChiPhiSuKiens { get; set; }

    public virtual DbSet<ChiTietLichTrong> ChiTietLichTrongs { get; set; }

    public virtual DbSet<DangKySuKien> DangKySuKiens { get; set; }

    public virtual DbSet<HoTroPhucLoi> HoTroPhucLois { get; set; }

    public virtual DbSet<HoanCanh> HoanCanhs { get; set; }

    public virtual DbSet<KhuPho> KhuPhos { get; set; }

    public virtual DbSet<LichTrong> LichTrongs { get; set; }

    public virtual DbSet<LopHoc> LopHocs { get; set; }

    public virtual DbSet<ManhThuongQuan> ManhThuongQuans { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<PhanCongTinhNguyenVien> PhanCongTinhNguyenViens { get; set; }

    public virtual DbSet<PhieuHocTap> PhieuHocTaps { get; set; }

    public virtual DbSet<PhieuMinhChung> PhieuMinhChungs { get; set; }

    public virtual DbSet<SuKien> SuKiens { get; set; }

    public virtual DbSet<ThoiGianChiTietSuKien> ThoiGianChiTietSuKiens { get; set; }

    public virtual DbSet<ThongBao> ThongBaos { get; set; }

    public virtual DbSet<ThongTinPhuHuynh> ThongTinPhuHuynhs { get; set; }

    public virtual DbSet<TietMucSuKien> TietMucSuKiens { get; set; }

    public virtual DbSet<TinhNguyenVien> TinhNguyenViens { get; set; }

    public virtual DbSet<TreEm> TreEms { get; set; }

    public virtual DbSet<TreEmHoanCanh> TreEmHoanCanhs { get; set; }

    public virtual DbSet<TreEmPhuHuynh> TreEmPhuHuynhs { get; set; }

    public virtual DbSet<TruongHoc> TruongHocs { get; set; }

    public virtual DbSet<UngHo> UngHos { get; set; }

    public virtual DbSet<VanDongTreEm> VanDongTreEms { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost,1433;Database=QuanLyTreEm;User Id=sa;Password=NewP@ssw0rd!;TrustServerCertificate=True;Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChiPhiSuKien>(entity =>
        {
            entity.HasKey(e => e.ChiPhiId).HasName("PK__ChiPhiSu__286EFE41D06CFD90");

            entity.ToTable("ChiPhiSuKien");

            entity.Property(e => e.ChiPhiId).HasColumnName("ChiPhiID");
            entity.Property(e => e.GhiChu).HasMaxLength(300);
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SuKienId).HasColumnName("SuKienID");
            entity.Property(e => e.TenKhoanChi).HasMaxLength(200);

            entity.HasOne(d => d.SuKien).WithMany(p => p.ChiPhiSuKiens)
                .HasForeignKey(d => d.SuKienId)
                .HasConstraintName("FK__ChiPhiSuK__SuKie__33D4B598");
        });

        modelBuilder.Entity<ChiTietChiPhiSuKien>(entity =>
        {
            entity.HasKey(e => e.ChiTietChiPhi).HasName("PK__ChiTietC__45AF0864EFC475A5");

            entity.ToTable("ChiTietChiPhiSuKien");

            entity.Property(e => e.ChiPhiId).HasColumnName("ChiPhiID");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NguoiDaiDien).HasMaxLength(200);
            entity.Property(e => e.TenPhanQua).HasMaxLength(20);

            entity.HasOne(d => d.ChiPhi).WithMany(p => p.ChiTietChiPhiSuKiens)
                .HasForeignKey(d => d.ChiPhiId)
                .HasConstraintName("FK__ChiTietCh__ChiPh__36B12243");
        });

        modelBuilder.Entity<ChiTietLichTrong>(entity =>
        {
            entity.HasKey(e => e.ChiTietLichTrongId).HasName("PK__ChiTietL__D12074778DF9DFEF");

            entity.ToTable("ChiTietLichTrong");

            entity.Property(e => e.ChiTietLichTrongId).HasColumnName("ChiTietLichTrongID");
            entity.Property(e => e.Buoi).HasMaxLength(15);
            entity.Property(e => e.LichTrongId).HasColumnName("LichTrongID");
            entity.Property(e => e.Thu).HasMaxLength(15);

            entity.HasOne(d => d.LichTrong).WithMany(p => p.ChiTietLichTrongs)
                .HasForeignKey(d => d.LichTrongId)
                .HasConstraintName("FK__ChiTietLi__LichT__22AA2996");
        });

        modelBuilder.Entity<DangKySuKien>(entity =>
        {
            entity.HasKey(e => e.DangKySuKienId).HasName("PK__DangKySu__57C14A7556D5E771");

            entity.ToTable("DangKySuKien");

            entity.Property(e => e.DangKySuKienId).HasColumnName("DangKySuKienID");
            entity.Property(e => e.SuKienId).HasColumnName("SuKienID");
            entity.Property(e => e.TrangThai).HasMaxLength(50);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.SuKien).WithMany(p => p.DangKySuKiens)
                .HasForeignKey(d => d.SuKienId)
                .HasConstraintName("FK__DangKySuK__SuKie__398D8EEE");

            entity.HasOne(d => d.User).WithMany(p => p.DangKySuKiens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__DangKySuK__UserI__3A81B327");
        });

        modelBuilder.Entity<HoTroPhucLoi>(entity =>
        {
            entity.HasKey(e => e.HoTroId).HasName("PK__HoTroPhu__530870AE2353BE29");

            entity.ToTable("HoTroPhucLoi");

            entity.Property(e => e.HoTroId).HasColumnName("HoTroID");
            entity.Property(e => e.LoaiHoTro).HasMaxLength(100);
            entity.Property(e => e.NguoiChiuTrachNhiemHoTro).HasMaxLength(100);
            entity.Property(e => e.TreEmId).HasColumnName("TreEmID");

            entity.HasOne(d => d.TreEm).WithMany(p => p.HoTroPhucLois)
                .HasForeignKey(d => d.TreEmId)
                .HasConstraintName("FK__HoTroPhuc__TreEm__656C112C");
        });

        modelBuilder.Entity<HoanCanh>(entity =>
        {
            entity.HasKey(e => e.HoanCanhId).HasName("PK__HoanCanh__316D0C4AC51B1535");

            entity.ToTable("HoanCanh");

            entity.Property(e => e.HoanCanhId).HasColumnName("HoanCanhID");
            entity.Property(e => e.LoaiHoanCanh).HasMaxLength(100);
        });

        modelBuilder.Entity<KhuPho>(entity =>
        {
            entity.HasKey(e => e.KhuPhoId).HasName("PK__KhuPho__E3AD7481E69F6650");

            entity.ToTable("KhuPho");

            entity.Property(e => e.KhuPhoId).HasColumnName("KhuPhoID");
            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.QuanHuyen).HasMaxLength(100);
            entity.Property(e => e.TenKhuPho).HasMaxLength(100);
            entity.Property(e => e.ThanhPho).HasMaxLength(100);
        });

        modelBuilder.Entity<LichTrong>(entity =>
        {
            entity.HasKey(e => e.LichTrongId).HasName("PK__LichTron__193AF7994610DA5B");

            entity.ToTable("LichTrong");

            entity.HasIndex(e => e.TinhNguyenVienId, "UQ__LichTron__3A613E9AEB037D23").IsUnique();

            entity.Property(e => e.LichTrongId).HasColumnName("LichTrongID");
            entity.Property(e => e.TinhNguyenVienId).HasColumnName("TinhNguyenVienID");

            entity.HasOne(d => d.TinhNguyenVien).WithOne(p => p.LichTrong)
                .HasForeignKey<LichTrong>(d => d.TinhNguyenVienId)
                .HasConstraintName("FK__LichTrong__TinhN__1FCDBCEB");
        });

        modelBuilder.Entity<LopHoc>(entity =>
        {
            entity.HasKey(e => e.LopId).HasName("PK__LopHoc__40585DCB3C379A7C");

            entity.ToTable("LopHoc");

            entity.Property(e => e.LopId).HasColumnName("LopID");
            entity.Property(e => e.TenLop).HasMaxLength(50);
            entity.Property(e => e.TruongId).HasColumnName("TruongID");

            entity.HasOne(d => d.Truong).WithMany(p => p.LopHocs)
                .HasForeignKey(d => d.TruongId)
                .HasConstraintName("FK__LopHoc__TruongID__5165187F");
        });

        modelBuilder.Entity<ManhThuongQuan>(entity =>
        {
            entity.HasKey(e => e.ManhThuongQuanId).HasName("PK__ManhThuo__62BB49D1C32E7B0B");

            entity.ToTable("ManhThuongQuan");

            entity.Property(e => e.ManhThuongQuanId).HasColumnName("ManhThuongQuanID");
            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.GhiChu).HasMaxLength(300);
            entity.Property(e => e.Loai).HasMaxLength(50);
            entity.Property(e => e.Sdt)
                .HasMaxLength(15)
                .HasColumnName("SDT");
            entity.Property(e => e.Ten).HasMaxLength(100);
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__NguoiDun__1788CCACD4696275");

            entity.ToTable("NguoiDung");

            entity.HasIndex(e => e.Email, "UQ__NguoiDun__A9D1053405DBD2B6").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhau).HasMaxLength(100);
            entity.Property(e => e.VaiTro).HasMaxLength(50);
        });

        modelBuilder.Entity<PhanCongTinhNguyenVien>(entity =>
        {
            entity.HasKey(e => e.PhanCongId).HasName("PK__PhanCong__7EF840DD4376F785");

            entity.ToTable("PhanCongTinhNguyenVien");

            entity.Property(e => e.PhanCongId).HasColumnName("PhanCongID");
            entity.Property(e => e.CongViec).HasMaxLength(200);
            entity.Property(e => e.GhiChu).HasMaxLength(300);
            entity.Property(e => e.NgayPhanCong).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.SuKienId).HasColumnName("SuKienID");
            entity.Property(e => e.TinhNguyenVienId).HasColumnName("TinhNguyenVienID");

            entity.HasOne(d => d.SuKien).WithMany(p => p.PhanCongTinhNguyenViens)
                .HasForeignKey(d => d.SuKienId)
                .HasConstraintName("FK__PhanCongT__SuKie__2C3393D0");

            entity.HasOne(d => d.TinhNguyenVien).WithMany(p => p.PhanCongTinhNguyenViens)
                .HasForeignKey(d => d.TinhNguyenVienId)
                .HasConstraintName("FK__PhanCongT__TinhN__2D27B809");
        });

        modelBuilder.Entity<PhieuHocTap>(entity =>
        {
            entity.HasKey(e => e.PhieuHocTapId).HasName("PK__PhieuHoc__CF3109D68FC2C6BD");

            entity.ToTable("PhieuHocTap");

            entity.Property(e => e.PhieuHocTapId).HasColumnName("PhieuHocTapID");
            entity.Property(e => e.GhiChu).HasMaxLength(200);
            entity.Property(e => e.HanhKiem).HasMaxLength(25);
            entity.Property(e => e.LopId).HasColumnName("LopID");
            entity.Property(e => e.TreEmId).HasColumnName("TreEmID");
            entity.Property(e => e.TruongId).HasColumnName("TruongID");
            entity.Property(e => e.XepLoai).HasMaxLength(50);

            entity.HasOne(d => d.Lop).WithMany(p => p.PhieuHocTaps)
                .HasForeignKey(d => d.LopId)
                .HasConstraintName("FK__PhieuHocT__LopID__5629CD9C");

            entity.HasOne(d => d.TreEm).WithMany(p => p.PhieuHocTaps)
                .HasForeignKey(d => d.TreEmId)
                .HasConstraintName("FK__PhieuHocT__TreEm__5535A963");

            entity.HasOne(d => d.Truong).WithMany(p => p.PhieuHocTaps)
                .HasForeignKey(d => d.TruongId)
                .HasConstraintName("FK__PhieuHocT__Truon__5441852A");
        });

        modelBuilder.Entity<PhieuMinhChung>(entity =>
        {
            entity.HasKey(e => e.MinhChungId).HasName("PK__PhieuMin__A56F07AB68AF4F70");

            entity.ToTable("PhieuMinhChung");

            entity.Property(e => e.MinhChungId).HasColumnName("MinhChungID");
            entity.Property(e => e.FilePath).HasMaxLength(300);
            entity.Property(e => e.HoTroId).HasColumnName("HoTroID");
            entity.Property(e => e.LoaiMinhChung).HasMaxLength(100);

            entity.HasOne(d => d.HoTro).WithMany(p => p.PhieuMinhChungs)
                .HasForeignKey(d => d.HoTroId)
                .HasConstraintName("FK__PhieuMinh__HoTro__68487DD7");
        });

        modelBuilder.Entity<SuKien>(entity =>
        {
            entity.HasKey(e => e.SuKienId).HasName("PK__SuKien__3B964778629F39E1");

            entity.ToTable("SuKien");

            entity.Property(e => e.SuKienId).HasColumnName("SuKienID");
            entity.Property(e => e.DiaDiem).HasMaxLength(200);
            entity.Property(e => e.KhuPhoId).HasColumnName("KhuPhoID");
            entity.Property(e => e.NguoiChiuTrachNhiem).HasMaxLength(200);
            entity.Property(e => e.TenSuKien).HasMaxLength(200);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.KhuPho).WithMany(p => p.SuKiens)
                .HasForeignKey(d => d.KhuPhoId)
                .HasConstraintName("FK__SuKien__KhuPhoID__267ABA7A");

            entity.HasOne(d => d.User).WithMany(p => p.SuKiens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__SuKien__UserID__25869641");
        });

        modelBuilder.Entity<ThoiGianChiTietSuKien>(entity =>
        {
            entity.HasKey(e => e.ThoiGianChiTietSuKienId).HasName("PK__ThoiGian__8F48913D6C084DE3");

            entity.ToTable("ThoiGianChiTietSuKien");

            entity.Property(e => e.ThoiGianChiTietSuKienId).HasColumnName("ThoiGianChiTietSuKienID");
            entity.Property(e => e.SuKienId).HasColumnName("SuKienID");
            entity.Property(e => e.ThoiGianBatDau).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianKetThuc).HasColumnType("datetime");

            entity.HasOne(d => d.SuKien).WithMany(p => p.ThoiGianChiTietSuKiens)
                .HasForeignKey(d => d.SuKienId)
                .HasConstraintName("FK__ThoiGianC__SuKie__29572725");
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.HasKey(e => e.ThongBaoId).HasName("PK__ThongBao__6E51A53BF50B4D0E");

            entity.ToTable("ThongBao");

            entity.Property(e => e.ThongBaoId).HasColumnName("ThongBaoID");
            entity.Property(e => e.NgayThongBao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.SuKienId).HasColumnName("SuKienID");

            entity.HasOne(d => d.SuKien).WithMany(p => p.ThongBaos)
                .HasForeignKey(d => d.SuKienId)
                .HasConstraintName("FK__ThongBao__SuKien__3D5E1FD2");

            entity.HasMany(d => d.Users).WithMany(p => p.ThongBaos)
                .UsingEntity<Dictionary<string, object>>(
                    "ThongBaoNguoiDung",
                    r => r.HasOne<NguoiDung>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ThongBao___UserI__4222D4EF"),
                    l => l.HasOne<ThongBao>().WithMany()
                        .HasForeignKey("ThongBaoId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ThongBao___Thong__412EB0B6"),
                    j =>
                    {
                        j.HasKey("ThongBaoId", "UserId").HasName("PK__ThongBao__BF2929F176CDF00D");
                        j.ToTable("ThongBao_NguoiDung");
                        j.IndexerProperty<int>("ThongBaoId").HasColumnName("ThongBaoID");
                        j.IndexerProperty<int>("UserId").HasColumnName("UserID");
                    });
        });

        modelBuilder.Entity<ThongTinPhuHuynh>(entity =>
        {
            entity.HasKey(e => e.PhuHuynhId).HasName("PK__ThongTin__D0ADD0903E507A15");

            entity.ToTable("ThongTinPhuHuynh");

            entity.Property(e => e.PhuHuynhId).HasColumnName("PhuHuynhID");
            entity.Property(e => e.DanToc).HasMaxLength(20);
            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.NgheNghiep).HasMaxLength(100);
            entity.Property(e => e.QuocTich).HasMaxLength(30);
            entity.Property(e => e.Sdt)
                .HasMaxLength(15)
                .HasColumnName("SDT");
            entity.Property(e => e.TonGiao).HasMaxLength(20);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.ThongTinPhuHuynhs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__ThongTinP__UserI__44FF419A");
        });

        modelBuilder.Entity<TietMucSuKien>(entity =>
        {
            entity.HasKey(e => e.TietMucId).HasName("PK__TietMucS__2D7B2C9E13C646D4");

            entity.ToTable("TietMucSuKien");

            entity.Property(e => e.TietMucId).HasColumnName("TietMucID");
            entity.Property(e => e.ChiPhiTietMuc).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NguoiThucHien).HasMaxLength(100);
            entity.Property(e => e.TenTietMuc).HasMaxLength(100);
            entity.Property(e => e.ThoiGianChiTietSuKienId).HasColumnName("ThoiGianChiTietSuKienID");

            entity.HasOne(d => d.ThoiGianChiTietSuKien).WithMany(p => p.TietMucSuKiens)
                .HasForeignKey(d => d.ThoiGianChiTietSuKienId)
                .HasConstraintName("FK__TietMucSu__ThoiG__30F848ED");
        });

        modelBuilder.Entity<TinhNguyenVien>(entity =>
        {
            entity.HasKey(e => e.TinhNguyenVienId).HasName("PK__TinhNguy__3A613E9BCA881A38");

            entity.ToTable("TinhNguyenVien");

            entity.HasIndex(e => e.UserId, "UQ__TinhNguy__1788CCADDC6F4A92").IsUnique();

            entity.Property(e => e.TinhNguyenVienId).HasColumnName("TinhNguyenVienID");
            entity.Property(e => e.ChucVu).HasMaxLength(100);
            entity.Property(e => e.KhuPhoId).HasColumnName("KhuPhoID");
            entity.Property(e => e.Sdt)
                .HasMaxLength(15)
                .HasColumnName("SDT");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.KhuPho).WithMany(p => p.TinhNguyenViens)
                .HasForeignKey(d => d.KhuPhoId)
                .HasConstraintName("FK__TinhNguye__KhuPh__1BFD2C07");

            entity.HasOne(d => d.User).WithOne(p => p.TinhNguyenVien)
                .HasForeignKey<TinhNguyenVien>(d => d.UserId)
                .HasConstraintName("FK__TinhNguye__UserI__1B0907CE");
        });

        modelBuilder.Entity<TreEm>(entity =>
        {
            entity.HasKey(e => e.TreEmId).HasName("PK__TreEm__CBDB2964992ACBAE");

            entity.ToTable("TreEm");

            entity.Property(e => e.TreEmId).HasColumnName("TreEmID");
            entity.Property(e => e.DanToc).HasMaxLength(20);
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.KhuPhoId).HasColumnName("KhuPhoID");
            entity.Property(e => e.QuocTich).HasMaxLength(30);
            entity.Property(e => e.TinhTrang).HasMaxLength(100);
            entity.Property(e => e.TonGiao).HasMaxLength(20);
            entity.Property(e => e.TruongId).HasColumnName("TruongID");

            entity.HasOne(d => d.KhuPho).WithMany(p => p.TreEms)
                .HasForeignKey(d => d.KhuPhoId)
                .HasConstraintName("FK__TreEm__KhuPhoID__4AB81AF0");

            entity.HasOne(d => d.Truong).WithMany(p => p.TreEms)
                .HasForeignKey(d => d.TruongId)
                .HasConstraintName("FK__TreEm__TruongID__49C3F6B7");
        });

        modelBuilder.Entity<TreEmHoanCanh>(entity =>
        {
            entity.HasKey(e => new { e.TreEmId, e.HoanCanhId }).HasName("PK__TreEm_Ho__78CDF9A0393E0E89");

            entity.ToTable("TreEm_HoanCanh");

            entity.Property(e => e.TreEmId).HasColumnName("TreEmID");
            entity.Property(e => e.HoanCanhId).HasColumnName("HoanCanhID");

            entity.HasOne(d => d.HoanCanh).WithMany(p => p.TreEmHoanCanhs)
                .HasForeignKey(d => d.HoanCanhId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TreEm_Hoa__HoanC__5BE2A6F2");

            entity.HasOne(d => d.TreEm).WithMany(p => p.TreEmHoanCanhs)
                .HasForeignKey(d => d.TreEmId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TreEm_Hoa__TreEm__5AEE82B9");
        });

        modelBuilder.Entity<TreEmPhuHuynh>(entity =>
        {
            entity.HasKey(e => new { e.TreEmId, e.PhuHuynhId }).HasName("PK__TreEm_Ph__D6D1F46D850EC5C0");

            entity.ToTable("TreEm_PhuHuynh");

            entity.Property(e => e.TreEmId).HasColumnName("TreEmID");
            entity.Property(e => e.PhuHuynhId).HasColumnName("PhuHuynhID");
            entity.Property(e => e.MoiQuanHe).HasMaxLength(50);

            entity.HasOne(d => d.PhuHuynh).WithMany(p => p.TreEmPhuHuynhs)
                .HasForeignKey(d => d.PhuHuynhId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TreEm_Phu__PhuHu__4E88ABD4");

            entity.HasOne(d => d.TreEm).WithMany(p => p.TreEmPhuHuynhs)
                .HasForeignKey(d => d.TreEmId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TreEm_Phu__TreEm__4D94879B");
        });

        modelBuilder.Entity<TruongHoc>(entity =>
        {
            entity.HasKey(e => e.TruongId).HasName("PK__TruongHo__499CE3D51290280F");

            entity.ToTable("TruongHoc");

            entity.Property(e => e.TruongId).HasColumnName("TruongID");
            entity.Property(e => e.CapHoc).HasMaxLength(50);
            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.TenTruong).HasMaxLength(200);
        });

        modelBuilder.Entity<UngHo>(entity =>
        {
            entity.HasKey(e => e.UngHoId).HasName("PK__UngHo__3905AC5F7C4AC287");

            entity.ToTable("UngHo");

            entity.Property(e => e.UngHoId).HasColumnName("UngHoID");
            entity.Property(e => e.GhiChu).HasMaxLength(300);
            entity.Property(e => e.LoaiUngHo).HasMaxLength(100);
            entity.Property(e => e.ManhThuongQuanId).HasColumnName("ManhThuongQuanID");
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.ManhThuongQuan).WithMany(p => p.UngHos)
                .HasForeignKey(d => d.ManhThuongQuanId)
                .HasConstraintName("FK__UngHo__ManhThuon__173876EA");

            entity.HasMany(d => d.HoTros).WithMany(p => p.UngHos)
                .UsingEntity<Dictionary<string, object>>(
                    "UngHoHoTroPhucLoi",
                    r => r.HasOne<HoTroPhucLoi>().WithMany()
                        .HasForeignKey("HoTroId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UngHo_HoT__HoTro__6FE99F9F"),
                    l => l.HasOne<UngHo>().WithMany()
                        .HasForeignKey("UngHoId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UngHo_HoT__UngHo__6EF57B66"),
                    j =>
                    {
                        j.HasKey("UngHoId", "HoTroId").HasName("PK__UngHo_Ho__CC352B559EB899C2");
                        j.ToTable("UngHo_HoTroPhucLoi");
                        j.IndexerProperty<int>("UngHoId").HasColumnName("UngHoID");
                        j.IndexerProperty<int>("HoTroId").HasColumnName("HoTroID");
                    });

            entity.HasMany(d => d.TreEms).WithMany(p => p.UngHos)
                .UsingEntity<Dictionary<string, object>>(
                    "UngHoTreEm",
                    r => r.HasOne<TreEm>().WithMany()
                        .HasForeignKey("TreEmId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UngHo_Tre__TreEm__6C190EBB"),
                    l => l.HasOne<UngHo>().WithMany()
                        .HasForeignKey("UngHoId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UngHo_Tre__UngHo__6B24EA82"),
                    j =>
                    {
                        j.HasKey("UngHoId", "TreEmId").HasName("PK__UngHo_Tr__65B81EC9028B81F4");
                        j.ToTable("UngHo_TreEm");
                        j.IndexerProperty<int>("UngHoId").HasColumnName("UngHoID");
                        j.IndexerProperty<int>("TreEmId").HasColumnName("TreEmID");
                    });
        });

        modelBuilder.Entity<VanDongTreEm>(entity =>
        {
            entity.HasKey(e => e.VanDongId).HasName("PK__VanDongT__561872EAEBD4ABE4");

            entity.ToTable("VanDongTreEm");

            entity.Property(e => e.VanDongId).HasColumnName("VanDongID");
            entity.Property(e => e.HoanCanhId).HasColumnName("HoanCanhID");
            entity.Property(e => e.KetQua).HasMaxLength(200);
            entity.Property(e => e.LyDo).HasMaxLength(200);
            entity.Property(e => e.NgayVanDong).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDungID");
            entity.Property(e => e.SoLan).HasDefaultValue(1);
            entity.Property(e => e.TreEmId).HasColumnName("TreEmID");

            entity.HasOne(d => d.HoanCanh).WithMany(p => p.VanDongTreEms)
                .HasForeignKey(d => d.HoanCanhId)
                .HasConstraintName("FK__VanDongTr__HoanC__5FB337D6");

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.VanDongTreEms)
                .HasForeignKey(d => d.NguoiDungId)
                .HasConstraintName("FK__VanDongTr__Nguoi__60A75C0F");

            entity.HasOne(d => d.TreEm).WithMany(p => p.VanDongTreEms)
                .HasForeignKey(d => d.TreEmId)
                .HasConstraintName("FK__VanDongTr__TreEm__5EBF139D");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
