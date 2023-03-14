using Domain;

namespace DAL.Db;

public class GameOptionsRepositoryDb : BaseRepositoryDb, IGameOptionsRepository
{
    
    public GameOptionsRepositoryDb(AppDbContext dbContext) : base(dbContext)
    {
    }

    public List<string> GetGameOptionsListOfNames()
    {
        return Ctx
            .CheckersOptions
            .OrderBy(o => o.Name)
            .Select(o => o.Name)
            .ToList();
    }

    public CheckersOption GetGameOption(string optionName)
    {
        return Ctx.CheckersOptions.First(o => o.Name == optionName);
    }

    public CheckersOption GetGameOptionById(int optionId)
    {
        return Ctx.CheckersOptions.First(o => o.Id == optionId);
    }

    public void SaveGameOption(string optionName, CheckersOption option)
    {
        var optionFromDb = Ctx.CheckersOptions.FirstOrDefault(o => o.Name == optionName);
        if (optionFromDb == null)
        {
            Ctx.CheckersOptions.Add(option);
            Ctx.SaveChanges();
            return;
        }

        optionFromDb.Name = option.Name;
        optionFromDb.Height = option.Height;
        optionFromDb.Width = option.Width;
        optionFromDb.RowsOfPieces = option.RowsOfPieces;
        optionFromDb.FlyingKings = option.FlyingKings;
        optionFromDb.BlackStarts = option.BlackStarts;
        optionFromDb.CapturingIsMandatory = option.CapturingIsMandatory;
        optionFromDb.PiecesCapturingForwardAndBackward = option.PiecesCapturingForwardAndBackward;

        Ctx.SaveChanges();


    }

    public void DeleteGameOption(string optionName)
    {
        var optionFromDb = GetGameOption(optionName);
        Ctx.CheckersOptions.Remove(optionFromDb);
        Ctx.SaveChanges();
    }


}