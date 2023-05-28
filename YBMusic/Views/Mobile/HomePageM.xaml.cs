using System.ComponentModel;
using System.Globalization;
using YBMusic.ViewModels;

namespace YBMusic.Views.Mobile;

public partial class HomePageM :  UraniumUI.Pages.UraniumContentPage
{
    public bool IsBottomSheetMusicPresented;
    readonly MusicServiceVM viewModel;
	public HomePageM(MusicServiceVM vm)
	{
		InitializeComponent();
		viewModel = vm;
		BindingContext = viewModel;

        viewModel.PageLoadedCommand.Execute(null);
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        BottomSheetMusic.PropertyChanged += BottomSheetMusicIsPresented;
    }

    private void BottomSheetMusicIsPresented(object sender, PropertyChangedEventArgs e)
    {
        
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(NowPlayingPageM));
    }
}
