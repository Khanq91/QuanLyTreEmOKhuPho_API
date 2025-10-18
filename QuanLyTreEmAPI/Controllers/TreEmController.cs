using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyTreEmAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyTreEmAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class TreEmController : ControllerBase
    {
        private readonly QuanLyTreEmContext _context;
        public TreEmController(QuanLyTreEmContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var treEms = await _context.TreEms.ToListAsync();
            return Ok(treEms);
        }
        [HttpGet("TongTreEmTheoKhuPho")]
        public async Task<IActionResult> TongTreEmTheoKhuPho()
        {
            var tongTreEm = await _context.TreEms
               .Include(te => te.KhuPho)
               .GroupBy(te => te.KhuPhoId)
               .Select(g => new
               {
                   KhuPhoID = g.Key,
                   TenKhuPho = g.FirstOrDefault().KhuPho.TenKhuPho,
                   SoLuongTreEm = g.Count()
               })
               .ToListAsync();


            return Ok(tongTreEm);
        }

    }
}
