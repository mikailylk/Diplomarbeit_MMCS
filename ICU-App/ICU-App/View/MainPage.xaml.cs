using System.Numerics;
using ICU_App.Calc;
using ICU_App.ViewModel;

namespace ICU_App.View;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

