using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class LopHoc
{
    public int LopId { get; set; }

    public string? TenLop { get; set; }

    public int? TruongId { get; set; }

    public virtual ICollection<PhieuHocTap> PhieuHocTaps { get; set; } = new List<PhieuHocTap>();

    public virtual TruongHoc? Truong { get; set; }
}
