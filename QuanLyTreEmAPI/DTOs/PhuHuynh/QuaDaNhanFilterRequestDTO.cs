namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class QuaDaNhanFilterRequestDTO
    {
        public string Filter { get; set; } = "all"; // "all", "da-nhan", "dang-tien-hanh"
        public string SortBy { get; set; } = "ngay-nhan"; // "ngay-nhan"
    }
}
