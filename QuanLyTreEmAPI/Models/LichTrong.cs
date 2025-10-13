using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class LichTrong
{
    public int LichTrongId { get; set; }

    public int? TinhNguyenVienId { get; set; }

    public virtual ICollection<ChiTietLichTrong> ChiTietLichTrongs { get; set; } = new List<ChiTietLichTrong>();

    public virtual TinhNguyenVien? TinhNguyenVien { get; set; }
}
