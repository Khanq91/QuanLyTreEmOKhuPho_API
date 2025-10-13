using QuanLyTreEmAPI.Models;

namespace QuanLyTreEmAPI.Repositories
{
    public interface IHoTroPhucLoiRepository
    {
        Task<HoTroPhucLoi> CreateAsync(HoTroPhucLoi hoTro);
        Task<List<HoTroPhucLoi>> GetByTreEmIdAsync(int treEmId);
    }
}
