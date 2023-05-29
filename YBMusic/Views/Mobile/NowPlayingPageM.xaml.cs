using CommunityToolkit.Mvvm.Messaging;
using YB.Utilities.Messages;
using YBMusic.ViewModels;

namespace YBMusic.Views.Mobile;

public partial class NowPlayingPageM : ContentPage
{
    MusicServiceVM viewModel;
    public NowPlayingPageM(MusicServiceVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;
    }

    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}