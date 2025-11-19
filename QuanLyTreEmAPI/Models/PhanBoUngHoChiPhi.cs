using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTreEmAPI.Models
{
    public class PhanBoUngHoChiPhi
    {
        public int PhanBoId { get; set; }
        public int? UngHoId { get; set; }
        public int? ChiPhiId { get; set; }
        public decimal? SoTienPhanBo { get; set; }
        public decimal? TyLe { get; set; }
        public string? NguoiPheDuyet { get; set; }
        public DateOnly? NgayPheDuyet { get; set; }
        public string? GhiChu { get; set; }
        public virtual UngHo? UngHo { get; set; }

        public virtual ChiPhiSuKien? ChiPhiSuKien { get; set; }
    }
}