using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class ChiPhiSuKien
{
    public int ChiPhiId { get; set; }

    public string? TenKhoanChi { get; set; }

    public decimal? SoTien { get; set; }
    public string? NguoiPheDuyet { get; set; }
    public string? VanBanPheDuyet { get; set; }
    public DateOnly? NgayPheDuyet { get; set; }


    public string? GhiChu { get; set; }

    public int? SuKienID { get; set; }

    public virtual ICollection<ChiTietChiPhiSuKien> ChiTietChiPhiSuKiens { get; set; } = new List<ChiTietChiPhiSuKien>();
    public virtual SuKien? SuKien { get; set; }
    public virtual ICollection<PhanBoUngHoChiPhi> PhanBoUngHoChiPhis { get; set; } = new List<PhanBoUngHoChiPhi>();
}