// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using ConsoleUI;
using DAL;
using DAL.Db;
using DALFileSystem;
using Domain;
using GameBrain;
using MenuSystem;
using Microsoft.EntityFrameworkCore;



CheckersBoard gameBoard = new CheckersBoard();



var defaultGameOptions = new CheckersOption();
CheckersOption currentGameOptions;
var newGameOptions = new CheckersOption();

IGameOptionsRepository optionsRepoFs = new GameOptionsRepositoryFileSystem();
IGameStateRepository stateRepoFs = new GameStateRepositoryFileSystem();
IGameRepository gameRepoFs = new GameRepositoryFileSystem();

var dbOptions =
    new DbContextOptionsBuilder<AppDbContext>()
        //.UseLoggerFactory(Helpers.MyLoggerFactory)
        .UseSqlite("Data Source=C:/Users/Aleks/db/checkers.db")
        .Options;
var ctx = new AppDbContext(dbOptions);
IGameOptionsRepository optionsRepoDb = new GameOptionsRepositoryDb(ctx);
IGameStateRepository stateRepoDb = new GameStateRepositoryDb(ctx);
IGameRepository gameRepoDb = new GameRepositoryDb(ctx);

IGameOptionsRepository optionsRepo = optionsRepoFs;
IGameStateRepository stateRepo = stateRepoFs;
IGameRepository gameRepo = gameRepoFs;

/*var thirdMenu = new Menu(
    "THIRD LEVEL",
    EMenuLevel.Other,
    new List<MenuItem>()
    {
        new MenuItem("Nothing", null),
    }
);*/

var secondMenu = new Menu(
    "OPTIONS",
    EMenuLevel.Second,
    new List<MenuItem>()
    {
        new MenuItem("Create options", CreateGameOption),
        new MenuItem("List saved options", ListGameOptions),
        new MenuItem("Edit options", EditGameOption), //Should it be even possible???
        new MenuItem("Delete options", DeleteGameOptions),
        new MenuItem("Swap implementation of storage type", SwapSavingImplementation)
    }
);

var mainMenu = new Menu(
    "MAIN MENU",
    EMenuLevel.Main,
    new List<MenuItem>()
    {
        new MenuItem( "New Game", NewGame),
        new MenuItem("Load Game", LoadGame),
        new MenuItem("Delete Game", DeleteGame),
        new MenuItem("Options", secondMenu.RunMenu)
    }
);


//Starting main menu 
mainMenu.RunMenu();


