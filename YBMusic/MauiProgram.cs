
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Plugin.ContextMenuContainer;
using Plugin.Maui.Audio;
using UraniumUI;
using YB.DataAccess;
using YB.DataAccess.IRepositories;
using YB.DataAccess.Platforms;
using YB.DataAccess.Platforms.IRepositories;
using YB.DataAccess.Repositories;
using YBMusic.ViewModels;
using YBMusic.Views.Mobile;

namespace YBMusic;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseUraniumUI()
            .UseUraniumUIMaterial()
            .UseMauiCommunityToolkitMediaElement()
            .ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                handlers.AddHandler(typeof(Shell), typeof(MyShellRenderer));
#endif
                handlers.AddHandler(typeof(ContextMenuContainer), typeof(ContextMenuContainerRenderer));
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton(WeakReferenceMessenger.Default);
        builder.Services.AddSingleton(AudioManager.Current);

        builder.Services.AddSingleton(FolderPicker.Default);
        builder.Services.AddSingleton(FilePicker.Default);
        builder.Services.AddSingleton(FileSaver.Default);

        /*----------------------- REGISTERING Repositories ----------------------------------------------------------------------*/
        builder.Services.AddSingleton<IDataAccessRepo, DataAccessRepo>();

        builder.Services.AddSingleton<IAppSettingsManager, AppSettingsManager>();
        builder.Services.AddSingleton(SongsManager.Current);
        builder.Services.AddSingleton(PlaylistManager.Current);
        builder.Services.AddSingleton<IGenreManager, GenreManager>();
        /*
		builder.Services.AddSingleton<IAlbumManager, AlbumManager>();
		builder.Services.AddSingleton<IArtistManager, ArtistManager>();
		builder.Services.AddSingleton<IPlaylistManager, PlaylistManager>(); */

        /*------------------------REGISTERING VIEW MODELS---------------------------------------------------------------------*/
        builder.Services.AddSingleton<MusicServiceVM>();
        builder.Services.AddSingleton<PlaylistVM>();

        /*------------------------REGISTERING DESKTOP VIEWS ---------------------------------------------------------------------*/

        /*------------------------REGISTERING MOBILE VIEWS ----------------------------------------------------------------------*/
        builder.Services.AddSingleton<HomePageM>();
        builder.Services.AddSingleton<NowPlayingPageM>();
        builder.Services.AddSingleton<PlayListPageM>();
        builder.Services.AddSingleton<AppSettingsListM>();

        return builder.Build();
    }
}
