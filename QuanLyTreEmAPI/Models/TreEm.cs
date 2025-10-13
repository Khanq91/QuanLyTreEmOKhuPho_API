using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class TreEm
{
    public int TreEmId { get; set; }

    public string? HoTen { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public string? GioiTinh { get; set; }

    public string? TonGiao { get; set; }

    public string? DanToc { get; set; }

    public string? QuocTich { get; set; }

    public int? TruongId { get; set; }

    public string? TinhTrang { get; set; }

    public int? KhuPhoId { get; set; }

    public virtual ICollection<HoTroPhucLoi> HoTroPhucLois { get; set; } = new List<HoTroPhucLoi>();

    public virtual KhuPho? KhuPho { get; set; }

    public virtual ICollection<PhieuHocTap> PhieuHocTaps { get; set; } = new List<PhieuHocTap>();

    public virtual ICollection<TreEmHoanCanh> TreEmHoanCanhs { get; set; } = new List<TreEmHoanCanh>();

    public virtual ICollection<TreEmPhuHuynh> TreEmPhuHuynhs { get; set; } = new List<TreEmPhuHuynh>();

    public virtual TruongHoc? Truong { get; set; }

    public virtual ICollection<VanDongTreEm> VanDongTreEms { get; set; } = new List<VanDongTreEm>();

    public virtual ICollection<UngHo> UngHos { get; set; } = new List<UngHo>();
}
