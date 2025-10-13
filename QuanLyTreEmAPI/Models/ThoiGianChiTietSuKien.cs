using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class ThoiGianChiTietSuKien
{
    public int ThoiGianChiTietSuKienId { get; set; }

    public string? MoTa { get; set; }

    public DateTime? ThoiGianBatDau { get; set; }

    public DateTime? ThoiGianKetThuc { get; set; }

    public int? SuKienId { get; set; }

    public virtual SuKien? SuKien { get; set; }

    public virtual ICollection<TietMucSuKien> TietMucSuKiens { get; set; } = new List<TietMucSuKien>();
}
