using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class VanDongTreEm
{
    public int VanDongId { get; set; }

    public int? TreEmId { get; set; }

    public int? HoanCanhId { get; set; }

    public int? NguoiDungId { get; set; }

    public int? SoLan { get; set; }

    public string? LyDo { get; set; }

    public string? KetQua { get; set; }

    public DateOnly? NgayVanDong { get; set; }

    public virtual HoanCanh? HoanCanh { get; set; }

    public virtual NguoiDung? NguoiDung { get; set; }

    public virtual TreEm? TreEm { get; set; }
}
