using Realms;

namespace YB.DataAccess.Platforms.IRepositories;

public interface IDataAccessRepo
{
    Task<Realm> GetDBAsync();
}
