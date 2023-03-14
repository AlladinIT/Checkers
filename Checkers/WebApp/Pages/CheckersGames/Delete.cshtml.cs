using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.CheckersGames
{
    public class DeleteModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        private readonly IGameRepository _gameRepo;
    
        private readonly IGameOptionsRepository _optionsRepo;
    
        private readonly IGameStateRepository _stateRepo;
        public DeleteModel(DAL.Db.AppDbContext context, IGameRepository gameRepo, IGameOptionsRepository optionsRepo, IGameStateRepository stateRepo)
        {
            _context = context;
            _gameRepo = gameRepo;
            _optionsRepo = optionsRepo;
            _stateRepo = stateRepo;
        }

        [BindProperty]
      public CheckersGame CheckersGame { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            
            //var checkersgame = await _context.CheckersGames.FirstOrDefaultAsync(m => m.Id == id);

            var checkersgame = _gameRepo.GetAllAboutGame(id);
            if (checkersgame == null)
            {
                return NotFound();
            }

            CheckersGame = checkersgame;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var checkersgame = await _context.CheckersGames.FindAsync(id);

            if (checkersgame != null)
            {
                CheckersGame = checkersgame;
                _context.CheckersGames.Remove(CheckersGame);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
