using YBMusic.ViewModels;

namespace YBMusic.Views.Mobile;

public partial class PlayListPageM : ContentPage
{
    PlaylistVM viewModel;
    public PlayListPageM(PlaylistVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = viewModel;
        viewModel.PageLoadedCommand.Execute(null);
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(NowPlayingPageM));
    }
}