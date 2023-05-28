using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Messaging;
using YB.DataAccess.IRepositories;
using YB.Utilities;
using YB.Utilities.Messages;

namespace YBMusic.Views.Mobile;

public partial class AppSettingsListM : ContentPage
{
	IAppSettingsManager _appSettingsManager;
    FolderPickerResult result;
    WeakReferenceMessenger messenger;
	public AppSettingsListM(IAppSettingsManager settingsManager, WeakReferenceMessenger wrm)
	{
		InitializeComponent();
		_appSettingsManager = settingsManager;
        messenger = wrm;
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
        CheckPermissions permCheck = new();

        if (await permCheck.CheckAndRequestStoragePermissionAsync())
        {
            CancellationTokenSource source = new();
            CancellationToken token = source.Token;

            result = await FolderPicker.PickAsync("/storage/emulated/0/Music", token);

            var folder = result.Folder.Path;
            List<string> folders = new()
            {
                folder
            };
            _appSettingsManager.ScanSongs(folders);
        }

        messenger.Send(new RefreshSongsList(true));
    }
}