string NewGame()
{
    
    //================================= CHOOSING WHAT OPTIONS TO USE (currentGameOptions) ==============================
    Console.Clear();
    
    //Checking if "default options" are present by default
    defaultGameOptions.Name = "Default options";
    if (!optionsRepo.GetGameOptionsListOfNames().Contains("Default options"))
    {
        optionsRepo.SaveGameOption("Default options",defaultGameOptions);
    }

    //variable for user input validation result
    var inputIsOk = false;
    
    //If there is only one available options ("default options") => no need to ask from user what options to use
    if (optionsRepo.GetGameOptionsListOfNames().Count == 1 && optionsRepo.GetGameOptionsListOfNames().Contains("Default options"))
    {
        //Changed current options to the "default"
        currentGameOptions = defaultGameOptions;

    }
    
    //Otherwise we show all available options
    else
    {
        Console.WriteLine("List of saved game options: ");
        foreach (var name in optionsRepo.GetGameOptionsListOfNames())
        {
            Console.WriteLine(name);
        }
        Console.WriteLine();
        
        var optionsName = "";

        //Keep asking what options to load, until user asks for an existing one
        do
        {
            Console.WriteLine("Options name you want to load: ");
            var userInput = Console.ReadLine() ?? "";
            
            // User wrote /quit
            if (ReturnBackToMenu(userInput))
            {
                return "";
            }
            
            if (!optionsRepo.GetGameOptionsListOfNames().Contains(userInput))
            {
                Console.WriteLine($"No saved options with name: '{userInput}'");
            }
            else
            {
                inputIsOk = true;
                optionsName = userInput;
            }

        } while (inputIsOk == false);

        //Changed current options to the one that user chose
        currentGameOptions = optionsRepo.GetGameOption(optionsName);

    }
    //========================================================================================================

    
    //========================================CREATING NEW GAME================================================
    string newGameName;
    inputIsOk = false;
    //Keep asking "new game name", until user writes correct one (non-existing)
    do
    {
        Console.WriteLine("Enter new game name: ");
        newGameName = Console.ReadLine() ?? "";
        
        // User wrote /quit
        if (ReturnBackToMenu(newGameName))
        {
            return "";
        }
        
        if (gameRepo.GetGamesListOfNames().Contains(newGameName))
        {
            Console.WriteLine("Game with this name already exists");
        }
        else
        {
            inputIsOk = true;
        }
    } while (inputIsOk == false);

    string choice;
    inputIsOk = false;
    
    //Keep asking "type of game", until user writes correct number (1 - 3)
    do
    {
        Console.WriteLine("1) Player vs Player");
        Console.WriteLine("2) Player vs AI");
        Console.WriteLine("3) AI vs AI");
        Console.WriteLine("Choose type of game (choose number): ");
        choice = Console.ReadLine() ?? "";
        
        // User wrote /quit
        if (ReturnBackToMenu(choice))
        {
            return "";
        }
        
        switch (choice)
        {
            case "1":
            case "2":
            case "3":
                inputIsOk = true;
                break;
            default:
                Console.WriteLine("Wrong input");
                break;
        }
    } while (inputIsOk == false);


    inputIsOk = false;
    CheckersGame checkersGame = new CheckersGame();
    checkersGame.Player1Name = "AI";
    checkersGame.Player1Type = EPlayerType.Ai;
    checkersGame.Player2Name = "AI";
    checkersGame.Player2Type = EPlayerType.Ai;
    if (choice == "1" || choice == "2")
    {
        do
        {
            Console.WriteLine("Player 1 name: ");
            var player1Name = Console.ReadLine() ?? "";
            
            // User wrote /quit
            if (ReturnBackToMenu(player1Name))
            {
                return "";
            }
            
            if (player1Name.Length > 32)
            {
                Console.WriteLine("too long name");
            }
            else
            {
                checkersGame.Player1Name = player1Name;
                checkersGame.Player1Type = EPlayerType.Human;
                inputIsOk = true;
            }

        } while (inputIsOk == false);

        
    }

    if (choice == "1")
    {
        inputIsOk = false;
        do
        {
            Console.WriteLine("Player 2 name: ");
            var player2Name = Console.ReadLine() ?? "";
            
            // User wrote /quit
            if (ReturnBackToMenu(player2Name))
            {
                return "";
            }
            
            if (player2Name.Length > 32)
            {
                Console.WriteLine("too long name");
            }
            else
            {
                checkersGame.Player2Name = player2Name;
                checkersGame.Player2Type = EPlayerType.Human;
                inputIsOk = true;
            }

        } while (inputIsOk == false);
    }
    
    checkersGame.Name = newGameName;
    checkersGame.CheckersOptionId = optionsRepo.GetGameOption(currentGameOptions.Name).Id;

    //Saving newly created game
    gameRepo.SaveGame(newGameName,checkersGame, currentGameOptions);

    //Getting initial positions of pieces on the game board (jagged array)
    var brain = new CheckersBrain(currentGameOptions, null);
    gameBoard.GameBoard = brain.GetBoard();
    
    //Creating and saving first "game state"
    CheckersGameState checkersGameState = new CheckersGameState
    {
        NextMoveByBlack = currentGameOptions.BlackStarts,
        SerializedGameState = JsonSerializer.Serialize(gameBoard),
        MoveId = 0,
        CheckersGameId = gameRepo.GetGame(newGameName).Id
    };
    stateRepo.SaveGameState(newGameName, checkersGameState.MoveId, checkersGameState);

    //User wrote /quit
    if (WaitForMove(newGameName, currentGameOptions.Name) == false)
    {
        return "";
    }

    WaitForExit(); 
    
    return "";
}

