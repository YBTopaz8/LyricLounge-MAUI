using Realms;
namespace YB.Models;

public class SongModel : RealmObject
{
    //add a realm db id
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; }
    public string FilePath { get; set; }
    public double DurationInMilliseconds { get; set; }
    public int? ReleaseYear { get; set; }
    public int? TrackNumber { get; set; }
    public string FileFormat { get; set; }
    public double FileSize { get; set; }
    public int BitRate { get; set; }
    public double SampleRate { get; set; }
    public decimal Rating { get; set; }
    public bool HasLyrics { get; set; }
    public IList<LyricPhraseModel> SynchronizedLyrics { get;}
    public string UnSynchronizedLyrics { get; set; }
    public string ImagePath { get; set; }

    public bool IsFavorite { get; set; }
    public DateTimeOffset DateAdded { get; set; }
    public IList<DateTimeOffset> PlayTimes { get; }

    public byte[] Picture { get; set; }
    public GenreModel? Genre { get; set; }
    public ArtistModel? Artist { get; set; }
    public AlbumModel? Album { get; set; }
}

public class LyricPhraseModel : RealmObject
{
    public int TimestampMs { get; set; }

    public string Text { get; set; }
}