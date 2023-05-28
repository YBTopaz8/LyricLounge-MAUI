
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

    FolderPickerResult result;

    System.Timers.Timer _progressTimer;
    public event Action SongStartedPlaying;

    [ObservableProperty]
    List<LyricsModel> lyrics;


    WeakReferenceMessenger messenger;
    public MusicServiceVM(IAppSettingsManager settingsManager, IFolderPicker Picker, ISongsManager songsManager, IAudioManager audio, WeakReferenceMessenger wrm)
    {
        appSettingsManager = settingsManager;
        FolderPicker = Picker;
        SongManager = songsManager;
        audioManager = audio;

        _progressTimer = new() { Interval = 1000 };
        _progressTimer.Elapsed += UpdateSongProgress;
        SongStartedPlaying += OnSongsStartedPlaying;

        messenger = wrm;
        messenger.Register(this);
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

    IAudioPlayer AudioPlayer { get; set; }
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

    IDisposable LyricsSyncSubscription;

    [RelayCommand]
    public void PageLoaded()
    {
        IsSongPlaying = false;

        var songsFromDB = SongManager.GetSongs();

        Songs = new ObservableCollection<SongModel>(songsFromDB);

        SelectedSong = Songs.FirstOrDefault();
        PreviousSong = SelectedSong;
        if (SelectedSong is not null)
        {
            SongDuration = TimeSpan.FromMilliseconds(SelectedSong.DurationInMilliseconds);
        }
        TotalSongsCount = Songs.Count;
        HighlightedLyrics = new LyricsModel { Text = "No lyrics found" };
    }

    [RelayCommand]
    public void PlaySongFromTap(SongModel song)
    {
        try
        {
            PreviousSong = SelectedSong;
            SelectedSong = song;
            SongDuration = TimeSpan.FromMilliseconds(SelectedSong.DurationInMilliseconds);
            if (IsSongPlaying)
            {
                AudioPlayer.PlaybackEnded -= AudioPlayer_PlaybackEnded;
                AudioPlayer.Stop();
            }
            using FileStream songStream = FetchAndPlaySong(song);

            StartLyricsSync();

            UpdateCurrentlyPlayingSong(); //send message to update now playing page
        }
        catch (Exception ex)
        {
            //show exception in debug
            Debug.WriteLine(ex.Message);
        }
    }

    private FileStream FetchAndPlaySong(SongModel song, double SeekPosition = 0)
    {
        FileStream songStream = new(song.FilePath, FileMode.Open, FileAccess.Read);
        AudioPlayer = audioManager.CreatePlayer(songStream);
        if (SongVolume == 0)
        {
            AudioPlayer.Volume = 0.5;
            SongVolume = 0.5;
        }
        if (SeekPosition != 0)
        {
            AudioPlayer.Seek(SeekPosition);
        }
        AudioPlayer.Play();
        IsSongPlaying = AudioPlayer.IsPlaying;
        SongVolume = AudioPlayer.Volume;
        //SongStartedPlaying?.Invoke();
        _progressTimer.Start();
        AudioPlayer.PlaybackEnded += AudioPlayer_PlaybackEnded;
        return songStream;
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

    [RelayCommand]
    public void PlayReSumeSong()
    {
        if (AudioPlayer is null)
        {
            using FileStream songStream = FetchAndPlaySong(SelectedSong);
        }
        else
        {
            AudioPlayer.Play();
            IsSongPlaying = AudioPlayer.IsPlaying;
            _progressTimer.Start();
        }
    }

    [RelayCommand]
    public void PauseSong()
    {
        AudioPlayer.Pause();
        _progressTimer.Stop();
        IsSongPlaying = AudioPlayer.IsPlaying;
    }

    [RelayCommand]
    public void StopSong()
    {
        LyricsSyncSubscription.Dispose();
        AudioPlayer.Stop();
        _progressTimer.Stop();
        IsSongPlaying = AudioPlayer.IsPlaying;
        AudioPlayer.Dispose();

        if (LyricsSyncSubscription is not null)
        {
            LyricsSyncSubscription.Dispose();
            LyricsSyncSubscription = null;
        }
    }

    private async void StartLyricsSync()
    {
        try
        {
            string SongFilePath = SelectedSong.FilePath;

            string lyricsFilePath = Path.ChangeExtension(SongFilePath, ".lrc");
            if (File.Exists(lyricsFilePath))
            {
                if (Lyrics is null)
                {
                    Lyrics = new();
                    ParseLrcFile(lyricsFilePath);
                }
                else
                {
                    if (SelectedSong != PreviousSong)
                    {
                        Lyrics = new();
                        ParseLrcFile(lyricsFilePath);
                    }
                }
                var LSS = Observable.Interval(TimeSpan.FromMilliseconds(250)).Subscribe(_ => UpdateHighlightedlyrics());
                LyricsSyncSubscription = LSS;
            }
            else
            {
                Lyrics = new()
                {
                    new LyricsModel { Text = "No lyrics found" }
                };
            }

        }
        catch (Exception ex)
        {
            // show a display alert on shell
            await Shell.Current.DisplayAlert("error", ex.Message, "Cancel");
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
        ChangingSongInfoModel changingSongInfo = new()
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

        if(await permCheck.CheckAndRequestStoragePermissionAsync())
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