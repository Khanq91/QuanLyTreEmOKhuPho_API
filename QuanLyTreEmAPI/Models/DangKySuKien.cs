using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class DangKySuKien
{
    public int DangKySuKienId { get; set; }

    public int? SuKienId { get; set; }

    public int? UserId { get; set; }

    public DateOnly? NgayDangKy { get; set; }

    public string? TrangThai { get; set; }

    public virtual SuKien? SuKien { get; set; }

    public virtual NguoiDung? User { get; set; }
}
