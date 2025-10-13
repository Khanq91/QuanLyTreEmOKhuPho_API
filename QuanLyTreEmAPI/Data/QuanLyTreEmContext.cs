using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyTreEmAPI.Models;


namespace QuanLyTreEmAPI.Data
{
    public class QuanLyTreEmContext : DbContext
    {
        public QuanLyTreEmContext(DbContextOptions<QuanLyTreEmContext> options) : base(options) { }
        public DbSet<KhuPho> KhuPhos { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<TinhNguyenVien> TinhNguyenViens { get; set; }
        public DbSet<TreEm> TreEms { get; set; }
        public DbSet<ThongTinPhuHuynh> ThongTinPhuHuynhs { get; set; }
        public DbSet<SuKien> SuKiens { get; set; }
        public DbSet<HoTroPhucLoi> HoTroPhucLois { get; set; }
        public DbSet<PhieuHocTap> PhieuHocTaps { get; set; }
        public DbSet<HoanCanh> HoanCanhs { get; set; }
        public DbSet<VanDongTreEm> VanDongTreEms { get; set; }
        public DbSet<ManhThuongQuan> ManhThuongQuans { get; set; }
        public DbSet<UngHo> UngHos { get; set; }
        public DbSet<DangKySuKien> DangKySuKiens { get; set; }
        public DbSet<PhanCongTinhNguyenVien> PhanCongTinhNguyenViens { get; set; }
        public DbSet<ThongBao> ThongBaos { get; set; }
        public DbSet<TruongHoc> TruongHocs { get; set; }
        public DbSet<LopHoc> LopHocs { get; set; }
        public DbSet<ChiPhiSuKien> ChiPhiSuKiens { get; set; }
        public DbSet<TietMucSuKien> TietMucSuKiens { get; set; }
        public DbSet<PhieuMinhChung> PhieuMinhChungs { get; set; }
        public DbSet<LichTrong> LichTrongs { get; set; }
        public DbSet<ChiTietLichTrong> ChiTietLichTrongs { get; set; }
        public DbSet<TreEmPhuHuynh> TreEmPhuHuynhs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TreEmPhuHuynh>()
                .HasKey(tp => new { tp.TreEmId, tp.PhuHuynhId });

            modelBuilder.Entity<TreEmHoanCanh>()
                .HasKey(th => new { th.TreEmId, th.HoanCanhId });
        }
    }
}
