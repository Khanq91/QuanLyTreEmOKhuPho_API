using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTreEmAPI.Models
{
    public class QuaTangUngHo
    {
        public int QuaTangUngHoId { get; set; }
        public int? UngHoId { get; set; }
        public int? SuKienId { get; set; }
        public string TenQua { get; set; }
        public string MoTa { get; set; }
        public int SoLuongTong { get; set; }
        public int SoLuongConLai { get; set; }
        public decimal DonGia { get; set; }
        public string DoiTuongNhan { get; set; }
        public string Anh { get; set; }
        // Navigation
        public virtual UngHo UngHo { get; set; }
        public virtual SuKien SuKien { get; set; }
        public virtual ICollection<PhanPhatQua> PhanPhatQuas { get; set; }
    }
}
