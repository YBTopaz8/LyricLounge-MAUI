using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using YB.Models;

namespace YBMusic.ViewModels;
//MusicPlayback related methods 

public partial class MusicServiceVM : ObservableObject
{
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
            AudioPlayer.Volume = 1;
            SongVolume = 1;
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

}