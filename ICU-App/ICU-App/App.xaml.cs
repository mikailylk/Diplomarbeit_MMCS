using ICU_App.Resources.Keys;
//using Microsoft.Maui.Controls.Compatibility.Platform.Android;

namespace ICU_App;

public partial class App : Application
{
	public App()
	{
		//Register Syncfusion license
		Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(Keys.SyncfusionKey);

		InitializeComponent();

		MainPage = new AppShell();
	}
}
