
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using YB.DataAccess.IRepositories;
using YB.Models;
using YB.Utilities;
using YB.Utilities.Messages;
using YBMusic.Views.Mobile;

namespace YBMusic.ViewModels;

public partial class MusicServiceVM : ObservableObject, IRecipient<RefreshSongsList>
{
    IAppSettingsManager appSettingsManager;
    IFolderPicker FolderPicker;
    ISongsManager SongManager;
    IAudioManager audioManager;
    IAudioPlayer AudioPlayer { get; set; }
    IDisposable LyricsSyncSubscription;

    FolderPickerResult result;
    WeakReferenceMessenger messenger;

    System.Timers.Timer _progressTimer;
    public event Action SongStartedPlaying;

    [ObservableProperty]
    List<LyricsModel> lyrics;

    public MusicServiceVM(IAppSettingsManager settingsManager, IFolderPicker Picker, ISongsManager songsManager, IAudioManager audio, WeakReferenceMessenger wrm, IPlaylistManager plManager)
    {
        appSettingsManager = settingsManager;
        FolderPicker = Picker;
        SongManager = songsManager;
        audioManager = audio;
        playlistManager = plManager;

        _progressTimer = new() { Interval = 1000 };
        _progressTimer.Elapsed += UpdateSongProgress;
        SongStartedPlaying += OnSongsStartedPlaying;

        messenger = wrm;
        messenger.Register(this);

        string LastSongPlayedID = Preferences.Get("LastPlayedSongID", null);
        if (LastSongPlayedID is not null)
        {
            SelectedSong = SongManager.GetSongById(LastSongPlayedID);
            PreviousSong = SelectedSong;
            SongDuration = TimeSpan.FromMilliseconds(SelectedSong.DurationInMilliseconds);
        }


        IsRefreshing = true;
        IsSongPlaying = false;
        InitializeSongsList();

        if (Songs?.Count > 0)
        {
            TotalSongsCount = Songs.Count;
        }
        IsRefreshing = false;
    }

    [ObservableProperty]
    public LyricsModel highlightedLyrics;

    [ObservableProperty]
    private ObservableCollection<SongModel> _songs;

    [ObservableProperty]
    int totalSongsCount;

    SongModel PreviousSong;
    [ObservableProperty]
    SongModel selectedSong;

    [ObservableProperty]
    bool isSongPlaying;

    [ObservableProperty]
    double songCurrentPosition;

    [ObservableProperty]
    TimeSpan songDuration;

    [ObservableProperty]
    double songProgress;

    [ObservableProperty]
    double songVolume;



    [RelayCommand]
    public void RefreshSongsList()
    {
        IsRefreshing = true;
        var defaultPlaylist = playlistManager.GetDefaultPlayList();
        if (defaultPlaylist is not null)
        {
            Songs = new ObservableCollection<SongModel>(defaultPlaylist.Songs);
        }
        IsRefreshing = false;
    }

    void InitializeSongsList()
    {
        if (Songs is null || Songs.Count == 0)
        {
            var defaultPlaylist = playlistManager.GetDefaultPlayList();
            if (defaultPlaylist is not null)
            {
                Songs = new ObservableCollection<SongModel>(defaultPlaylist.Songs);
            }
        }
    }
    [ObservableProperty]
    bool isRefreshing;


    [RelayCommand]
    public void PageLoaded()
    {
        HighlightedLyrics = new LyricsModel { Text = "No lyrics found" };
    }

    [RelayCommand]
    public void SeekSongSliderValueChanged()
    {
        var newTimeInSeconds = SongDuration.TotalSeconds * SongProgress;
        if (AudioPlayer is not null)
        {
            AudioPlayer.Seek(newTimeInSeconds);
            if (Lyrics?.Count > 1)
            {
                var LSS = Observable.Interval(TimeSpan.FromMilliseconds(250)).Subscribe(_ => UpdateHighlightedlyrics());
                LyricsSyncSubscription = LSS;
            }
            else
            {
                StartLyricsSync();
            }
        }
        else
        {
            FetchAndPlaySong(SelectedSong, newTimeInSeconds);
        }
    }

