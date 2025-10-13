using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class ThongTinPhuHuynh
{
    public int PhuHuynhId { get; set; }

    public string? HoTen { get; set; }

    public string? Sdt { get; set; }

    public string? DiaChi { get; set; }

    public string? NgheNghiep { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public string? TonGiao { get; set; }

    public string? DanToc { get; set; }

    public string? QuocTich { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<TreEmPhuHuynh> TreEmPhuHuynhs { get; set; } = new List<TreEmPhuHuynh>();

    public virtual NguoiDung? User { get; set; }
}
