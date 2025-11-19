namespace QuanLyTreEmAPI.DTOs.HoTroVaPhucLoi
{
    public class DsTreEm
    {
        public int TreEmId { get; set; }
        public string TenTreEm { get; set; }
        public DateTime? NgaySinh { get; set; } // Nullable
        public string NgaySinhDisplay { get; set; } // Format sẵn
        public string KhuPho { get; set; }
        public string TinhTrang { get; set; }

    }
}
