using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class PhanCongTinhNguyenVien
{
    public int PhanCongId { get; set; }

    public int? SuKienId { get; set; }

    public int? TinhNguyenVienId { get; set; }

    public string? CongViec { get; set; }

    public string? GhiChu { get; set; }

    public DateOnly? NgayPhanCong { get; set; }

    public virtual SuKien? SuKien { get; set; }

    public virtual TinhNguyenVien? TinhNguyenVien { get; set; }
}
