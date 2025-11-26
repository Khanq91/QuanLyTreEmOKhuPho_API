namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class ChiTietLichTrongDTO
    {
        public string Thu { get; set; } // "Thứ 2", "Thứ 3"...
        public string Buoi { get; set; } // "Sáng", "Chiều", "Tối"
        public bool IsAvailable { get; set; } // true = rảnh (màu xanh)
    }
}
