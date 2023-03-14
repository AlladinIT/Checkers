using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public class GameStateRepositoryDb : BaseRepositoryDb, IGameStateRepository
{
    public GameStateRepositoryDb(AppDbContext dbContext) : base(dbContext)
    {
    }
    public CheckersGameState GetGameState(string checkersGameName, int moveId)
    {
        return Ctx.CheckersGameStates
            .Where(state => state.MoveId == moveId)
            .Include(state => state.CheckersGame)
            .First(state => state.CheckersGame != null && state.CheckersGame.Name == checkersGameName);
    }

    public CheckersGameState GetLastGameState(string checkersGameName)
    {
        return Ctx.CheckersGameStates
            .Include(state => state.CheckersGame)
            .Where(state => state.CheckersGame != null && state.CheckersGame.Name == checkersGameName)
            .OrderByDescending(s => s.MoveId).First();
    }

    public List<CheckersGameState> GetAllGameStates(string checkersGameName)
    {
        return Ctx.CheckersGameStates
            .Include(state => state.CheckersGame)
            .Where(state => state.CheckersGame != null && state.CheckersGame.Name == checkersGameName).ToList();
    }

    public void SaveGameState(string checkersGameName, int moveId, CheckersGameState state)
    {
        var gameStateFromDb = Ctx.CheckersGameStates
            .Where(s => s.MoveId == moveId)
            .Include(s => s.CheckersGame)
            .FirstOrDefault(s => s.CheckersGame != null && s.CheckersGame.Name == checkersGameName);

        if (gameStateFromDb == null)
        {
            Ctx.CheckersGameStates.Add(state);
            Ctx.SaveChanges();
            return;
        }

        /*gameStateFromDb.CreatedAt = state.CreatedAt;
        gameStateFromDb.SerializedGameState = state.SerializedGameState;
        gameStateFromDb.NextMoveByBlack = state.NextMoveByBlack;
        gameStateFromDb.MoveId = state.MoveId;
        gameStateFromDb.CheckersGameId = state.CheckersGame!.Id;*/
        Ctx.SaveChanges();
    }

    public void DeleteGameState(string checkersGameName, int moveId)
    {
        var stateFromDb = GetGameState(checkersGameName, moveId);
        Ctx.Remove(stateFromDb);
        Ctx.SaveChanges();
    }

    public void DeleteAllGameStates(string checkersGameName)
    {
        var gameStates = GetAllGameStates(checkersGameName);

        foreach (var state in gameStates)
        {
            Ctx.Remove(state);
        }
        Ctx.SaveChanges();
    }


}