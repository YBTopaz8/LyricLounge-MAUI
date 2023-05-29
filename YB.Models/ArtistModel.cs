using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YB.Models;

public class ArtistModel : RealmObject
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public IList<AlbumModel> Albums { get; }
    public string ImagePath { get; set; }
}
