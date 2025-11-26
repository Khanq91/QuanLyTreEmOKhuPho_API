namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class TabQuaDaNhanResponseDTO
    {
        public List<QuaPhanPhatInfoDTO> DanhSachQua { get; set; } = new List<QuaPhanPhatInfoDTO>();
        public int TongSoQua { get; set; }
    }
}
