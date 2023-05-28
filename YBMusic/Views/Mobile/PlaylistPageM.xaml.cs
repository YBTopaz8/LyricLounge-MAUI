namespace YBMusic.Views.Mobile;

public partial class PlayListPageM : ContentPage
{
	public PlayListPageM()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
		await Shell.Current.GoToAsync(nameof(NowPlayingPageM));
    }
}