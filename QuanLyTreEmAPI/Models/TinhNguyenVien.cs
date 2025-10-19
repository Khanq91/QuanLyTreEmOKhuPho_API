using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class TinhNguyenVien
{
    public int TinhNguyenVienId { get; set; }

    public string? Sdt { get; set; }

    public DateTime? NgaySinh { get; set; }

    public string? ChucVu { get; set; }

    public int? UserId { get; set; }

    public int? KhuPhoId { get; set; }

    public virtual KhuPho? KhuPho { get; set; }

    public virtual LichTrong? LichTrong { get; set; }

    public virtual ICollection<PhanCongTinhNguyenVien> PhanCongTinhNguyenViens { get; set; } = new List<PhanCongTinhNguyenVien>();

    public virtual NguoiDung? User { get; set; }
}
