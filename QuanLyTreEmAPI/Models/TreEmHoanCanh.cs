using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class TreEmHoanCanh
{
    public int TreEmId { get; set; }

    public int HoanCanhId { get; set; }

    public DateOnly? NgayCapNhat { get; set; }

    public virtual HoanCanh HoanCanh { get; set; } = null!;

    public virtual TreEm TreEm { get; set; } = null!;
}
