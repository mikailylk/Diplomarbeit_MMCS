using ICU_App.Calc;
using ICU_App.ViewModel;
using System.Numerics;
using ICU_App.Helper;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using Microsoft.Maui.Dispatching;
using System.Net.NetworkInformation;
// using Android.Media;
using System.Text.Json;
using System.Globalization;

namespace ICU_App.View;

public partial class DebugPage : ContentPage
{
    public DebugPage(DebugViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}