using Domain;

namespace DAL;

public interface IGameStateRepository : IBaseRepository
{
    CheckersGameState GetGameState(string checkersGameName, int moveId);

    CheckersGameState GetLastGameState(string checkersGameName);

    List<CheckersGameState> GetAllGameStates(string checkersGameName);

    void SaveGameState(string checkersGameName, int moveId, CheckersGameState state);

    void DeleteGameState(string checkersGameName, int moveId);

    void DeleteAllGameStates(string checkersGameName);

}