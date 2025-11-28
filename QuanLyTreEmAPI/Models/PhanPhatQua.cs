using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTreEmAPI.Models
{
    public class PhanPhatQua
    {
        [Column("PhanPhatID")]
        public int PhanPhatId { get; set; }

        [Column("QuaTangUngHoID")]
        public int QuaTangUngHoId { get; set; }

        [Column("TreEmID")]
        public int TreEmId { get; set; }

        public int SoLuongNhan { get; set; }
        public DateTime NgayPhanPhat { get; set; }
        public string? NguoiPhanPhat { get; set; }
        public string? TrangThai { get; set; }
        public string? GhiChu { get; set; }

        // Navigation properties
        public virtual QuaTangUngHo? QuaTangUngHo { get; set; }
        public virtual TreEm? TreEm { get; set; }
    }
}