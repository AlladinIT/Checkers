using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.CheckersOptions
{
    public class CreateModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        
        private readonly IGameRepository _gameRepo;
    
        private readonly IGameOptionsRepository _optionsRepo;
    
        private readonly IGameStateRepository _stateRepo;
        public CreateModel(DAL.Db.AppDbContext context, IGameRepository gameRepo, IGameOptionsRepository optionsRepo, IGameStateRepository stateRepo)
        {
            _context = context;
            _gameRepo = gameRepo;
            _optionsRepo = optionsRepo;
            _stateRepo = stateRepo;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public CheckersOption CheckersOption { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if (!ModelState.IsValid)
          {
              return Page();
          }
          
          if (_optionsRepo.GetGameOptionsListOfNames().Contains(CheckersOption.Name))
          {
              return RedirectToPage("./Create", new { error = "Options with this name already exists!" });
          }

          _context.CheckersOptions.Add(CheckersOption);
          await _context.SaveChangesAsync();

          return RedirectToPage("./Index");
        }
    }
}
