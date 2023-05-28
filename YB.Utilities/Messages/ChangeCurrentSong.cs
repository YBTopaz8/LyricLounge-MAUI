using CommunityToolkit.Mvvm.Messaging.Messages;
using Plugin.Maui.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YB.Models;

namespace YB.Utilities.Messages;

public class ChangeCurrentSong : ValueChangedMessage<ChangingSongInfoModel>
{
    public ChangeCurrentSong(ChangingSongInfoModel value) : base(value)
    {
    }
}

public class StopLyricsUpdate : ValueChangedMessage<bool>
{
    public StopLyricsUpdate(bool value) : base(value)
    {
    }
}

public class ChangingSongInfoModel
{
    public bool StopCurrentlyPlayingSong { get; set; }
    public bool IsSongPlaying { get; set; }
    public double SongCurrentPosition { get; set; }
    public TimeSpan SongDuration { get; set; }
    public SongModel SelectedSong { get; set; }
    public IAudioPlayer AudioPlayer { get; set; }
    public List<LyricsModel> Lyrics { get; set; }
    
}