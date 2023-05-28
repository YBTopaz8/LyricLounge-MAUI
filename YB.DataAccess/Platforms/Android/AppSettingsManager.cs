
using System.Diagnostics;
using YB.DataAccess.IRepositories;
using YB.DataAccess.Platforms.IRepositories;
using YB.Models;
using ATL.AudioData;
using ATL;

namespace YB.DataAccess.Platforms;

public class AppSettingsManager : IAppSettingsManager
{
    IDataAccessRepo repo;
    ISongsManager songManager;
    bool IsFirstRun;

    public AppSettingsManager(IDataAccessRepo dataAccessRepo, ISongsManager songsManager)
    {
        repo = dataAccessRepo;
        songManager = songsManager;
    }

    public void DropCollectionAsync()
    {
         songManager.DropCollection();
    }

    public void ScanSongs()
    {
        MusicScanner musicScanner = new (songManager);
        musicScanner.ScanFolderAsync();
    }

    public void ScanSongs (List<string> PathsToFolders = null)
    {
        MusicScanner musicScanner = new(songManager);
        musicScanner.ScanFolderAsync(PathsToFolders);
    }
}
//TODO write a method to check if the app is running for the first time

class MusicScanner
{
    ISongsManager songManager;

    const string MUSIC_FOLDER_PATH = "/storage/emulated/0/Music/";
    private readonly List<SongModel> _songs;
    public MusicScanner(ISongsManager songsRepo)
    {
        songManager = songsRepo;
        _songs = new List<SongModel>();
    }

    public void ScanFolderAsync( List<string> ListofFolders = null )
    {
        if (ListofFolders is null)
        {
             ScanDirectory(MUSIC_FOLDER_PATH);
        }
        else
        {
            foreach (string folder in ListofFolders)
            {
                 ScanDirectory(folder);
            }
        }
    }
    void ScanDirectory(string folder)
    {
        ScanSongsAndFillList(folder);
        AddSongsToDB();
    }

    private void ScanSongsAndFillList(string folder)
    {
        try
        {
            string[] files = Directory.GetFiles(folder);
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                if (file.EndsWith(".mp3") || file.EndsWith(".flac"))
                {
                    Track track = new(file);

                    if (track.Duration >= 29)
                    {
                        SongModel song = new()
                        {
                            Title = track.Title,
                            BitRate = track.Bitrate,
                            DateAdded = DateTime.Now,
                            DurationInMilliseconds = track.DurationMs,
                            TrackNumber = track.TrackNumber,
                            ReleaseYear = track.Year,
                            FilePath = track.Path,
                            FileFormat = Path.GetExtension(file).TrimStart('.'),

                            //FileFormat = track,
                        };

                        song.BitRate = track.Bitrate;

                        song.Artist = (track.Artist is not null) ? new() { Name = track.Artist } : null;
                        song.Album = track.Album is not null ? new() { Name = track.Album, ReleaseYear = track.Year, Artist = song.Artist } : null;
                        _songs.Add(song);
                    }
                }
            }

            string[] subFolders = Directory.GetDirectories(folder);
            foreach (string subFolder in subFolders)
            {
                if (subFolder != "/storage/emulated/0/Music/.thumbnails" && subFolder is not null)
                {
                    ScanSongsAndFillList(subFolder);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            throw;
        }
    }

    private void AddSongsToDB()
    {
        for (int i = 0; i < _songs.Count; i++)
        {
            songManager.AddSong(_songs[i]);
        }
    }
}