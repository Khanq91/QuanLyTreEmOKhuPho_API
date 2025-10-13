using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class PhieuHocTap
{
    public int PhieuHocTapId { get; set; }

    public double? DiemTrungBinh { get; set; }

    public string? XepLoai { get; set; }

    public string? HanhKiem { get; set; }

    public string? GhiChu { get; set; }

    public DateOnly? NgayCapNhat { get; set; }

    public int? TruongId { get; set; }

    public int? TreEmId { get; set; }

    public int? LopId { get; set; }

    public virtual LopHoc? Lop { get; set; }

    public virtual TreEm? TreEm { get; set; }

    public virtual TruongHoc? Truong { get; set; }
}
