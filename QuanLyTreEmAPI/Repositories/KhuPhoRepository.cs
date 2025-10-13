using QuanLyTreEmAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyTreEmAPI.Repositories
{
    public class KhuPhoRepository : IKhuPhoRepository
    {
        private readonly QuanLyTreEmAPI.Data.QuanLyTreEmContext _context;

        public KhuPhoRepository(QuanLyTreEmAPI.Data.QuanLyTreEmContext context)
        {
            _context = context;
        }

        public async Task<List<KhuPho>> GetAllAsync()
        {
            return await _context.KhuPhos.ToListAsync();
        }

        public async Task<KhuPho> GetByIdAsync(int id)
        {
            return await _context.KhuPhos.FindAsync(id);
        }
    }
}
