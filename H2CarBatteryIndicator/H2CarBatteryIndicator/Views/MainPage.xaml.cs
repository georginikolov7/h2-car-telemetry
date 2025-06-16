using H2CarBatteryIndicator.ViewModels;

namespace H2CarBatteryIndicator.View;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel vm;
    public MainPage(MainPageViewModel model)
    {
        InitializeComponent();
        BindingContext = model;
        vm = model;
    }
    protected async override void OnAppearing()
    {
        base.OnAppearing();
        if (vm != null)
        {
            await vm.RequestPermissions();
            await vm.OnPageAppear();
        }
    }

}

