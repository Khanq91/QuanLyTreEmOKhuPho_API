using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class UngHo
{
    public int UngHoId { get; set; }

    public decimal? SoTien { get; set; }

    public string? LoaiUngHo { get; set; }

    public DateOnly? NgayUngHo { get; set; }

    public string? GhiChu { get; set; }

    public int? ManhThuongQuanId { get; set; }

    public virtual ManhThuongQuan? ManhThuongQuan { get; set; }

    public virtual ICollection<HoTroPhucLoi> HoTros { get; set; } = new List<HoTroPhucLoi>();

    public virtual ICollection<TreEm> TreEms { get; set; } = new List<TreEm>();
}
