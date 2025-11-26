namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class ChuongTrinhSuKienDTO
    {
        public int ThoiGianChiTietSuKienId { get; set; }
        public string MoTa { get; set; }
        public string ThoiGianBatDau { get; set; } // dd/MM/yyyy HH:mm
        public string ThoiGianKetThuc { get; set; } // dd/MM/yyyy HH:mm
        public List<TietMucInfoDTO> DanhSachTietMuc { get; set; }
    }
}
