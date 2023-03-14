using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.CheckersGameStates
{
    public class DetailsModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public DetailsModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

      public CheckersGameState CheckersGameState { get; set; } = default!; 

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkersgamestate = await _context.CheckersGameStates.FirstOrDefaultAsync(m => m.Id == id);
            if (checkersgamestate == null)
            {
                return NotFound();
            }
            else 
            {
                CheckersGameState = checkersgamestate;
            }
            return Page();
        }
    }
}
