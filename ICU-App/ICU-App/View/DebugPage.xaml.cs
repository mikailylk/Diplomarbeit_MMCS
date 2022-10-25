using ICU_App.Calc;
using ICU_App.ViewModel;
using System.Numerics;

namespace ICU_App.View;

public partial class DebugPage : ContentPage
{
    public DebugPage(DebugViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
    }
}