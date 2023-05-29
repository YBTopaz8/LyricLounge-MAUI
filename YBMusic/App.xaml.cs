using LiteDB.Async;
using YB.DataAccess.Platforms.IRepositories;

namespace YBMusic;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
#if ANDROID

        MainPage = new AppShellMobile();
#elif WINDOWS

        MainPage = new AppShell();
#endif

    }

    protected override void OnResume()
    {
    }
    protected override void OnSleep()
    {
    }
}