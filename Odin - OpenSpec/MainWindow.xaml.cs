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
        private string _currentModule = "Home";

        public MainWindow()
        {
            InitializeComponent();
            
            // Get theme service from DI container
            var app = Application.Current as App;
            _themeService = app?.ServiceProvider?.GetService(typeof(IThemeService)) as IThemeService
                ?? throw new InvalidOperationException("Theme service not available");
            
            // Set initial content and navigation state
            UpdateContentArea("Home");
            UpdateNavigationButtonStates(HomeButton);
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

        private void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string moduleId)
            {
                _currentModule = moduleId;
                UpdateContentArea(moduleId);
                UpdateNavigationButtonStates(button);
            }
        }

        private void UpdateNavigationButtonStates(Button activeButton)
        {
            // Reset all buttons to default style
            var buttons = new[] { HomeButton, CalendarButton, TasksButton, GroceryButton, WeatherButton, MusicButton, SettingsButton };
            foreach (var button in buttons)
            {
                // Use ButtonStyle for inactive buttons
                button.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
                button.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            }
            
            // Highlight active button
            activeButton.Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["SystemControlBackgroundAccentBrush"];
            activeButton.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["SystemControlForegroundAccentBrush"];
        }

        private void UpdateContentArea(string moduleId)
        {
            // Update content based on selected module
            ContentTitle.Text = $"{moduleId} Module";
            
            switch (moduleId)
            {
                case "Home":
                    ContentMessage.Text = "Welcome to your personal productivity hub!";
                    break;
                case "Calendar":
                    ContentMessage.Text = "View and manage your calendar events and appointments.";
                    break;
                case "Tasks":
                    ContentMessage.Text = "Track your to-do lists and tasks.";
                    break;
                case "Grocery":
                    ContentMessage.Text = "Manage your grocery lists and shopping needs.";
                    break;
                case "Weather":
                    ContentMessage.Text = "Check current weather conditions and forecasts.";
                    break;
                case "Music":
                    ContentMessage.Text = "Control your music playback and playlists.";
                    break;
                case "Settings":
                    ContentMessage.Text = "Configure application preferences and settings.";
                    break;
                default:
                    ContentMessage.Text = "Module not found.";
                    break;
            }
        }
    }
}
