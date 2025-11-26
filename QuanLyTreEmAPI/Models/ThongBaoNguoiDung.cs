using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTreEmAPI.Models
{
    public class ThongBaoNguoiDung
    {
        [Column("ThongBaoID")]
        public int ThongBaoID { get; set; }

        public int UserId { get; set; }

        public bool DaDoc { get; set; }

        public virtual ThongBao ThongBao { get; set; } = null!;

        public virtual NguoiDung User { get; set; } = null!;
    }
}
