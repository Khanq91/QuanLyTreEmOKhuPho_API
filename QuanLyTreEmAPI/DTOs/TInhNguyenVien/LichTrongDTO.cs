namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class LichTrongDTO
    {
        public int? LichTrongId { get; set; }
        public bool IsEmpty { get; set; } // true nếu chưa có dữ liệu
        public List<ChiTietLichTrongDTO> ChiTietLichTrong { get; set; }
    }
}
