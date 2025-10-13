using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class KhuPho
{
    public int KhuPhoId { get; set; }

    public string? TenKhuPho { get; set; }

    public string? DiaChi { get; set; }

    public string? QuanHuyen { get; set; }

    public string? ThanhPho { get; set; }

    public virtual ICollection<SuKien> SuKiens { get; set; } = new List<SuKien>();

    public virtual ICollection<TinhNguyenVien> TinhNguyenViens { get; set; } = new List<TinhNguyenVien>();

    public virtual ICollection<TreEm> TreEms { get; set; } = new List<TreEm>();
}
