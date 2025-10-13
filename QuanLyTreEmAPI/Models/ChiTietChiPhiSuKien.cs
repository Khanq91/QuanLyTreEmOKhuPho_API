using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class ChiTietChiPhiSuKien
{
    public int ChiTietChiPhi { get; set; }

    public string? TenPhanQua { get; set; }

    public string? NguoiDaiDien { get; set; }

    public int? SoLuong { get; set; }

    public decimal? DonGia { get; set; }

    public int? ChiPhiId { get; set; }

    public virtual ChiPhiSuKien? ChiPhi { get; set; }
}
