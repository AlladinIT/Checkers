using DAL;
using Domain;

namespace DALFileSystem;

public class GameStateRepositoryFileSystem : BaseRepositoryFileSystem, IGameStateRepository
{
    private const string FileExtension = "json";
    private readonly string _gameDirectory = "." + Path.DirectorySeparatorChar + "games";
    

    public CheckersGameState GetGameState(string checkersGameName, int moveId)
    {
        CheckOrCreateGamesDirectory();
        CheckOrCreateGameDirectoryForDifferentGames(checkersGameName);
        
        var fileContent = File.ReadAllText(_gameDirectory + Path.DirectorySeparatorChar + 
                                           checkersGameName + Path.DirectorySeparatorChar +
                                           moveId + "." +FileExtension);
        var res = System.Text.Json.JsonSerializer.Deserialize<CheckersGameState>(fileContent);
        if (res == null)
        {
            throw new NullReferenceException($"Could not deserialize: {fileContent}");
        }
        return res;
    }

    public CheckersGameState GetLastGameState(string checkersGameName)
    {
        CheckOrCreateGamesDirectory();
        CheckOrCreateGameDirectoryForDifferentGames(checkersGameName);

        List<CheckersGameState> allGameStatesGameStates= GetAllGameStates(checkersGameName);
        List<int> moveIds = new List<int>();
        foreach (var state in allGameStatesGameStates)
        {
            moveIds.Add(state.MoveId);
        }

        var lastMoveId = moveIds.Max();
        var fileContent = File.ReadAllText(_gameDirectory + Path.DirectorySeparatorChar +
                          checkersGameName + Path.DirectorySeparatorChar +
                          lastMoveId + "." + FileExtension);
        var res = System.Text.Json.JsonSerializer.Deserialize<CheckersGameState>(fileContent);
        if (res == null)
        {
            throw new NullReferenceException($"Could not deserialize: {fileContent}");
        }
        return res;
    }

    public List<CheckersGameState> GetAllGameStates(string checkersGameName)
    {
        CheckOrCreateGamesDirectory();
        CheckOrCreateGameDirectoryForDifferentGames(checkersGameName);
        
        var filenamesList = new List<string>();
        var path = _gameDirectory + Path.DirectorySeparatorChar +
                   checkersGameName + Path.DirectorySeparatorChar;

        var filenames = Directory.EnumerateFiles(path);
        
        foreach (var filename in filenames)
        {
            filenamesList.Add(Path.GetFileNameWithoutExtension(filename));
        }

        var listOfMoveIds = new List<int>();
        foreach (var filename in filenamesList)
        {
            var isNumeric = int.TryParse(filename, out int moveId);
            if (isNumeric)
            {
                listOfMoveIds.Add(moveId);
            }
        }

        List<CheckersGameState> allGameStates = new List<CheckersGameState>();
        foreach (var moveId in listOfMoveIds)
        {
            allGameStates.Add(GetGameState(checkersGameName, moveId));
        }
        
        return allGameStates;
    }

    public void SaveGameState(string checkersGameName, int moveId, CheckersGameState state)
    {
        CheckOrCreateGamesDirectory();
        CheckOrCreateGameDirectoryForDifferentGames(checkersGameName);
        
        var path = _gameDirectory + Path.DirectorySeparatorChar +
                   checkersGameName + Path.DirectorySeparatorChar +
                   moveId + "." + FileExtension;

        var fileContent = System.Text.Json.JsonSerializer.Serialize(state);
        File.WriteAllText(path,fileContent);
    }

    public void DeleteGameState(string checkersGameName, int moveId)
    {
        File.Delete(_gameDirectory + Path.DirectorySeparatorChar +
                    checkersGameName + Path.DirectorySeparatorChar +
                    moveId + "." + FileExtension);
    }

    public void DeleteAllGameStates(string checkersGameName)
    {
        List<CheckersGameState> allGameStates= GetAllGameStates(checkersGameName);
        foreach (var gameState in allGameStates)
        {
            File.Delete(_gameDirectory + Path.DirectorySeparatorChar +
                        checkersGameName + Path.DirectorySeparatorChar +
                        gameState.MoveId + "." + FileExtension);
        }
    }


    private void CheckOrCreateGamesDirectory()
    {
        if (!Directory.Exists(_gameDirectory))
        {
            Directory.CreateDirectory(_gameDirectory);
        }
    }
    
