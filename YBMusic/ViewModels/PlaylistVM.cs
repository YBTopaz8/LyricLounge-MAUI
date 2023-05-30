using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YB.DataAccess.IRepositories;
using YB.Models;

namespace YBMusic.ViewModels;

public partial class PlaylistVM : ObservableObject
{
    IPlaylistManager playlistManager;
    public PlaylistVM(IPlaylistManager plManager)
    {
        playlistManager = plManager;
    }

    [ObservableProperty]
    List<PlaylistModel> playlists;
    [ObservableProperty]
    int numberOfPlaylists;

    [RelayCommand]
    void PageLoaded()
    {
        Playlists ??= playlistManager.GetAllPlaylists();
        NumberOfPlaylists = Playlists.Count;
    }

    [RelayCommand]
    void OpenPlaylistPage(PlaylistModel playlist)
    {

        Shell.Current.DisplayAlert("Playlist Info", $"Selected Playlist is {playlist.Name} and has {playlist.Songs.Count} songs", "OK");
    }
}