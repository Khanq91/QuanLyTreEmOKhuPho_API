using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class TruongHoc
{
    public int TruongId { get; set; }

    public string? TenTruong { get; set; }

    public string? DiaChi { get; set; }

    public string? CapHoc { get; set; }

    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();

    public virtual ICollection<PhieuHocTap> PhieuHocTaps { get; set; } = new List<PhieuHocTap>();

    public virtual ICollection<TreEm> TreEms { get; set; } = new List<TreEm>();
}
