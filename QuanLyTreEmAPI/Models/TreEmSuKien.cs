
namespace QuanLyTreEmAPI.Models
{
    public class TreEmSuKien
    {
        public int TreEmSuKienId { get; set; }
        public int TreEmId { get; set; }
        public int SuKienId { get; set; }
        public DateTime? NgayDangKy { get; set; }
        public string? GhiChu { get; set; }

        // ⭐ THÊM PROPERTY NÀY
        public string? TrangThai { get; set; }

        public virtual TreEm TreEm { get; set; } = null!;
        public virtual SuKien SuKien { get; set; } = null!;

    }
}
