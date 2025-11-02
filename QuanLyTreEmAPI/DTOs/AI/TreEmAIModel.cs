namespace QuanLyTreEmAPI.DTOs.AI
{
    public class TreEmAIModel
    {
        public int TreEmID { get; set; }
        public string HoTen { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string TinhTrang { get; set; }
        public string TenKhuPho { get; set; }
        public string TenTruong { get; set; }

        // Hoàn cảnh
        public List<string> DanhSachHoanCanh { get; set; } = new List<string>();

        // Học tập
        public float? DiemTrungBinh { get; set; }
        public string XepLoai { get; set; }
        public string HanhKiem { get; set; }

        // Lịch sử hỗ trợ
        public int SoLanHoTro { get; set; }
        public DateTime? NgayHoTroGanNhat { get; set; }

        // Tính toán
        public int Tuoi => NgaySinh.HasValue
     ? DateTime.Now.Year - NgaySinh.Value.Year
     : 0; // hoặc một giá trị mặc định nếu null

        public int? SoThangChuaHoTro => NgayHoTroGanNhat.HasValue
            ? (int)((DateTime.Now - NgayHoTroGanNhat.Value).TotalDays / 30)
            : null;
    }

    public class PhanLoaiUuTienResponse
    {
        public int TreEmID { get; set; }
        public string HoTen { get; set; }
        public int Tuoi { get; set; }
        public string TinhTrang { get; set; }
        public string MucDoUuTien { get; set; } // Khẩn cấp, Cao, Trung bình, Thấp
        public int DiemUuTien { get; set; } // 0-100

        public ChiTietDiem ChiTietDiem { get; set; }
        public List<string> LyDo { get; set; } = new List<string>();
        public List<string> DeXuat { get; set; } = new List<string>();

        // Thông tin bổ sung
        public string TenKhuPho { get; set; }
        public string TenTruong { get; set; }
        public List<string> HoanCanh { get; set; }
        public float? DiemTrungBinh { get; set; }
        public string XepLoai { get; set; }
        public int SoLanHoTro { get; set; }
        public DateTime? NgayHoTroGanNhat { get; set; }
    }

    public class ChiTietDiem
    {
        public int DiemHoanCanh { get; set; }
        public int DiemHocTap { get; set; }
        public int DiemTinhTrang { get; set; }
        public int DiemThoiGian { get; set; }
        public int TongDiem { get; set; }
    }

    public class Top5Response
    {
        public int TotalCount { get; set; }
        public List<PhanLoaiUuTienResponse> Top5UuTien { get; set; }
        public DateTime NgayPhanLoai { get; set; }
        public ThongKeTongQuat ThongKe { get; set; }
    }

    public class ThongKeTongQuat
    {
        public int TongSoTreEm { get; set; }
        public int SoKhanCap { get; set; }
        public int SoCao { get; set; }
        public int SoTrungBinh { get; set; }
        public int SoThap { get; set; }
    }
}
