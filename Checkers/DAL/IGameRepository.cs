using Domain;

namespace DAL;

public interface IGameRepository : IBaseRepository
{
    List<CheckersGame> GetAllGames(string? filter);
    CheckersGame? GetAllAboutGame(int? id);
    List<string> GetGamesListOfNames();
    CheckersGame GetGame(string checkersGameName);

    CheckersOption GetSavedOptionsForGame(string checkersGameName);

    CheckersGame SaveGame(string checkersGameName, CheckersGame game, CheckersOption currentOption);

    void DeleteGame(string checkersGameName);
}