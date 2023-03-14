using Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.CheckersGameStates
{
    public class IndexModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public IndexModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

        public IList<CheckersGameState> CheckersGameState { get;set; } = default!;

        public async Task OnGetAsync()
        {
            CheckersGameState = await _context.CheckersGameStates
                .Include(c => c.CheckersGame).ToListAsync();
        }
    }
}
