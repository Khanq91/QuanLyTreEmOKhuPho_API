using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTreEmAPI.Models;

public partial class ThongBao
{
    [Column("ThongBaoID")]
    public int ThongBaoID { get; set; }

    public int? SuKienId { get; set; }

    public string? NoiDung { get; set; }

    public DateOnly? NgayThongBao { get; set; }

    public virtual SuKien? SuKien { get; set; }

    //public virtual ICollection<NguoiDung> Users { get; set; } = new List<NguoiDung>();
    public virtual ICollection<ThongBaoNguoiDung> ThongBaoNguoiDungs { get; set; } = new List<ThongBaoNguoiDung>();
}
