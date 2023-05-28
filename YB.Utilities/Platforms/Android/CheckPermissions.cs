namespace YB.Utilities;

// All the code in this file is only included on Android.
public class CheckPermissions
{
    public async Task<bool> CheckAndRequestStoragePermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        if (status != PermissionStatus.Granted)
        {
            if (Permissions.ShouldShowRationale<Permissions.StorageRead>())
            {
                await Shell.Current.DisplayPromptAsync("Permission Request", "Please Grant Storage Permissions", "OK", "Cancel");
            }
            // await Shell.Current.DisplayAlert("Storage Permission", "Permission Denied", "OK");
            _ = await Permissions.RequestAsync<Permissions.StorageRead>();
            return false;
        }
        else
        {
            return true;
        }
    }
}