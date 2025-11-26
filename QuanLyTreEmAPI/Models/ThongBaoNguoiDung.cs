namespace QuanLyTreEmAPI.Models
{
    public class ThongBaoNguoiDung
    {
        public int ThongBaoId { get; set; }

        public int UserId { get; set; }

        public bool DaDoc { get; set; }

        public virtual ThongBao ThongBao { get; set; } = null!;

        public virtual NguoiDung User { get; set; } = null!;
    }
}
