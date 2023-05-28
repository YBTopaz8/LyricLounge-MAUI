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
    public string FileSize { get; set; }
    public int BitRate { get; set; }
    public decimal Rating { get; set; }
    public IList<string> Lyrics { get; }
    public string ImagePath { get; set; }
    public bool IsFavorite { get; set; }
    public DateTimeOffset DateAdded { get; set; }
    public IList<DateTimeOffset> PlayTimes { get; }
    public GenreModel? Genre { get; set; }
    public ArtistModel? Artist { get; set; }
    public AlbumModel? Album { get; set; }
}
