using DAL;
using Domain;

namespace DALFileSystem;

public class GameRepositoryFileSystem : BaseRepositoryFileSystem, IGameRepository
{
    private const string FileExtension = "json";
    private readonly string _gameDirectory = "." + Path.DirectorySeparatorChar + "games";

    public List<CheckersGame> GetAllGames(string? filter)
    {
        throw new NotImplementedException();
    }

    public CheckersGame GetAllAboutGame(int? id)
    {
        throw new NotImplementedException();
    }

    public List<string> GetGamesListOfNames()
    {
        CheckOrCreateGamesDirectory();

        var path = _gameDirectory + Path.DirectorySeparatorChar;
        var array = Directory.GetDirectories(path, "*",SearchOption.AllDirectories);
        var listOfDirectoriesWithLongPaths = array.ToList();
        var res = new List<string>();
        foreach (var directory in listOfDirectoriesWithLongPaths)
        {
            var remove = directory.Remove(0, path.Length);
            res.Add(remove);
        }
        return res;
    }

    public CheckersGame GetGame(string checkersGameName)
    {
        CheckOrCreateGamesDirectory();
        CheckOrCreateGameDirectoryForDifferentGames(checkersGameName);
        
        var fileContent = File.ReadAllText(_gameDirectory + Path.DirectorySeparatorChar + 
                                           checkersGameName + Path.DirectorySeparatorChar +
                                           "game" + "." +FileExtension);
        var res = System.Text.Json.JsonSerializer.Deserialize<CheckersGame>(fileContent);
        if (res == null)
        {
            throw new NullReferenceException($"Could not deserialize: {fileContent}");
        }
        return res;
    }

    public CheckersOption GetSavedOptionsForGame(string checkersGameName)
    {
        CheckOrCreateGamesDirectory();
        CheckOrCreateGameDirectoryForDifferentGames(checkersGameName);
        
        var fileContent = File.ReadAllText(_gameDirectory + Path.DirectorySeparatorChar + 
                                           checkersGameName + Path.DirectorySeparatorChar +
                                           "options" + "." +FileExtension);
        var res = System.Text.Json.JsonSerializer.Deserialize<CheckersOption>(fileContent);
        if (res == null)
        {
            throw new NullReferenceException($"Could not deserialize: {fileContent}");
        }
        return res;
    }

    public CheckersGame SaveGame(string checkersGameName, CheckersGame game, CheckersOption currentOption)
    {
        CheckOrCreateGamesDirectory();
        CheckOrCreateGameDirectoryForDifferentGames(checkersGameName);
        
        var path1 = _gameDirectory + Path.DirectorySeparatorChar +
                   checkersGameName + Path.DirectorySeparatorChar +
                   "game" + "." + FileExtension;

        var fileContent1 = System.Text.Json.JsonSerializer.Serialize(game);
        File.WriteAllText(path1,fileContent1);
        
        var path2 = _gameDirectory + Path.DirectorySeparatorChar +
                    checkersGameName + Path.DirectorySeparatorChar +
                    "options" + "." + FileExtension;
        
        var fileContent2 = System.Text.Json.JsonSerializer.Serialize(currentOption);
        File.WriteAllText(path2,fileContent2);
        return game;
    }

    public void DeleteGame(string checkersGameName)
    {
        File.Delete(_gameDirectory + Path.DirectorySeparatorChar +
                    checkersGameName + Path.DirectorySeparatorChar +
                    "game" + "." + FileExtension);
        File.Delete(_gameDirectory + Path.DirectorySeparatorChar +
                    checkersGameName + Path.DirectorySeparatorChar +
                    "options" + "." + FileExtension);
        Directory.Delete(_gameDirectory + Path.DirectorySeparatorChar + checkersGameName);
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