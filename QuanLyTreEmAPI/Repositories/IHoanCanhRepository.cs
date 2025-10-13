using QuanLyTreEmAPI.Models;

namespace QuanLyTreEmAPI.Repositories
{
    public interface IHoanCanhRepository
    {
        Task<List<HoanCanh>> GetAllAsync();
        Task<HoanCanh> GetByIdAsync(int id);
    }
}
