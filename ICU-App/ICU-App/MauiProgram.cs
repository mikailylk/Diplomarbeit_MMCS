using ICU_App.ViewModel;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Syncfusion.Maui.Core.Hosting;

namespace ICU_App.View;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseSkiaSharp(true) // Openstreetmap verwenden --> Mapsui
			.ConfigureSyncfusionCore()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// View Registrationen als Service
		builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<DebugPage>();
		builder.Services.AddTransient<SettingsPage>();
		builder.Services.AddTransient<LogPage>();

        // TODO: Zwischen Windows und Android Platform unterscheiden -> Sensoren nicht einlesen (Orientation) für Windows
		// ViewModel Registrationen als Service
        builder.Services.AddTransient<MainViewModel>();
		builder.Services.AddTransient<DebugViewModel>();
		builder.Services.AddTransient<SettingsViewModel>();
		builder.Services.AddTransient<LogViewModel>();


		// Sensor Registrationen als Service (hier: nur für ANDROID Geräte nutzen)
		#if ANDROID
			builder.Services.AddSingleton<IOrientationSensor>(OrientationSensor.Default);
		#endif
		return builder.Build();
	}
}
