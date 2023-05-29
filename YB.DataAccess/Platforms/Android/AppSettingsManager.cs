
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
    IPlaylistManager playlistManager;

    bool IsFirstRun;

    public AppSettingsManager(IDataAccessRepo dataAccessRepo, ISongsManager songsManager, IPlaylistManager plManager)
    {
        repo = dataAccessRepo;
        songManager = songsManager;
        playlistManager = plManager;
    }

    public void DropCollection()
    {
        songManager.DropCollection();
    }

    public void ScanSongs()
    {
        MusicScanner musicScanner = new(songManager, playlistManager);
        musicScanner.ScanFolder();
    }

    public void ScanSongs(List<string> PathsToFolders = null)
    {
        MusicScanner musicScanner = new(songManager, playlistManager);
        musicScanner.ScanFolder(PathsToFolders);
    }
}
//TODO write a method to check if the app is running for the first time

class MusicScanner
{
    ISongsManager songManager;
    IPlaylistManager playlistManager;
    const string MUSIC_FOLDER_PATH = "/storage/emulated/0/Music/";
    private readonly List<SongModel> _songs;

    //constructor
    public MusicScanner(ISongsManager songsRepo, IPlaylistManager plManager)
    {
        songManager = songsRepo;
        _songs = new List<SongModel>();
        playlistManager = plManager;
    }

    public void ScanFolder(List<string> ListofFolders = null)
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
        List<SongModel> listOfSongs = new();
        ScanSongsAndFillList(folder);
        if (AddSongsToDB(out listOfSongs))
        {
            PlaylistModel defaultPlaylist = new()
            {
                Name = "Default",
                CreationDate = DateTimeOffset.Now,
                Id = Guid.NewGuid().ToString(),
            };
            listOfSongs.ForEach(song => defaultPlaylist.Songs.Add(song));
            playlistManager.CreateNewPlaylist(defaultPlaylist);
        }
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

    private bool AddSongsToDB(out List<SongModel> listOfDefaultSongs)
    {
        try
        {
            for (int i = 0; i < _songs.Count; i++)
            {
                songManager.AddSong(_songs[i]);
            }
            listOfDefaultSongs = _songs;
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            listOfDefaultSongs = Enumerable.Empty<SongModel>().ToList();
            return false;
        }
    }
}