string LoadGame()
{
    // =============================== LOOKING FOR SAVED GAMES =====================================
    Console.Clear();
    if (gameRepo.GetGamesListOfNames().Count < 1)
    {
        //Waiting to return to menu
        Console.WriteLine("You don't have saved games");
        WaitForExit();
        return "";
    }

    //listing all saved games
    Console.WriteLine("Saved games: ");
    foreach (var game in gameRepo.GetGamesListOfNames())
    {
        Console.WriteLine(game);
    }
    Console.WriteLine();

    var inputIsOk = false;
    string userInput;
    //Keep asking what user what game he wants to load, until he chooses existing one
    do
    {
        Console.WriteLine("Please write name of the game you want to load");
        userInput = Console.ReadLine() ?? "";
        
        // User wrote /quit
        if (ReturnBackToMenu(userInput))
        {
            return "";
        }
        
        if (!gameRepo.GetGamesListOfNames().Contains(userInput))
        {
            Console.WriteLine($"Game with name: {userInput} does not exists");
        }
        else
        {
            inputIsOk = true;
        }
    } while (inputIsOk == false);
    // ============================================================================================  
    
    //User wrote /quit
    if (WaitForMove(userInput,gameRepo.GetSavedOptionsForGame(userInput).Name) == false)
    {
        return "";
    }
    
    WaitForExit(); 
    
    return "";
}

string DeleteGame()
{
    // =============================== LOOKING FOR SAVED GAMES =====================================
    Console.Clear();
    if (gameRepo.GetGamesListOfNames().Count < 1)
    {
        //Waiting to return to menu
        Console.WriteLine("You don't have saved games");
        WaitForExit();
        return "";
    }

    //listing saved games
    Console.WriteLine("Saved games: ");
    foreach (var game in gameRepo.GetGamesListOfNames())
    {
        Console.WriteLine(game);
    }
    Console.WriteLine();
    
    var gameName = "";
    var inputIsOk = false;
    //Keep asking user what game to delete, until he chooses existing one
    do
    {
        Console.WriteLine("Please write name of the game you want to delete");
        var userInput = Console.ReadLine() ?? "";
        
        // User wrote /quit
        if (ReturnBackToMenu(userInput))
        {
            return "";
        }
        
        if (!gameRepo.GetGamesListOfNames().Contains(userInput))
        {
            Console.WriteLine($"Game with name: {userInput} does not exists");
        }
        else
        {
            gameName = userInput;
            inputIsOk = true;
        }
    } while (inputIsOk == false);
    
    //Firstly we delete all Game states for a particular game
    stateRepo.DeleteAllGameStates(gameName);
    //Secondly we delete the game itself 
    gameRepo.DeleteGame(gameName);
    
    Console.WriteLine($"Game with name: {gameName} was successfully deleted");
    
    //Waiting to return to menu
    WaitForExit();
    
    return "";

}


string CreateGameOption()
{
    //=====================================Choosing New Options name==========================================
    Console.Clear();
    var optionsName = "";
    var inputIsOk = false;
    do
    {
        Console.WriteLine("New options name: ");
        var userInput = Console.ReadLine() ?? "";
        
        // User wrote /quit
        if (ReturnBackToMenu(userInput))
        {
            return "";
        }
        
        if (userInput.Length is > 32 or < 1)
        {
            Console.WriteLine("Too short or long name, try again");
        }
        else if (optionsRepo.GetGameOptionsListOfNames().Contains(userInput))
        {
            Console.WriteLine($"Option name: '{userInput}' is already taken!");
        }
        else if (userInput == "Default options")
        {
            Console.WriteLine("You can't create Default options");
        }
        else
        {
            inputIsOk = true;
            optionsName = userInput;
        }

    } while (inputIsOk == false);
    newGameOptions.Name = optionsName;
    //======================================================================================================

    //User wrote /quit
    if (ValidateUsersNewChosenOptions() == false)
    {
        return "";
    }
    
    
    //Saving new options
    optionsRepo.SaveGameOption(optionsName,newGameOptions);
    Console.WriteLine("New options were successfully saved");
    
    //waiting for exit
    WaitForExit();
    return "";
}

