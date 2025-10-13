using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class ThongBao
{
    public int ThongBaoId { get; set; }

    public int? SuKienId { get; set; }

    public string? NoiDung { get; set; }

    public DateOnly? NgayThongBao { get; set; }

    public virtual SuKien? SuKien { get; set; }

    public virtual ICollection<NguoiDung> Users { get; set; } = new List<NguoiDung>();
}
