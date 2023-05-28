using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YB.Models;

public class AppSettingsModel : RealmObject
{
    public bool HasPerformedInitialScan { get; set; }
    public bool HasPerformedManualScan { get; set; }
    public IList<string> MusicFoldersPaths { get; }
}
