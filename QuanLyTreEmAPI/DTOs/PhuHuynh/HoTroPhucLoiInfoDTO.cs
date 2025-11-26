namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class HoTroPhucLoiInfoDTO
    {
        public int HoTroID { get; set; }
        public string LoaiHoTro { get; set; }
        public string MoTa { get; set; }
        public string NgayCap { get; set; } // dd/MM/yyyy
        public string NguoiChiuTrachNhiem { get; set; }
        public List<MinhChungInfoDTO> DanhSachMinhChung { get; set; }
    }
}
