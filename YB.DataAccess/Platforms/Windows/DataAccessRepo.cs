
using Realms;
using YB.DataAccess.Platforms.IRepositories;

namespace YB.DataAccess.Platforms;

// All the code in this file is only included on Windows.
public class DataAccessRepo : IDataAccessRepo
{
    public Task<Realm> GetDBAsync()
    {
        throw new NotImplementedException();
    }
}