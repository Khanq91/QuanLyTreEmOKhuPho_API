using QuanLyTreEmAPI.Models;

namespace QuanLyTreEmAPI.Repositories
{
    public interface IVanDongTreEmRepository
    {
        Task<VanDongTreEm> CreateAsync(VanDongTreEm vanDong);
        Task<List<VanDongTreEm>> GetByTreEmIdAsync(int treEmId);
    }
}
