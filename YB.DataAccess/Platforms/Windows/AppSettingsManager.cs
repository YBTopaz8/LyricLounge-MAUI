using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YB.DataAccess.IRepositories;

namespace YB.DataAccess.Platforms;

public class AppSettingsManager : IAppSettingsManager
{
    public void DropCollectionAsync()
    {
        throw new NotImplementedException();
    }

    public void ScanSongs()
    {
        throw new NotImplementedException();
    }

    public void ScanSongs(List<string> PathsToFolders)
    {
        throw new NotImplementedException();
    }
}