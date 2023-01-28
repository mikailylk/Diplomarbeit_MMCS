using ICU_App.ViewModel;

namespace ICU_App.View;

public partial class SettingsPage : ContentPage
{
	private SettingsViewModel _settingsviewmodel;
	public SettingsPage(SettingsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		_settingsviewmodel = vm;
	}

    private void SettingsPage_Loaded(object sender, EventArgs e)
    {
		_settingsviewmodel.IsActive = true;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        _settingsviewmodel.IsActive = true;
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        _settingsviewmodel.IsActive = false;
    }

    private void SettingsPage_Unloaded(object sender, EventArgs e)
    {
        _settingsviewmodel.IsActive = true;
    }

}