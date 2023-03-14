using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Pages.CheckersGameStates
{
    public class CreateModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public CreateModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["CheckersGameId"] = new SelectList(_context.CheckersGames, "Id", "Name");
            return Page();
        }

        [BindProperty]
        public CheckersGameState CheckersGameState { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.CheckersGameStates.Add(CheckersGameState);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
