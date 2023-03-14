namespace DAL.Db;

public abstract class BaseRepositoryDb : IBaseRepository
{
    protected readonly AppDbContext Ctx;

    protected BaseRepositoryDb(AppDbContext dbContext)
    {
        Ctx = dbContext;
    }

    public string Name { get; set; } = "DATABASE";
    public void SaveChanges()
    {
        Ctx.SaveChanges();
    }
}