using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.CheckersGames;

public class LaunchGame : PageModel
{
    
    private readonly IGameRepository _gameRepo;
    
    private readonly IGameOptionsRepository _optionsRepo;
    
    private readonly IGameStateRepository _stateRepo;
    public LaunchGame(IGameRepository gameRepo,IGameOptionsRepository optionsRepo, IGameStateRepository stateRepo)
    {
        _gameRepo = gameRepo;
        _optionsRepo = optionsRepo;
        _stateRepo = stateRepo;
        Console.WriteLine(gameRepo);
        Console.WriteLine(optionsRepo);
        Console.WriteLine(stateRepo);
    }
    
    public int GameId { get; set; }
    

    public IActionResult OnGet(int? id)
    {
        if (id == null)
        {
            return RedirectToPage("/Index", new {error = "No id!"});
        }
        
        var game = _gameRepo.GetAllAboutGame(id);
        
        if (game == null)
        {
            return RedirectToPage("/Index", new {error = "No game found!"});
        }
        
        // is it 2 player game

        if (game.Player1Type == EPlayerType.Human && game.Player2Type == EPlayerType.Human)
        {
            // create 2 links - tab1, tab2
            GameId = game.Id;
            return Page();
        }
        
        
        // its a single player (human vs ai, ai vs ai)
        // just redirect to play page
        
        //ai vs ai
        //human vs ai
        int res = 1;
        
        
        //ai vs human
        if (game.Player2Type == EPlayerType.Human)
        {
            res = 2;
        }
        
        
        return RedirectToPage("Play", new { id = game.Id, playerNo = res });
    }
}