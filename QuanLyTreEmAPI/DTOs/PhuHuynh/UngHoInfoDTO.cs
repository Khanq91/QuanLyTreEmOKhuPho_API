namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class UngHoInfoDTO
    {
        public int UngHoID { get; set; }
        public decimal? SoTien { get; set; }
        public string LoaiUngHo { get; set; }
        public string NgayUngHo { get; set; } // dd/MM/yyyy
        public string GhiChu { get; set; }
        public string TenManhThuongQuan { get; set; }
        public string LoaiManhThuongQuan { get; set; }
    }
}
