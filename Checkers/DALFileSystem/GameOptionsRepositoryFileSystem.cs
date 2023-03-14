using DAL;
using Domain;

namespace DALFileSystem;

public class GameOptionsRepositoryFileSystem : BaseRepositoryFileSystem, IGameOptionsRepository
{
    private const string FileExtension = "json";
    private readonly string _optionsDirectory = "." + Path.DirectorySeparatorChar + "options";
    

    public List<string> GetGameOptionsListOfNames()
    {
        CheckOrCreateDirectory();
        
        var res = new List<string>();
        
        foreach (var fileName in Directory.GetFileSystemEntries(_optionsDirectory,"*."+FileExtension))
        {
            res.Add(Path.GetFileNameWithoutExtension(fileName));
        }

        return res;
    }

    public CheckersOption GetGameOption(string optionName)
    {
        var fileContent = File.ReadAllText(GetFileName(optionName));
        var options = System.Text.Json.JsonSerializer.Deserialize<CheckersOption>(fileContent);
        if (options == null)
        {
            throw new NullReferenceException($"Could not deserialize: {fileContent}");
        }
        return options;
    }

    public CheckersOption GetGameOptionById(int optionId)
    {
        throw new NotImplementedException();
    }

    public void SaveGameOption(string optionName, CheckersOption option)
    {
        CheckOrCreateDirectory();
        var fileContent = System.Text.Json.JsonSerializer.Serialize(option);
        File.WriteAllText(GetFileName(optionName),fileContent);
    }

    public void DeleteGameOption(string optionName)
    {
        File.Delete(GetFileName(optionName));
    }

    private string GetFileName(string optionName)
    {
        return _optionsDirectory + Path.DirectorySeparatorChar + optionName + "." + FileExtension;
    }

    private void CheckOrCreateDirectory()
    {
        if (!Directory.Exists(_optionsDirectory))
        {
            Directory.CreateDirectory(_optionsDirectory);
        }
    }
    
}