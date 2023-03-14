using DAL;
using Domain;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.CheckersGames;

public class Play : PageModel
{
    
    private readonly IGameRepository _gameRepo;
    
    private readonly IGameOptionsRepository _optionsRepo;
    
    private readonly IGameStateRepository _stateRepo;
    public Play(IGameRepository gameRepo,IGameOptionsRepository optionsRepo, IGameStateRepository stateRepo)
    {
        _gameRepo = gameRepo;
        _optionsRepo = optionsRepo;
        _stateRepo = stateRepo;
    }
    
    public CheckersBrain Brain { get; set; } = default!;

    public CheckersGame CheckersGame { get; set; } = default!;
    public bool NextMoveByBlack { get; set; }
    
    public int MoveId { get; set; }
    
    public int GameOverCheck { get; set; }

    public int PlayerNo { get; set; }
    
    public bool AiMove { get; set; }

    public int BlackPiecesLeft { get; set; }
    
    public int WhitePiecesLeft { get; set; }
    public bool Player1MadeAMove { get; set; } = true;
    public async Task<IActionResult> OnGet(int? id, int? playerNo,bool aiMove, int? x1, int? y1, int? x2, int? y2)
    {
        if (id == null)
        {
            return RedirectToPage("/Index", new { error = "No game id!" });
        }
        
        if (playerNo == null || playerNo.Value < 1 || playerNo.Value > 2)
        {
            return RedirectToPage("/Index", new { error = "No playerNo, or wrong playerNo!" });
        }

        PlayerNo = playerNo.Value;
        // playerNo 1 - first player (BLACK)
        // playerNo 2 - second player (RED)
        
        var game = _gameRepo.GetAllAboutGame(id);

        if (game == null || game.CheckersOption == null)
        {
            return NotFound();
        }

        CheckersGame = game;
        Brain = new CheckersBrain(game.CheckersOption, game.CheckersGameStates!.LastOrDefault());
        NextMoveByBlack = _stateRepo.GetLastGameState(game.Name).NextMoveByBlack;
        MoveId = _stateRepo.GetLastGameState(game.Name).MoveId;

        
        //IF A GAME WAS ALREADY FINISHED
        if (CheckersGame.GameOverAt.HasValue)
        {
            return Page();
        }

        //CHECK IF IT IS AI MOVE (USEFUL WHEN AI MOVES FIRST , meaning the controller was NOT called from javascript)
        if (NextMoveByBlack && CheckersGame.Player1Type == EPlayerType.Ai && !CheckersGame.GameOverAt.HasValue ||
            !NextMoveByBlack && CheckersGame.Player2Type == EPlayerType.Ai && !CheckersGame.GameOverAt.HasValue)
        {
            AiMove = true;
        }
        
        // User clicked on table cell
        if (x1 != null && y1 != null && x2 != null && y2 != null)
        {
            
            // Since we don't have user registration (playerNo is our only distinguishable parameter),
            // we can't allow OTHER player to move our pieces
            // and also it only can be real HUMAN
            if (playerNo == 1 && NextMoveByBlack && CheckersGame.Player1Type == EPlayerType.Human ||
                playerNo == 2 && !NextMoveByBlack && CheckersGame.Player2Type == EPlayerType.Human)
            {

                // coordinates ARE NOT EQUAL => SECOND CLICK
                if (x1 != x2 || y1 != y2)
                {

                    if (Brain.IsMovePossible(x1.Value, y1.Value, x2.Value, y2.Value, NextMoveByBlack,
                            CheckersGame.CheckersOption, true))
                    {
                        Brain.MakeMove(x1.Value, y1.Value, x2.Value, y2.Value, NextMoveByBlack);
                        game.CheckersGameStates!.Add(new CheckersGameState()
                        {
                            SerializedGameState = Brain.GetSerializedGameState(),
                            NextMoveByBlack = !NextMoveByBlack,
                            MoveId = MoveId + 1
                        });

                        _gameRepo.SaveChanges();
                        NextMoveByBlack =
                            _stateRepo.GetLastGameState(game.Name)
                                .NextMoveByBlack; //So the Play.cshtml gets updated data to print out info
                    }

                    //PERSON MADE A MOVE    AND     NOW WE CHECK IF OTHER PLAYER IS AI
                    if (CheckersGame.Player1Type == EPlayerType.Ai || CheckersGame.Player2Type == EPlayerType.Ai)
                    {
                        AiMove = true;
                    }

                }
                //coordinates ARE EQUAL => FIRST CLICK
                else if (x1 == x2 && y1 == y2)
                {
                    if (Brain.FirstClickIsValid(x1.Value, y1.Value, NextMoveByBlack))
                    {
                        var x1NotNull = (int)x1;
                        var y1NotNull = (int)y1;
                        
                        //We store coordinates of first click in TempData between 2 requests
                        TempData["firstCoordinates"] = new[] { x1NotNull, y1NotNull };
                    }

                }
            }
        }
                    
        // IT IS AI MOVE
        else if(AiMove)
        {
            // AI MAKES A MOVE
            Brain.MakeAiMove(NextMoveByBlack,CheckersGame.CheckersOption);

            game.CheckersGameStates!.Add(new CheckersGameState()
            {
                SerializedGameState = Brain.GetSerializedGameState(),
                NextMoveByBlack = !NextMoveByBlack,
                MoveId = MoveId + 1
            });

            _gameRepo.SaveChanges();
            NextMoveByBlack =
                _stateRepo.GetLastGameState(game.Name)
                    .NextMoveByBlack; //So the Play.cshtml gets updated data to print out info

            //AI MADE A MOVE    AND     NOW WE CHECK IF OTHER PLAYER IS HUMAN
            if (CheckersGame.Player1Type == EPlayerType.Human || CheckersGame.Player2Type == EPlayerType.Human)
            {
                AiMove = false;
            }

        }

        Player1MadeAMove = !_stateRepo.GetLastGameState(game.Name).NextMoveByBlack;

        // true - count "Black"
        // false - count "Red"
        BlackPiecesLeft = Brain.CountPiecesLeftOnBoard(true);
        WhitePiecesLeft = Brain.CountPiecesLeftOnBoard(false);
        
        GameOverCheck = Brain.GameOver(NextMoveByBlack, CheckersGame.CheckersOption);
        if (GameOverCheck == 1 || GameOverCheck == 2)
        {
            CheckersGame.GameOverAt = DateTime.Now;
            CheckersGame.GameWonByPlayer = GameOverCheck == 1 ? CheckersGame.Player1Name : CheckersGame.Player2Name;
            _gameRepo.SaveChanges();
        }

        return Page();
    }
}