using Realms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YB.DataAccess.IRepositories;
using YB.DataAccess.Platforms.IRepositories;
using YB.Models;

namespace YB.DataAccess.Repositories;

public class PlaylistManager : IPlaylistManager
{
#if ANDROID
    string DB_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YBMusics.realm");
#elif WINDOWS
    string DB_PATH = "random";
#else
    string DB_PATH = "random";
#endif

    Realm db;

    List<string> playlistNames;
    static IDataAccessRepo repo;
    static IPlaylistManager currentImplementation;
    public static IPlaylistManager Current => currentImplementation ??= new PlaylistManager(repo);

    public PlaylistManager(IDataAccessRepo dataAccessRepo)
    {
        repo = dataAccessRepo;
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

    public async Task<bool> CreateNewPlaylist(PlaylistModel playlist)
    {
        try
        {
            OpenDB();
            var existingPlayList = db.All<PlaylistModel>().FirstOrDefault(pl => pl.Name == playlist.Name);
            if (existingPlayList is null)
            {
                var pl = db.Write(() => db.Add(playlist));
                return pl is not null;
            }
            else if(existingPlayList.Songs.Count == 0)
            {
                using var transaction = await db.BeginWriteAsync();
                foreach (var song in playlist.Songs)
                {
                    existingPlayList.Songs.Add(song);
                }
                transaction.Commit();
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            //write the exception message in debug window
            Debug.WriteLine(ex.Message);
            return false;
        }
    }

    public bool AddSongToPlaylist(string playlistName, SongModel songToAdd)
    {
        try
        {
            OpenDB();
            var existingPlayList = db.All<PlaylistModel>().FirstOrDefault(pl => pl.Name == playlistName);
            if (existingPlayList is null)
            {
                if (existingPlayList.Songs.Any(item => item.FilePath == songToAdd.FilePath))
                {
                    return false;
                }
                else
                {
                    using var transaction = db.BeginWrite();
                    existingPlayList.Songs.Add(songToAdd);
                    transaction.Commit();
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error couldn't add song to playlist in db: {ex.Message}");
            return false;
        }
    }

    public bool DeletePlaylist(string playlistName)
    {
        throw new NotImplementedException();
    }

    public bool EditPlaylist(string playlistName, PlaylistModel playlistToEdit)
    {
        OpenDB();
        var playlist = db.All<PlaylistModel>().FirstOrDefault(pl => pl.Name == playlistName);

        if (playlist != null)
        {
            using var transaction = db.BeginWrite();

            playlist.Name = playlistToEdit.Name;

            transaction.Commit();

            return true;
        }
        return false;
    }

    public PlaylistModel GetPlayList(string PlaylistName)
    {
        try
        {
            OpenDB();
            var playlist = db.All<PlaylistModel>().FirstOrDefault(pl => pl.Name == PlaylistName);
            return playlist;
        }
        catch (Exception)
        {
            return Enumerable.Empty<PlaylistModel>().FirstOrDefault();
        }
    }

    public PlaylistModel GetDefaultPlayList()
    {
        try
        {
            OpenDB();
            return db.All<PlaylistModel>().FirstOrDefault(pl => pl.Name == "Default");
        }
        catch (Exception)
        {
            return Enumerable.Empty<PlaylistModel>().FirstOrDefault();
        }
    }

    public bool RemoveSongFromPlaylist(string playlistName, SongModel songToRemove)
    {
        try
        {
            OpenDB();
            var playlist = db.All<PlaylistModel>().FirstOrDefault(pl => pl.Name == playlistName);

            if (playlist != null)
            {
                var song = playlist.Songs.FirstOrDefault(s => s.FilePath == songToRemove.FilePath);

                if (song != null)
                {
                    using var transaction = db.BeginWrite();

                    playlist.Songs.Remove(song);
                    transaction.Commit();

                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to delete song from playlist " + ex.Message);
            return false;
        }
    }

    public List<PlaylistModel> GetAllPlaylists()
    {
        OpenDB();
        var playlists = db.All<PlaylistModel>().ToList();
        return playlists ?? Enumerable.Empty<PlaylistModel>().ToList();
    }
}