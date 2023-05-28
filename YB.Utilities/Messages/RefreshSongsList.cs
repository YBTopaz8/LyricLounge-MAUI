using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YB.Utilities.Messages;

public class RefreshSongsList : ValueChangedMessage<bool>
{
    public RefreshSongsList(bool value) : base(value)
    {
        
    }
}