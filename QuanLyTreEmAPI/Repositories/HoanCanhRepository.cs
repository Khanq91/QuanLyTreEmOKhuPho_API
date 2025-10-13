using QuanLyTreEmAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyTreEmAPI.Repositories
{
    public class HoanCanhRepository : IHoanCanhRepository
    {
        private readonly QuanLyTreEmAPI.Data.QuanLyTreEmContext _context;

        public HoanCanhRepository(QuanLyTreEmAPI.Data.QuanLyTreEmContext context)
        {
            _context = context;
        }

        public async Task<List<HoanCanh>> GetAllAsync()
        {
            return await _context.HoanCanhs.ToListAsync();
        }

        public async Task<HoanCanh> GetByIdAsync(int id)
        {
            return await _context.HoanCanhs.FindAsync(id);
        }
    }
}
