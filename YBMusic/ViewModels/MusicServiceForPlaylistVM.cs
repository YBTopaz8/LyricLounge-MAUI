using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YB.DataAccess.IRepositories;
using YB.Utilities.Messages;

namespace YBMusic.ViewModels;

public partial class MusicServiceVM : ObservableObject
{
    IPlaylistManager playlistManager;
}