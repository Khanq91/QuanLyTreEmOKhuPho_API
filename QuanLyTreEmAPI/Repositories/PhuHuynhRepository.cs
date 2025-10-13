using QuanLyTreEmAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyTreEmAPI.Repositories
{
    public class PhuHuynhRepository : IPhuHuynhRepository
    {
        private readonly QuanLyTreEmAPI.Data.QuanLyTreEmContext _context;

        public PhuHuynhRepository(QuanLyTreEmAPI.Data.QuanLyTreEmContext context)
        {
            _context = context;
        }

        public async Task<ThongTinPhuHuynh> CreateAsync(ThongTinPhuHuynh phuHuynh)
        {
            _context.ThongTinPhuHuynhs.Add(phuHuynh);
            await _context.SaveChangesAsync();
            return phuHuynh;
        }

        public async Task<bool> AddTreEmPhuHuynhAsync(TreEmPhuHuynh relation)
        {
            _context.TreEmPhuHuynhs.Add(relation);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<ThongTinPhuHuynh>> GetPhuHuynhByTreEmIdAsync(int treEmId)
        {
            return await _context.TreEmPhuHuynhs
                .Where(tp => tp.TreEmId == treEmId)
                .Include(tp => tp.PhuHuynh)
                .Select(tp => tp.PhuHuynh)
                .ToListAsync();
        }
    }
}
