using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.CheckersOptions
{
    public class IndexModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public IndexModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

        public IList<CheckersOption> CheckersOption { get;set; } = default!;

        
        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }
        
        public async Task OnGetAsync()
        {
            var query = _context.CheckersOptions.AsQueryable();
            
            if (!string.IsNullOrEmpty(Search))
            {
                Search = Search.Trim().ToUpper();

                query = query.Where(o =>
                    o.Name.ToUpper().Contains(Search)
                );
            }
            
            
            CheckersOption = await query.ToListAsync();
        }
    }
}
