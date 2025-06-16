using H2CarBatteryIndicator.Services;
using H2CarBatteryIndicator.View;
using H2CarBatteryIndicator.ViewModels;
using static H2CarBatteryIndicator.Constants.AppConstants;
namespace H2CarBatteryIndicator;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        builder.Services.AddSingleton<IBleService>(new BleService(BleDeviceName, Guid.Parse(ServiceGuid), Guid.Parse(CharacteristicGuid)));
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<MainPageViewModel>();
        return builder.Build();
    }
}
