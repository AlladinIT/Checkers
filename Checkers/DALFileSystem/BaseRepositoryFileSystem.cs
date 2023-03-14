using DAL;

namespace DALFileSystem;

public class BaseRepositoryFileSystem : IBaseRepository
{
    public string Name { get; set; } = "FILE SYSTEM";
    public void SaveChanges()
    {
        throw new NotImplementedException("File system is updated immediately!");
    }
}