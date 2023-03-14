using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.CheckersGameStates
{
    public class DeleteModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public DeleteModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var checkersgamestate = await _context.CheckersGameStates.FindAsync(id);

            if (checkersgamestate != null)
            {
                CheckersGameState = checkersgamestate;
                _context.CheckersGameStates.Remove(CheckersGameState);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
