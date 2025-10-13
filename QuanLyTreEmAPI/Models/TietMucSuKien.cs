using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class TietMucSuKien
{
    public int TietMucId { get; set; }

    public string? TenTietMuc { get; set; }

    public string? NguoiThucHien { get; set; }

    public decimal? ChiPhiTietMuc { get; set; }

    public int? ThoiGianChiTietSuKienId { get; set; }

    public virtual ThoiGianChiTietSuKien? ThoiGianChiTietSuKien { get; set; }
}
