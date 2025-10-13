using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class NguoiDung
{
    public int UserId { get; set; }

    public string? HoTen { get; set; }

    public string? Email { get; set; }

    public string? MatKhau { get; set; }

    public string? VaiTro { get; set; }

    public DateOnly? NgayTao { get; set; }

    public virtual ICollection<DangKySuKien> DangKySuKiens { get; set; } = new List<DangKySuKien>();

    public virtual ICollection<SuKien> SuKiens { get; set; } = new List<SuKien>();

    public virtual ICollection<ThongTinPhuHuynh> ThongTinPhuHuynhs { get; set; } = new List<ThongTinPhuHuynh>();

    public virtual TinhNguyenVien? TinhNguyenVien { get; set; }

    public virtual ICollection<VanDongTreEm> VanDongTreEms { get; set; } = new List<VanDongTreEm>();

    public virtual ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();
}
