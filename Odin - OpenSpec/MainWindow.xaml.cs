using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Odin___OpenSpec.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Odin___OpenSpec
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly IThemeService _themeService;

        public MainWindow()
        {
            InitializeComponent();
            
            // Get theme service from DI container
            var app = Application.Current as App;
            _themeService = app?.ServiceProvider?.GetService(typeof(IThemeService)) as IThemeService
                ?? throw new InvalidOperationException("Theme service not available");
        }

        private async void ToggleThemeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _themeService.ToggleThemeAsync();
            }
            catch (Exception ex)
            {
                // In a real app, we'd show a proper error dialog
                System.Diagnostics.Debug.WriteLine($"Failed to toggle theme: {ex.Message}");
            }
        }
    }
}
