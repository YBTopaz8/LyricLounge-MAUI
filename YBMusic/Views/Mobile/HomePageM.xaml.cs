using System.ComponentModel;
using System.Globalization;
using YBMusic.ViewModels;

namespace YBMusic.Views.Mobile;

public partial class HomePageM : UraniumUI.Pages.UraniumContentPage
{
    public bool IsBottomSheetMusicPresented;
    readonly MusicServiceVM viewModel;
    public HomePageM(MusicServiceVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = viewModel;
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();

        
        viewModel.PageLoadedCommand.Execute(null);
    }
    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(NowPlayingPageM));
    }
}
