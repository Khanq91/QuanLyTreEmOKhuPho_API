namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class HoTroInfoDTO
    {
        public int HoTroID { get; set; }
        public string LoaiHoTro { get; set; }
        public string MoTa { get; set; }
        public DateOnly? NgayCap { get; set; }
        public string NguoiChiuTrachNhiemHoTro { get; set; }
    }
}
