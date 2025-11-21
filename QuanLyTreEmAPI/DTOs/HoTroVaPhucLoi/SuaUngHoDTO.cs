namespace QuanLyTreEmAPI.DTOs.HoTroVaPhucLoi
{
    public class SuaUngHoDTO
    {
        public int QuaTangUngHoId { get; set; }
        public string LoaiHoTro { get; set; }
        public string NguoiChiuTrachNhiem { get; set; }
        public string MoTaQua { get; set; }
        public string TenQuaTang { get; set; }
        public string DoiTuongNhan { get; set; }
        public string NguoiNhanPhat { get; set; }
        public List<int> PhanPhatQuaIds { get; set; }
    }
}
