
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
    async void ScanDirectory(string folder)
    {
        List<SongModel> listOfSongs = new();
        await ScanSongsAndFillList(folder);
        if (AddSongsToDB(out listOfSongs))
        {
            PlaylistModel defaultPlaylist = new()
            {
                Name = "Default",
                CreationDate = DateTimeOffset.Now,
                Id = Guid.NewGuid().ToString(),
            };
            listOfSongs.ForEach(song => defaultPlaylist.Songs.Add(song));
           await playlistManager.CreateNewPlaylist(defaultPlaylist);
        }
    }

    private async Task ScanSongsAndFillList(string folder)
    {
        await Task.Run(async () =>
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

                        if (track.Duration > 29)
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
                                SampleRate = track.SampleRate,

                            };

                            song.BitRate = track.Bitrate;

                            song.Picture = track.EmbeddedPictures?[0].PictureData;

                            if (track.Lyrics.SynchronizedLyrics is not null)
                            {
                                track.Lyrics.SynchronizedLyrics.ToList().ForEach(lyric => song.SynchronizedLyrics.Add( new LyricPhraseModel { Text = lyric.Text, TimestampMs = lyric.TimestampMs }));
                                //track.Lyrics.SynchronizedLyrics.ToList().ForEach(lyric => song.Lyrics.Add(lyric.ToString()));
                            }
                            else if(track.Lyrics.UnsynchronizedLyrics is not null)
                            {
                                song.UnSynchronizedLyrics = track.Lyrics.UnsynchronizedLyrics;
                            }

                            song.FileSize = new FileInfo(file).Length / 1024 / 1024 / 1024;

                            song.Artist = (track.Artist is not null) ? new() { Name = track.Artist } : null;
                            song.Album = track.Album is not null ? new() { Name = track.Album, ReleaseYear = track.Year, Artist = song.Artist } : null;
                            if (File.Exists(Path.ChangeExtension(file, ".lrc")))
                            {
                                song.HasLyrics = true;
                            }
                            _songs.Add(song);
                           // Debug.WriteLine(song.Title + " Added");
                        }
                    }
                }

                string[] subFolders = Directory.GetDirectories(folder);
                List<Task> tasks = new List<Task>();
                foreach (string subFolder in subFolders)
                {
                    if (subFolder != "/storage/emulated/0/Music/.thumbnails" && subFolder is not null)
                    {
                       tasks.Add( ScanSongsAndFillList(subFolder));
                    }

                }
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "Mark 1");
                //throw;
            }
        });
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
            Debug.WriteLine(ex.Message + "Mark 2");
            listOfDefaultSongs = Enumerable.Empty<SongModel>().ToList();
            return false;
        }
    }
}