string ListGameOptions()
{
    Console.Clear();
    if (!optionsRepo.GetGameOptionsListOfNames().Any())
    {
        Console.WriteLine("List of game options is empty");
    }
    else
    {
        Console.WriteLine("List of saved game options: ");
        foreach (var name in optionsRepo.GetGameOptionsListOfNames())
        {
            Console.WriteLine(name);
        }
       
    }
    WaitForExit();
    return "";
}

string EditGameOption()
{
    Console.Clear();
    if (!optionsRepo.GetGameOptionsListOfNames().Any())
    {
        Console.WriteLine("List of game options is empty");
        //waiting to return to menu
        WaitForExit();
        return "";
    }

    //Listing saved game options
    Console.WriteLine("List of saved game options: ");
    foreach (var name in optionsRepo.GetGameOptionsListOfNames())
    {
        Console.WriteLine(name);
    }
    Console.WriteLine();
    
    //keep asking what options to change, until user chooses existing one
    var optionsName = "";
    var inputIsOk = false;
    do
    {
        Console.WriteLine("Options name you want to change: ");
        var userInput = Console.ReadLine() ?? "";
        
        // User wrote /quit
        if (ReturnBackToMenu(userInput))
        {
            return "";
        }
            
        if (!optionsRepo.GetGameOptionsListOfNames().Contains(userInput))
        {
            Console.WriteLine($"No saved options with name: {userInput}");
        }
        else if (userInput == "Default options")
        {
            Console.WriteLine("You can't change Default options");
        }
        else
        {
            inputIsOk = true;
            optionsName = userInput;
        }

    } while (inputIsOk == false);

    newGameOptions.Name = optionsName;

    //User wrote /quit
    if (ValidateUsersNewChosenOptions() == false)
    {
        return "";
    }

    //Updating saved options
    optionsRepo.SaveGameOption(optionsName, newGameOptions);
    Console.WriteLine($"Options with name: '{optionsName}' were successfully changed");
    
    //waiting for return to menu
    WaitForExit();
    return "";
    
}

string DeleteGameOptions()
{
    Console.Clear();
    if (!optionsRepo.GetGameOptionsListOfNames().Any())
    {
        //waiting for return to menu
        Console.WriteLine("List of game options is empty");
        WaitForExit();
        return "";
    }

    Console.WriteLine("List of saved game options: ");
    foreach (var name in optionsRepo.GetGameOptionsListOfNames())
    {
        Console.WriteLine(name);
    }
    Console.WriteLine();
        
    var optionsName = "";
    var inputIsOk = false;
    //Repeating, until user chooses to delete EXISTING options
    do
    {
        Console.WriteLine("NB! ALL GAMES DEPENDANT ON THESE OPTIONS WILL ALSO BE DELETED!");
        Console.WriteLine("Options name you want to delete: ");
        var userInput = Console.ReadLine() ?? "";
        
        // User wrote /quit
        if (ReturnBackToMenu(userInput))
        {
            return "";
        }
            
        if (!optionsRepo.GetGameOptionsListOfNames().Contains(userInput))
        {
            Console.WriteLine($"No saved options with name: {userInput}");
        }
        else if (userInput == "Default options")
        {
            Console.WriteLine("You can't delete Default options");
        }
        else
        {
            inputIsOk = true;
            optionsName = userInput;
        }

    } while (inputIsOk == false);
    
    //deleting options
    optionsRepo.DeleteGameOption(optionsName);
    Console.WriteLine($"Options with name: {optionsName} was successfully deleted");
    
    //Waiting for  return to menu
    WaitForExit();
    return "";
}

