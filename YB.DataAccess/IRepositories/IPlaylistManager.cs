using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YB.Models;

namespace YB.DataAccess.IRepositories;
public interface IPlaylistManager
{
    Task<bool> CreateNewPlaylist(PlaylistModel playlist);
    List<PlaylistModel> GetAllPlaylists();
    PlaylistModel GetPlayList(string PlaylistName);
    PlaylistModel GetDefaultPlayList();
    bool AddSongToPlaylist(string playlistName, SongModel song);
    bool RemoveSongFromPlaylist(string playlistName, SongModel song);
    //bool AddListOfSongsToPlaylist(string playlistName, List<SongModel> songs); later
    bool EditPlaylist(string playlistName, PlaylistModel playlist);
    bool DeletePlaylist(string playlistName);
}