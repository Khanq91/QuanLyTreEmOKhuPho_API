using System;
using System.Collections.Generic;
namespace QuanLyTreEmAPI.Models;

public partial class UngHo
{
    public int UngHoId { get; set; }
    public decimal? SoTien { get; set; }
    public string? LoaiUngHo { get; set; }
    public string? DoiTuong { get; set; }
    public string? TenVatPham { get; set; }

    public DateOnly? NgayUngHo { get; set; }
    public string? GhiChu { get; set; }
    public int? SoLuongVatPham { get; set; }
    public int? SoLuongConLai { get; set; }
    public int? ManhThuongQuanId { get; set; }

    public virtual ManhThuongQuan? ManhThuongQuan { get; set; }

    //public virtual ICollection<HoTroPhucLoi> HoTros { get; set; } = new List<HoTroPhucLoi>();
    public virtual ICollection<PhieuMinhChung> PhieuMinhChungs { get; set; } = new List<PhieuMinhChung>();

    public virtual ICollection<TreEm> TreEms { get; set; } = new List<TreEm>();
    public virtual ICollection<QuaTangUngHo> QuaTangUngHos { get; set; } = new List<QuaTangUngHo>();
    public virtual ICollection<PhanBoUngHoChiPhi> PhanBoUngHoChiPhis { get; set; } = new List<PhanBoUngHoChiPhi>();
}