    private void ParseLrcFile(string lyricsFilePath)
    {
        var regex = new Regex(@"\[(\d+):(\d+\.\d+)\](.*)");

        foreach (var line in File.ReadLines(lyricsFilePath))
        {
            var match = regex.Match(line);
            if (match.Success)
            {
                var mins = int.Parse(match.Groups[1].Value);
                var secs = double.Parse(match.Groups[2].Value);
                var text = match.Groups[3].Value;

                var timestamp = TimeSpan.FromMinutes(mins) + TimeSpan.FromSeconds(secs);

                Lyrics.Add(new LyricsModel { Timestamp = timestamp, Text = text });
            }
        }
    }
    private void UpdateHighlightedlyrics()
    {
        if (AudioPlayer is not null)
        {
            HighlightedLyrics = Lyrics.LastOrDefault(l => l.Timestamp <= TimeSpan.FromSeconds(AudioPlayer.CurrentPosition));
        }
    }

    [RelayCommand]
    public void UpdateVolume(int value)
    {
        if (value == 1)
        {
            AudioPlayer.Volume += 0.1;
            SongVolume += 0.1;
        }
        else if (value == 0)
        {
            AudioPlayer.Volume -= 0.1;
            SongVolume -= 0.1;
        }
    }
    [RelayCommand]
    public void SliderVolumeChanged()
    {
        if (SongVolume == 0)
        {
            PauseSong();
        }
        AudioPlayer.Volume = SongVolume;
    }
    private void UpdateSongProgress(object sender, ElapsedEventArgs e)
    {
        if (AudioPlayer != null && SongDuration.TotalSeconds > 0)
        {
            SongCurrentPosition = AudioPlayer.CurrentPosition;
            var totalDuration = SongDuration.TotalSeconds;
            SongProgress = SongCurrentPosition / totalDuration;
        }
    }
    private void OnSongsStartedPlaying()
    {
        _progressTimer.Start();
    }

    private void AudioPlayer_PlaybackEnded(object sender, EventArgs e)
    {
        try
        {
            LyricsSyncSubscription?.Dispose();
            IsSongPlaying = AudioPlayer.IsPlaying;

            SongProgress = 0;

            if (AudioPlayer is not null)
            {
                AudioPlayer.PlaybackEnded -= AudioPlayer_PlaybackEnded;

                AudioPlayer.Dispose();
                AudioPlayer = null;
            }
            SongCurrentPosition = 0;
        }
        finally
        {
            if (AudioPlayer is not null)
            {
                AudioPlayer.Dispose();
                AudioPlayer = null;
            }
        }
    }

    void UpdateCurrentlyPlayingSong()
    {
        _ = new ChangingSongInfoModel()
        {
            StopCurrentlyPlayingSong = true,
            IsSongPlaying = IsSongPlaying,
            SongCurrentPosition = SongCurrentPosition,
            SongDuration = SongDuration,
            SelectedSong = SelectedSong,
            AudioPlayer = AudioPlayer,
            Lyrics = Lyrics,
        };
    }

    [RelayCommand]
    public void ClearCollection()
    {
        SongManager.DropCollection();
        PageLoaded();
    }

    [RelayCommand]
    public async void PickFolder()
    {
        CheckPermissions permCheck = new();

        if (await permCheck.CheckAndRequestStoragePermissionAsync())
        {
            CancellationTokenSource source = new();
            CancellationToken token = source.Token;

            result = await FolderPicker.PickAsync("/storage/emulated/0/Music", token);

            var folder = result.Folder.Path;
            List<string> folders = new()
            {
                folder
            };
            appSettingsManager.ScanSongs(folders);
            PageLoaded();
        }
    }

    public void Receive(RefreshSongsList message)
    {
        MainThread.BeginInvokeOnMainThread(PageLoaded);
    }
}