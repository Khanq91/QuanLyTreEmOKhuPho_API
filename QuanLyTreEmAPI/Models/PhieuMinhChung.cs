using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class PhieuMinhChung
{
    public int MinhChungId { get; set; }

    public string? LoaiMinhChung { get; set; }

    public string? FilePath { get; set; }

    public DateOnly? NgayCap { get; set; }

    public int? HoTroId { get; set; }

    public virtual HoTroPhucLoi? HoTro { get; set; }
}