string SwapSavingImplementation()
{
    Console.Clear();
    if (optionsRepo == optionsRepoDb)
    {
        optionsRepo = optionsRepoFs;
        gameRepo = gameRepoFs;
        stateRepo = stateRepoFs;
    }
    else
    {
        optionsRepo = optionsRepoDb;
        gameRepo = gameRepoDb;
        stateRepo = stateRepoDb;
    }
    Console.WriteLine("Saving Implementation was changed to " + optionsRepo.Name);
    
    //Waiting for return to menu
    WaitForExit();
    return "";
}

bool ValidateUsersNewChosenOptions()
{
    Console.WriteLine("Please enter new board dimensions");
    
    //New options board height
    var boardHeight = 8;
    var heightIsOk = false;
    do
    {
        Console.WriteLine("Enter board Height (Only integer and not less than 8): ");
        var userInput = Console.ReadLine() ?? "";
        
        // User wrote /quit
        if (ReturnBackToMenu(userInput))
        {
            return false;
        }
        
        bool isNumber = int.TryParse(userInput, out var numericValue);
        if (isNumber && numericValue >= 8)
        {
            heightIsOk = true;
            boardHeight = numericValue;
        }
        else
        {
            Console.WriteLine("Incorrect input, read carefully!");
        }
    } while (heightIsOk == false);
    newGameOptions.Height = boardHeight;
    
    
    //New options board width
    var boardWidth = 8;
    var widthIsOk = false;
    do
    {
        Console.WriteLine("Now enter board Width (Should be even and between 4 and 26): ");
        var userInput = Console.ReadLine() ?? "";
        
        // User wrote /quit
        if (ReturnBackToMenu(userInput))
        {
            return false;
        }
        
        bool isNumber = int.TryParse(userInput, out var numericValue);
        if (isNumber && numericValue is >= 4 and <= 26 && numericValue % 2 == 0)
        {
            widthIsOk = true;
            boardWidth = numericValue;
        }
        else
        {
            Console.WriteLine("Incorrect input, read carefully!");
        }
    } while (widthIsOk == false);
    newGameOptions.Width = boardWidth;

    
    //New options Rows of pieces
    var rowsOfPieces = 3;
    var rowsOfPiecesIsOk = false;
    do
    {
        Console.WriteLine("Now enter board Rows of pieces (Should be more than 1 and (boardHeight - 2 * RowsOfPieces) should be at least 2): ");
        var userInput = Console.ReadLine() ?? "";
        
        // User wrote /quit
        if (ReturnBackToMenu(userInput))
        {
            return false;
        }
        
        bool isNumber = int.TryParse(userInput, out var numericValue);
        if (isNumber && numericValue > 1 && (newGameOptions.Height - 2 * numericValue) >= 2)
        {
            rowsOfPiecesIsOk = true;
            rowsOfPieces = numericValue;
        }
        else
        {
            Console.WriteLine("Incorrect input, read carefully!");
        }
    } while (rowsOfPiecesIsOk == false);
    newGameOptions.RowsOfPieces = rowsOfPieces;
    
    
    //New options boolean "black starts"
    var userInputIsValid = false;
    do
    {
        Console.WriteLine("Player with blue pieces starts: Y/N");

        var userInput = Console.ReadLine() ?? "";
        
        // User wrote /quit
        if (ReturnBackToMenu(userInput))
        {
            return false;
        }
        
        userInput = userInput.ToUpper().Trim();

        if (userInput == "Y")
        {
            newGameOptions.BlackStarts = true;
            userInputIsValid = true;
        }
        else if(userInput == "N")
        {
            newGameOptions.BlackStarts = false;
            userInputIsValid = true;
        }
        else
        {
            Console.WriteLine("Invalid input");
        }
    } while (userInputIsValid == false);
    
    
    
    //New options boolean "capturing is mandatory"
    userInputIsValid = false;
    do
    {
        Console.WriteLine("Capturing as many opponent's pieces as possible is mandatory: Y/N");
        var userInput = Console.ReadLine() ?? "";
        
        // User wrote /quit
        if (ReturnBackToMenu(userInput))
        {
            return false;
        }
        
        userInput = userInput.ToUpper().Trim();
        
        if (userInput == "Y")
        {
            newGameOptions.CapturingIsMandatory = true;
            userInputIsValid = true;
        }
        else if(userInput == "N")
        {
            newGameOptions.CapturingIsMandatory = false;
            userInputIsValid = true;
        }
        else
        {
            Console.WriteLine("Invalid input");
        }
    } while (userInputIsValid == false);
    
    
    //New options boolean "Flying kings"
    userInputIsValid = false;
    do
    {
        Console.WriteLine("King can move diagonally any number of fields, forwards or backwards (Flying kings): Y/N");
        var userInput = Console.ReadLine() ?? "";
        
        // User wrote /quit
        if (ReturnBackToMenu(userInput))
        {
            return false;
        }
        
        userInput = userInput.ToUpper().Trim();
        
        if (userInput == "Y")
        {
            newGameOptions.FlyingKings = true;
            userInputIsValid = true;
        }
        else if(userInput == "N")
        {
            newGameOptions.FlyingKings = false;
            userInputIsValid = true;
        }
        else
        {
            Console.WriteLine("Invalid input");
        }
    } while (userInputIsValid == false);
    
    
    //New options boolean "Pieces can capture forward and backward:"
    userInputIsValid = false;
    do
    {
        Console.WriteLine("Ordinary pieces can capture forward and backward: Y/N");
        var userInput = Console.ReadLine() ?? "";
        
        // User wrote /quit
        if (ReturnBackToMenu(userInput))
        {
            return false;
        }
        
        userInput = userInput.ToUpper().Trim();
        
        if (userInput == "Y")
        {
            newGameOptions.PiecesCapturingForwardAndBackward = true;
            userInputIsValid = true;
        }
        else if(userInput == "N")
        {
            newGameOptions.PiecesCapturingForwardAndBackward = false;
            userInputIsValid = true;
        }
        else
        {
            Console.WriteLine("Invalid input");
        }
    } while (userInputIsValid == false);

    return true;
}


