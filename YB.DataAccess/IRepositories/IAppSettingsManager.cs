using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YB.DataAccess.IRepositories;

public interface IAppSettingsManager
{
    void DropCollection();
    void ScanSongs();
    void ScanSongs(List<string> PathsToFolders);
}
