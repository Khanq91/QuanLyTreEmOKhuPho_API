using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class HoanCanh
{
    public int HoanCanhId { get; set; }

    public string? LoaiHoanCanh { get; set; }

    public string? MoTa { get; set; }

    public virtual ICollection<TreEmHoanCanh> TreEmHoanCanhs { get; set; } = new List<TreEmHoanCanh>();

    public virtual ICollection<VanDongTreEm> VanDongTreEms { get; set; } = new List<VanDongTreEm>();
}
