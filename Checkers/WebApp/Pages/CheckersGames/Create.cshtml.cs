using System.Text.Json;
using DAL;
using Domain;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Pages.CheckersGames
{
    public class CreateModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;
        private readonly IGameRepository _gameRepo;
    
        private readonly IGameOptionsRepository _optionsRepo;
    
        private readonly IGameStateRepository _stateRepo;
        public CreateModel(DAL.Db.AppDbContext context, IGameRepository repo, IGameRepository gameRepo, IGameOptionsRepository optionsRepo, IGameStateRepository stateRepo)
        {
            _context = context;
            _gameRepo = gameRepo;
            _optionsRepo = optionsRepo;
            _stateRepo = stateRepo;
        }

        public IActionResult OnGet()
        {
            OptionsSelectList = new SelectList(_context.CheckersOptions, "Id", "Name");
            return Page();
        }

        [BindProperty]
        public CheckersGame CheckersGame { get; set; } = default!;


        public SelectList OptionsSelectList { get; set; } = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (_gameRepo.GetGamesListOfNames().Contains(CheckersGame.Name))
            {
                return RedirectToPage("./Create", new { error = "Game with this name already exists!" });
            }

            _gameRepo.SaveGame(CheckersGame.Name,CheckersGame,CheckersGame.CheckersOption!);
            _optionsRepo.GetGameOptionById(CheckersGame.CheckersOptionId);
            var game = new CheckersBrain(CheckersGame.CheckersOption!, null);
            CheckersGameState checkersGameState = new CheckersGameState();
            if (CheckersGame.CheckersOption!.BlackStarts == false)
            {
                checkersGameState.NextMoveByBlack = false;
            }
            
            checkersGameState.SerializedGameState = game.GetSerializedGameState();
            checkersGameState.MoveId = 0;
            checkersGameState.CheckersGameId = _gameRepo.GetGame(CheckersGame.Name).Id;
            _stateRepo.SaveGameState(CheckersGame.Name, 0, checkersGameState);
            return RedirectToPage("./LaunchGame", new {id = _gameRepo.GetGame(CheckersGame.Name).Id});
            //return RedirectToRoute("./LaunchGame", new { id = _gameRepo.GetGame(CheckersGame.Name).Id });

        }
    }
}
