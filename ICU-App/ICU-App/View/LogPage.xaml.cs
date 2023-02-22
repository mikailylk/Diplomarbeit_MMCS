using ICU_App.ViewModel;

namespace ICU_App.View;

/// <summary>
/// The LogPage class is a view that displays logs. It contains a reference to an instance of LogViewModel,
/// which is the view model for this page.
/// </summary>
public partial class LogPage : ContentPage
{
	private LogViewModel _logViewModel;

    /// <summary>
	/// Constructor for the LogPage class. Initializes the page and sets the binding context to the specified view model.
	/// </summary>
	/// <param name="vm">
	/// The LogViewModel to use as the binding context for this page.
	/// </param>
	public LogPage(LogViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		_logViewModel = vm;
        _logViewModel.MapView = mapView;
    }

    /// <summary>
	/// Overrides the OnNavigatedTo method of the ContentPage class
	/// and sets the IsActive property of the view model to true.
	/// </summary>
	/// <param name="args">
	/// The arguments passed to the OnNavigatedTo method.
	/// </param>
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
		_logViewModel.IsActive = true;
    }

    /// <summary>
    /// Overrides the OnNavigatedFrom method of the ContentPage class 
    /// and sets the IsActive property of the view model to false.
    /// </summary>
    /// <param name="args">
    /// The arguments passed to the OnNavigatedFrom method.
    /// </param>
    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        _logViewModel.IsActive = false;
    }
}