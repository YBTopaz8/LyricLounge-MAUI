using Realms;
using YB.DataAccess.Platforms.IRepositories;

namespace YB.DataAccess.Platforms;

public class DataAccessRepo : IDataAccessRepo
{
    Realm db;

    public async Task<Realm> GetDBAsync()
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YBMusics.realm");

        var config = new RealmConfiguration(path);

        db = await Realm.GetInstanceAsync(config);
        return db;
    }
}
