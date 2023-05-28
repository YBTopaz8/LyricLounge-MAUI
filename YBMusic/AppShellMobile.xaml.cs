using YBMusic.Views.Mobile;

namespace YBMusic;

public partial class AppShellMobile : Shell
{
	public AppShellMobile()
	{
		InitializeComponent();
        Routing.RegisterRoute("HomePageM", typeof(HomePageM));
		Routing.RegisterRoute("PlayListPageM", typeof(PlayListPageM));
		Routing.RegisterRoute("NowPlayingPageM", typeof(NowPlayingPageM));
		Routing.RegisterRoute("AppSetttingsListM", typeof(AppSettingsListM));
    }
}