using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class ManhThuongQuan
{
    public int ManhThuongQuanId { get; set; }

    public string? Ten { get; set; }

    public string? Loai { get; set; }

    public string? DiaChi { get; set; }

    public string? Sdt { get; set; }

    public string? Email { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<UngHo> UngHos { get; set; } = new List<UngHo>();
}
