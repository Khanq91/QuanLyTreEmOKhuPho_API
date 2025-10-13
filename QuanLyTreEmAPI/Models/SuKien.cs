using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class SuKien
{
    public int SuKienId { get; set; }

    public string? TenSuKien { get; set; }

    public string? NguoiChiuTrachNhiem { get; set; }

    public string? MoTa { get; set; }

    public string? DiaDiem { get; set; }

    public DateOnly? NgayBatDau { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    public int? SoLuongTinhNguyenVien { get; set; }

    public int? SoLuongTreEm { get; set; }

    public int? UserId { get; set; }

    public int? KhuPhoId { get; set; }

    public virtual ICollection<ChiPhiSuKien> ChiPhiSuKiens { get; set; } = new List<ChiPhiSuKien>();

    public virtual ICollection<DangKySuKien> DangKySuKiens { get; set; } = new List<DangKySuKien>();

    public virtual KhuPho? KhuPho { get; set; }

    public virtual ICollection<PhanCongTinhNguyenVien> PhanCongTinhNguyenViens { get; set; } = new List<PhanCongTinhNguyenVien>();

    public virtual ICollection<ThoiGianChiTietSuKien> ThoiGianChiTietSuKiens { get; set; } = new List<ThoiGianChiTietSuKien>();

    public virtual ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();

    public virtual NguoiDung? User { get; set; }
}
