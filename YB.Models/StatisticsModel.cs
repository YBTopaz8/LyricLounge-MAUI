using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YB.Models;

public class StatisticsModel : RealmObject
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public StringPair SongPlays { get; set; }
    public StringPair ArtistPlays { get; set; }
    public StringPair GenrePlays { get; set;}
    public StringPair SongsSkips { get; set; }

    //Remember to update the statistics each time a song is played or skipped,
    //and to handle errors and edge cases appropriately,
    //such as what to do if a song, artist, or genre is deleted.
}

public class StringPair : RealmObject
{
    // declaire properties Key and Value to be string and int respectively with their getters and setters
    public string Key { get; set; }
    public int Value { get; set; }
}
