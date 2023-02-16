using ICU_App.ViewModel;

namespace ICU_App.View;

public partial class LogPage : ContentPage
{
	private LogViewModel _logViewModel;
	public LogPage(LogViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		_logViewModel = vm;
	}

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
		_logViewModel.IsActive = true;
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        _logViewModel.IsActive = false;
    }
}