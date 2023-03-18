using ICU_App.View;

namespace ICU_App;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
		Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
		Routing.RegisterRoute(nameof(DebugPage), typeof(DebugPage));
		Routing.RegisterRoute(nameof(LogPage), typeof(LogPage));
	}
}
