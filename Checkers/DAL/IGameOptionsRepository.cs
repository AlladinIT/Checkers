using Domain;

namespace DAL;

public interface IGameOptionsRepository : IBaseRepository
{
    //crud methods
    
    //read
    List<string> GetGameOptionsListOfNames();
    CheckersOption GetGameOption(string optionName);

    CheckersOption GetGameOptionById(int optionId);
    //create and update
    void SaveGameOption(string optionName, CheckersOption option);
    
    //delete
    void DeleteGameOption(string optionName);
}