using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.CheckersOptions
{
    public class DeleteModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public DeleteModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
      public CheckersOption CheckersOption { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkersoption = await _context.CheckersOptions.FirstOrDefaultAsync(m => m.Id == id);

            if (checkersoption == null)
            {
                return NotFound();
            }
            else 
            {
                CheckersOption = checkersoption;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var checkersoption = await _context.CheckersOptions.FindAsync(id);

            if (checkersoption != null)
            {
                CheckersOption = checkersoption;
                _context.CheckersOptions.Remove(CheckersOption);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
