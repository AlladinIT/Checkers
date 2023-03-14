using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public class GameRepositoryDb : BaseRepositoryDb, IGameRepository
{
    public GameRepositoryDb(AppDbContext dbContext) : base(dbContext)
    {
    }

    public List<CheckersGame> GetAllGames(string? filter)
    {
        if (filter == null)
        {
            return Ctx.CheckersGames
                .Include(c => c.CheckersOption)
                .OrderBy(c => c.StartedAt)
                .ToList();
        }
        return Ctx.CheckersGames
            .Include(c => c.CheckersOption)
            .Where(c => c.Name.ToUpper().Contains(filter) ||
                        c.Player1Name.ToUpper().Contains(filter) ||
                        c.Player2Name.ToUpper().Contains(filter) ||
                        c.CheckersOption!.Name.ToUpper().Contains(filter))
            .OrderBy(c => c.StartedAt)
            .ToList();
    }

    public CheckersGame? GetAllAboutGame(int? id)
    {
        return Ctx.CheckersGames
            .Include(g => g.CheckersOption)
            .Include(g => g.CheckersGameStates)
            .FirstOrDefault(g => g.Id == id);
    }

    public List<string> GetGamesListOfNames()
    {
        return Ctx.CheckersGames.OrderBy(game => game.Name).Select(game => game.Name).ToList();
    }

    public CheckersGame GetGame(string checkersGameName)
    {
        return Ctx.CheckersGames.First(game => game.Name == checkersGameName);
    }

    public CheckersOption GetSavedOptionsForGame(string checkersGameName)
    {
        var cg = Ctx.CheckersGames.First(g => g.Name == checkersGameName);

        return Ctx.CheckersOptions.First(o => o.Id == cg.CheckersOptionId);
    }

    public CheckersGame SaveGame(string checkersGameName, CheckersGame game, CheckersOption currentOption)
    {
        var gameFromDb = Ctx.CheckersGames.FirstOrDefault(g => g.Name == checkersGameName);
        if (gameFromDb == null)
        {
            Ctx.CheckersGames.Add(game);
            Ctx.SaveChanges();
            return game;
        }

        gameFromDb.Name = checkersGameName;
        gameFromDb.StartedAt = game.StartedAt;
        gameFromDb.Player1Name = game.Player1Name;
        gameFromDb.Player1Type = game.Player1Type;
        gameFromDb.Player2Name = game.Player2Name;
        gameFromDb.Player2Type = game.Player2Type;
        gameFromDb.GameOverAt = game.GameOverAt;
        gameFromDb.GameWonByPlayer = game.GameWonByPlayer;
        gameFromDb.CheckersOptionId = game.CheckersOptionId;

        Ctx.SaveChanges();
        return game;
    }

    public void DeleteGame(string checkersGameName)
    {
        var gameFromDb = GetGame(checkersGameName);
        Ctx.CheckersGames.Remove(gameFromDb);
        Ctx .SaveChanges();
    }


}