void WaitForExit()
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkRed;
    Console.WriteLine("Press 'q' to quit");
    Console.ResetColor();
    var doExit = false;
    ConsoleKey keyPressed;
    do
    {
        ConsoleKeyInfo keyInfo = Console.ReadKey(true);
        keyPressed = keyInfo.Key;
        if (keyPressed == ConsoleKey.Q)
        {
            doExit = true;
        }
    } while (doExit == false);
}

bool WaitForMove(string checkersGameName, string optionsName)
{
    //Retrieving saved game
    CheckersGame game = gameRepo.GetGame(checkersGameName);
    
    //Getting options for current game
    var options = optionsRepo.GetGameOption(optionsName);
    
    //getting previous game state for current game
    var state = stateRepo.GetLastGameState(checkersGameName);
    
    var brain = new CheckersBrain(options,state);
    var nextMoveByBlack = state.NextMoveByBlack;
    
    //Continue playing, until BRAIN GAME OVER returns 1 or 2
    for (;brain.GameOver(nextMoveByBlack,options) == 0;) 
    {
        //getting previous game state for current game
        state = stateRepo.GetLastGameState(checkersGameName);
        
        brain = new CheckersBrain(options,state);
        nextMoveByBlack = state.NextMoveByBlack;
        
        var moveId = state.MoveId;

        int x1;
        int y1;
        int x2;
        int y2;
        var firstClickIsValid = false;
        var secondClickIsValid = false;
        

        //PLAYER 1 = ALWAYS BLACK (BLUE)
        //PLAYER 2 = ALWAYS WHITE (RED)
        
        
        // IF AI MOVE => MAKE A MOVE AND CONTINUE
        if (state.NextMoveByBlack && game.Player1Type == EPlayerType.Ai ||
            !state.NextMoveByBlack && game.Player2Type == EPlayerType.Ai)
        {
            //Printing out, who should make next move
            ConsoleWriteNextMoveBy(nextMoveByBlack,game);
            
            //Drawing board with pieces
            UI.DrawGameBoard(brain.GetBoard(),brain,null,null,nextMoveByBlack,options);
            
            // AI MAKES A MOVE
            brain.MakeAiMove(nextMoveByBlack,options);

            //Saving new state
            stateRepo.SaveGameState(checkersGameName,moveId+1,new CheckersGameState()
            {
                SerializedGameState = brain.GetSerializedGameState(),
                NextMoveByBlack = !nextMoveByBlack,
                MoveId = moveId + 1,
                CheckersGameId = gameRepo.GetGame(checkersGameName).Id
            });

            //if "AI vs AI"
            if (game.Player1Type == EPlayerType.Ai && game.Player2Type == EPlayerType.Ai)
            {
                Console.WriteLine("Press any button to continue watching or 'q' to leave");
                ConsoleKey keyPressed;
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                keyPressed = keyInfo.Key;
                if (keyPressed == ConsoleKey.Q)
                {
                    return false;
                }
            }
            //if "Player vs AI"
            else
            {
                //Making a small pause for user to understand what happened :)
                Thread.Sleep(4000);
            }
            
        }
        
        // IF HUMAN MOVE => KEEP ASKING USER ABOUT "FIRS CLICK COORDINATES" AND "SECOND CLICK COORDINATES" UNTIL THEY ARE VALID
        else
        {
            do
            {
                //Printing out, who should make next move
                ConsoleWriteNextMoveBy(nextMoveByBlack,game);
                //Redraw the board after the move was made OR user inserted invalid "firstclick"
                UI.DrawGameBoard(brain.GetBoard(),brain,null,null,nextMoveByBlack,options);
                
                //Current player is HUMAN => asking for coordinates of "piece to move"
                Console.WriteLine("Write coordinates of the piece you want to move (2 characters: letter + number): ");
                var input = Console.ReadLine() ?? "";
                
                // User wrote /quit
                if (ReturnBackToMenu(input))
                {
                    return false;
                }
                
                //CHECKING IF INPUT (FIRST CLICK COORDINATES) WERE VALID "LETTER + NUMBER"
                if (input.Length >= 2)
                {
                    //Retrieving letter from input (future y1)
                    var letter1 = input[0];
                    //Retrieving number from input, but still string (future x1)
                    string number1 = input.Remove(0,1);
                    
                    //Transforming Letter to (y1)
                    y1 = char.ToUpper(letter1) - 65;

                    //Trying to parse number(string value) to integer 
                    if (Int32.TryParse(number1, out var numberOut1))
                    {
                        //Calculating from number our (x1)
                        x1 = options.Height - numberOut1;

                        //Checking if first click is valid
                        if (brain.FirstClickIsValid(x1,y1,nextMoveByBlack))
                        {
                            //Printing out, who should make next move
                            ConsoleWriteNextMoveBy(nextMoveByBlack,game);
                            
                            //NOW WE REDRAW OUR BOARD BUT NOW WE SEND (x1 and y1) AND THEREFORE DRAW POSSIBLE MOVES FOR CHOSEN PIECE
                            UI.DrawGameBoard(brain.GetBoard(),brain,x1,y1,nextMoveByBlack,options);
                            
                            //Current player is HUMAN => asking for coordinates of "second click"
                            Console.WriteLine("Write coordinates of the square you want to move your piece (2 characters: letter + number): ");
                            var input2 = Console.ReadLine() ?? "";
                            
                            // User wrote /quit
                            if (ReturnBackToMenu(input2))
                            {
                                return false;
                            }
                            
                            //CHECKING IF INPUT (SECOND CLICK COORDINATES) WERE VALID "LETTER + NUMBER"
                            if (input2.Length >= 2)
                            {
                                //Retrieving letter from input (future y2)
                                var letter2 = input2[0];
                                
                                //Retrieving number from input, but still string (future x2)
                                string number2 = input2.Remove(0,1);
                    
                                //Transforming Letter to (y2)
                                y2 = char.ToUpper(letter2) - 65;
                    
                                //Trying to parse number(string value) to integer 
                                if (Int32.TryParse(number2, out var numberOut2))
                                {
                                    //Calculating from number our (x2)
                                    x2 = options.Height - numberOut2;

                                    //NOW WE HAVE ALL NEEDED COORDINATES:
                                    //Checking if Move is possible
                                    if (brain.IsMovePossible(x1,y1,x2,y2,nextMoveByBlack,options,true))
                                    {
                                        //Making move
                                        brain.MakeMove(x1,y1,x2,y2,nextMoveByBlack);
                                        
                                        //After player made a move, Saving new game state
                                        stateRepo.SaveGameState(checkersGameName,moveId+1,new CheckersGameState()
                                        {
                                            SerializedGameState = brain.GetSerializedGameState(),
                                            NextMoveByBlack = !nextMoveByBlack,
                                            MoveId = moveId + 1,
                                            CheckersGameId = gameRepo.GetGame(checkersGameName).Id
                                        });
                                        
                                        //Move was made => We should finish this loop
                                        secondClickIsValid = true;
                                    }
                                }
                            }
                            //Move was made => We should finish this loop
                            firstClickIsValid = true;
                        }
                    }
                }
            } while (firstClickIsValid == false || secondClickIsValid ==false);
        }

        /*Console.Clear();
        //Printing out, who should make next move
        ConsoleWriteNextMoveBy(nextMoveByBlack,game);
        //Redraw the board after the move was made
        UI.DrawGameBoard(brain.GetBoard(),brain,null,null,nextMoveByBlack,options);*/
        
        //So the GameOver check gets updated "NextMoveByBlack"
        nextMoveByBlack = stateRepo.GetLastGameState(checkersGameName).NextMoveByBlack;

    }

    
    //if WE ARE HERE => SOMEBODY HAS LOST THE GAME
    // 1 - Blue wins
    // 2 - Red Wins


    Console.Clear();
    //Redraw the board with last state
    UI.DrawGameBoard(brain.GetBoard(),brain,null,null,nextMoveByBlack,options);
    
    var winner = "";
    if (brain.GameOver(nextMoveByBlack, options) == 1)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        winner = "BLUE";
    }
    if (brain.GameOver(nextMoveByBlack, options) == 2)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        winner = "RED";
    }
    Console.WriteLine();
    Console.WriteLine(winner + " WON THE GAME");
    Console.ResetColor();

    return true;
}

void ConsoleWriteNextMoveBy(bool nextMoveByBlack, CheckersGame game){
    string nextColour;
    string player;
    if (nextMoveByBlack)
    {
        nextColour = "BLUE";
        player = game.Player1Name;
        Console.ForegroundColor = ConsoleColor.Blue;
    }
    else
    {
        nextColour = "RED";
        player = game.Player2Name;
        Console.ForegroundColor = ConsoleColor.Red;
    }
    Console.Clear();
    Console.WriteLine("NEXT MOVE BY "+nextColour+ " ("+player+")");
    Console.ResetColor();
    Console.WriteLine();
}

bool ReturnBackToMenu(string userInput)
{
    //if user input wrote '/quit'   =>   need to return to menu
    bool result = userInput == "/quit";
    return result;
}