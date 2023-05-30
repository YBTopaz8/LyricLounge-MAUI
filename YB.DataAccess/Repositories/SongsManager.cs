
using Realms;
using System.Diagnostics;
using YB.DataAccess.IRepositories;
using YB.DataAccess.Platforms.IRepositories;
using YB.Models;

namespace YB.DataAccess.Repositories;

public class SongsManager : ISongsManager
{
#if ANDROID
    string DB_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YBMusics.realm");
#elif WINDOWS
    string DB_PATH = "random";
#else 
    string DB_PATH = "random";
#endif
    Realm db;

    List<SongModel> SongsList;

    static IDataAccessRepo repo;

    static ISongsManager currentImplementation;
    public static ISongsManager Current => currentImplementation ??= new SongsManager(repo);
    public SongsManager(IDataAccessRepo dataAccess)
    {
        repo = dataAccess;
    }

    void OpenDB()
    {
        var config = new RealmConfiguration(DB_PATH)
        {
            ShouldDeleteIfMigrationNeeded = true, // never use in prod
            SchemaVersion = 1,
        };
        db = Realm.GetInstance(config);
    }

    public List<SongModel> GetSongs()
    {
        OpenDB();

        var ss = db.All<SongModel>().ToList();
        return ss;
    }
    public Task<bool> AddListOfSongsAsync(List<SongModel> song)
    {
        throw new NotImplementedException();
    }
    public async Task<bool> AddSong(SongModel song)
    {
        try
        {
            OpenDB();
            var exisingSong = db.All<SongModel>().FirstOrDefault(s => s.FilePath == song.FilePath);
            if (exisingSong is null)
            {
                var s = await db.WriteAsync(() => db.Add(song));

                return s is not null;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            //write the exception message in debug 
            Debug.WriteLine("Error in adding song to the database " + ex.Message);
            return false;
        }
    }

    public SongModel GetSongById(string id)
    {
        OpenDB();
        SongModel song = db.All<SongModel>().FirstOrDefault(s => s.Id == id);
        return song;
    }

    void OpenAndDeleteDB()
    {
        string path = DB_PATH;

        var config = new RealmConfiguration(path);
        Realm.DeleteRealm(config);
    }
    public void DropCollection()
    {
        //OpenAndDeleteDB();
        db.Write(() => db.RemoveAll<SongModel>());
    }
}