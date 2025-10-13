namespace QuanLyTreEmAPI.DTOs.TreEm
{
    public class HoTroPhucLoiDTO
    {
        public int HoTroID { get; set; }
        public string LoaiHoTro { get; set; }
        public string MoTa { get; set; }
        public DateTime? NgayCap { get; set; }
        public string NguoiChiuTrachNhiemHoTro { get; set; }
        public List<MinhChungDTO> DanhSachMinhChung { get; set; }
    }
}
