using ICU_App.ViewModel;

namespace ICU_App.View;

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

		// View Registrationen als Service
		builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<DebugPage>();

        // TODO: Zwischen Windows und Android Platform unterscheiden -> Sensoren nicht einlesen (Orientation) für Windows
		// ViewModel Registrationen als Service
        builder.Services.AddSingleton<MainViewModel>();
		builder.Services.AddTransient<DebugViewModel>();


		// Sensor Registrationen als Service
		if (!DeviceInfo.Current.Platform.Equals(DevicePlatform.WinUI))
		{
			builder.Services.AddSingleton<IOrientationSensor>(OrientationSensor.Default);
		}
		return builder.Build();
	}
}
