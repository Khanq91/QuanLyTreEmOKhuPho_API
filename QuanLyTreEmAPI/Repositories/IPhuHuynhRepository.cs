using QuanLyTreEmAPI.Models;

namespace QuanLyTreEmAPI.Repositories
{
    public interface IPhuHuynhRepository
    {
        Task<ThongTinPhuHuynh> CreateAsync(ThongTinPhuHuynh phuHuynh);
        Task<bool> AddTreEmPhuHuynhAsync(TreEmPhuHuynh relation);
        Task<List<ThongTinPhuHuynh>> GetPhuHuynhByTreEmIdAsync(int treEmId);
    }
}