    private void CheckOrCreateGameDirectoryForDifferentGames(string checkersGameName)
    {
        if (!Directory.Exists(_gameDirectory + Path.DirectorySeparatorChar + checkersGameName))
        {
            Directory.CreateDirectory(_gameDirectory + Path.DirectorySeparatorChar + checkersGameName);
        }
    }
}






/*using DAL;
using Domain;

namespace DALFileSystem;

public class GameStateRepositoryFileSystem : IGameStateRepository
{
    private const string FileExtension = "json";
    private readonly string _gameDirectory = "." + Path.DirectorySeparatorChar + "games";


    public List<CheckersGameState> GetAllGameStates(string checkersGameName)
    {
        List<string>serializedListOfStates = File.ReadAllLines(_gameDirectory + Path.DirectorySeparatorChar + 
                                           checkersGameName + Path.DirectorySeparatorChar +
                                           "states" + "." +FileExtension).ToList();
        List<CheckersGameState> res = new List<CheckersGameState>();
        foreach (var serializedState in serializedListOfStates)
        {
            var state = System.Text.Json.JsonSerializer
                .Deserialize<CheckersGameState>(serializedState);
            if (state == null)
            {
                throw new NullReferenceException($"Could not deserialize: {serializedState}");
            }
            res.Add(state);
        }
        return res;
    }

    public CheckersGameState GetGameState(string checkersGameName, int moveId)
    {
        CheckOrCreateGamesDirectory();
        CheckOrCreateGameDirectoryForDifferentGames(checkersGameName);
        
        var fileContent = File.ReadLines(_gameDirectory + Path.DirectorySeparatorChar + 
                                           checkersGameName + Path.DirectorySeparatorChar +
                                           "states" + "." +FileExtension)
            .Skip(moveId-1).Take(moveId).First();
        
        var res = System.Text.Json.JsonSerializer.Deserialize<CheckersGameState>(fileContent);
        if (res == null)
        {
            throw new NullReferenceException($"Could not deserialize: {fileContent}");
        }
        return res;
    }

    public CheckersGameState GetLastGameState(string checkersGameName)
    {
        CheckOrCreateGamesDirectory();
        CheckOrCreateGameDirectoryForDifferentGames(checkersGameName);
        
        var fileContent = File.ReadLines(_gameDirectory + Path.DirectorySeparatorChar + 
                                           checkersGameName + Path.DirectorySeparatorChar +
                                           "states" + "." +FileExtension).Last();
        var res = System.Text.Json.JsonSerializer.Deserialize<CheckersGameState>(fileContent);
        if (res == null)
        {
            throw new NullReferenceException($"Could not deserialize: {fileContent}");
        }
        return res;
    }

    public void SaveGameState(string checkersGameName, int moveId, CheckersGameState state)
    {
        CheckOrCreateGamesDirectory();
        CheckOrCreateGameDirectoryForDifferentGames(checkersGameName);
        
        var path = _gameDirectory + Path.DirectorySeparatorChar +
                   checkersGameName + Path.DirectorySeparatorChar +
                   "states" + "." + FileExtension;

        var fileContent = System.Text.Json.JsonSerializer.Serialize(state);
        File.AppendAllText(path,fileContent + Environment.NewLine);
    }

    public void DeleteGameState(string checkersGameName, int moveId)
    {
        File.Delete(_gameDirectory + Path.DirectorySeparatorChar +
                    checkersGameName + Path.DirectorySeparatorChar +
                    moveId + "." + FileExtension);


        List<string> listOfStates = File.ReadAllLines(_gameDirectory + Path.DirectorySeparatorChar +
                                                      checkersGameName + Path.DirectorySeparatorChar +
                                                      moveId + "." + FileExtension).ToList();
        
        listOfStates.RemoveAt(moveId);
        File.WriteAllLines(_gameDirectory + Path.DirectorySeparatorChar +
                           checkersGameName + Path.DirectorySeparatorChar +
                           moveId + "." + FileExtension, listOfStates.ToArray());

    }
    


    private void CheckOrCreateGamesDirectory()
    {
        if (!Directory.Exists(_gameDirectory))
        {
            Directory.CreateDirectory(_gameDirectory);
        }
    }
    
    private void CheckOrCreateGameDirectoryForDifferentGames(string checkersGameName)
    {
        if (!Directory.Exists(_gameDirectory + Path.DirectorySeparatorChar + checkersGameName))
        {
            Directory.CreateDirectory(_gameDirectory + Path.DirectorySeparatorChar + checkersGameName);
        }
    }
}*/