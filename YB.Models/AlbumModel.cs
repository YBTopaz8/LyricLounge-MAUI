using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YB.Models;

public class AlbumModel :RealmObject
{
    [PrimaryKey]
    public string  Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public int? ReleaseYear { get; set; }
    //property for number of tracks
    public int NumberOfTracks { get; set; }
    public ArtistModel Artist { get; set; }
    public IList<SongModel> Songs { get; }
    public string ImagePath { get; set; }
}
