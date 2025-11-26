namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class DanhSachSuKienDTO
    {
        public int SuKienId { get; set; }
        public string TenSuKien { get; set; }
        public string NgayBatDau { get; set; } // dd/MM/yyyy
        public string NgayKetThuc { get; set; } // dd/MM/yyyy
        public string DiaDiem { get; set; }
        public string TenKhuPho { get; set; }
        public bool DaDangKy { get; set; } // Đã đăng ký hay chưa
        public string TrangThaiDangKy { get; set; } // Chờ duyệt, Đã duyệt, Từ chối
    }
}
