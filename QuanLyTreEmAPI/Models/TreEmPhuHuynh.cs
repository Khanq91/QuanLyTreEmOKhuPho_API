using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class TreEmPhuHuynh
{
    public int TreEmId { get; set; }

    public int PhuHuynhId { get; set; }

    public string? MoiQuanHe { get; set; }

    public virtual ThongTinPhuHuynh PhuHuynh { get; set; } = null!;

    public virtual TreEm TreEm { get; set; } = null!;
}
