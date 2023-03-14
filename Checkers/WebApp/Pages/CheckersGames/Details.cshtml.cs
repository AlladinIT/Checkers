using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.CheckersGames
{
    public class DetailsModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public DetailsModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

      public CheckersGame CheckersGame { get; set; } = default!; 

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkersgame = await _context.CheckersGames.FirstOrDefaultAsync(m => m.Id == id);
            if (checkersgame == null)
            {
                return NotFound();
            }
            else 
            {
                CheckersGame = checkersgame;
            }
            return Page();
        }
    }
}
