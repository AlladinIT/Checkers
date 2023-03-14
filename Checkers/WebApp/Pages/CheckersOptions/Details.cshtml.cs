using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.CheckersOptions
{
    public class DetailsModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public DetailsModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

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
    }
}
