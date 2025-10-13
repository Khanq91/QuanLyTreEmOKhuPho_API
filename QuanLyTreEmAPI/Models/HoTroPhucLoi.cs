using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class HoTroPhucLoi
{
    public int HoTroId { get; set; }

    public string? LoaiHoTro { get; set; }

    public string? MoTa { get; set; }

    public DateOnly? NgayCap { get; set; }

    public string? NguoiChiuTrachNhiemHoTro { get; set; }

    public int? TreEmId { get; set; }

    public virtual ICollection<PhieuMinhChung> PhieuMinhChungs { get; set; } = new List<PhieuMinhChung>();

    public virtual TreEm? TreEm { get; set; }

    public virtual ICollection<UngHo> UngHos { get; set; } = new List<UngHo>();
}
