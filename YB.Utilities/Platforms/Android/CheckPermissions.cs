using Android;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace YB.Utilities;

// All the code in this file is only included on Android.
public class CheckPermissions
{
    public async Task<bool> CheckAndRequestStoragePermissionAsync()
    {
        var activity = Platform.CurrentActivity ?? throw new NullReferenceException("Current Activity is null");

        if (ContextCompat.CheckSelfPermission(activity, Manifest.Permission.ReadExternalStorage) == Permission.Granted)
        {
            return true;
        }
        else
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(activity, Manifest.Permission.ReadExternalStorage))
            {
                //  Toast.MakeText(activity, "Please Grant Storage Permissions", ToastLength.Short).Show();
                await Shell.Current.DisplayPromptAsync("Permission Request", "Please Grant Storage Permissions", "OK", "Cancel");
            }
            else
            {
                ActivityCompat.RequestPermissions(activity, new string[] { Manifest.Permission.ReadExternalStorage }, 1);
                return true;
            }
            return false;
        }
        //var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        //if (status != PermissionStatus.Granted)
        //{
        //    if (Permissions.ShouldShowRationale<Permissions.StorageRead>())
        //    {
        //        await Shell.Current.DisplayPromptAsync("Permission Request", "Please Grant Storage Permissions", "OK", "Cancel");
        //    }
        //    // await Shell.Current.DisplayAlert("Storage Permission", "Permission Denied", "OK");
        //    _ = await Permissions.RequestAsync<Permissions.StorageRead>();
        //    return false;
        //}
        //else
        //{
        //    return true;
        //}
    }
}