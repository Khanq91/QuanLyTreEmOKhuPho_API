namespace QuanLyTreEmAPI.Models
{
    public class TreEmSuKien
    {
        public int TreEmId { get; set; }
        public int SuKienId { get; set; }

        public virtual TreEm TreEm { get; set; }
        public virtual SuKien SuKien { get; set; }
    }
}