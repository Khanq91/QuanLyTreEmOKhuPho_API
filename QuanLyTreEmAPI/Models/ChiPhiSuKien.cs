using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class ChiPhiSuKien
{
    public int ChiPhiId { get; set; }

    public string? TenKhoanChi { get; set; }

    public decimal? SoTien { get; set; }

    public string? GhiChu { get; set; }

    public int? SuKienId { get; set; }

    public virtual ICollection<ChiTietChiPhiSuKien> ChiTietChiPhiSuKiens { get; set; } = new List<ChiTietChiPhiSuKien>();

    public virtual SuKien? SuKien { get; set; }
}
