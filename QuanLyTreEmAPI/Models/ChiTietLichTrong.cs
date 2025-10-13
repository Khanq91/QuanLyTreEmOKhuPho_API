using System;
using System.Collections.Generic;

namespace QuanLyTreEmAPI.Models;

public partial class ChiTietLichTrong
{
    public int ChiTietLichTrongId { get; set; }

    public string? Buoi { get; set; }

    public string? Thu { get; set; }

    public int? LichTrongId { get; set; }

    public virtual LichTrong? LichTrong { get; set; }
}
