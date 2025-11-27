
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTreEmAPI.Models;

public partial class TietMucSuKien
{

    public int TietMucId { get; set; }
    public int SuKienID { get; set; }

    public string? TenTietMuc { get; set; }

    public string? NguoiThucHien { get; set; }

    public decimal? ChiPhiTietMuc { get; set; }

    public int? ThoiGianChiTietSuKienId { get; set; }

    public virtual ThoiGianChiTietSuKien? ThoiGianChiTietSuKien { get; set; }
}
