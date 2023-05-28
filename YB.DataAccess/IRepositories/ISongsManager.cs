using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YB.Models;

namespace YB.DataAccess.IRepositories;

public interface ISongsManager
{
    List<SongModel> GetSongs();
    bool AddSong(SongModel song);
    Task<bool> AddListOfSongsAsync(List<SongModel> song);
    Task<SongModel> GetSongById(string id);

    void DropCollection();
}
