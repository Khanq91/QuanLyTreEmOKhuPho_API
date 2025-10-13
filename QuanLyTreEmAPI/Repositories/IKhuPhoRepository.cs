using QuanLyTreEmAPI.Models;

namespace QuanLyTreEmAPI.Repositories
{
    public interface IKhuPhoRepository
    {
        Task<List<KhuPho>> GetAllAsync();
        Task<KhuPho> GetByIdAsync(int id);
    }
}
