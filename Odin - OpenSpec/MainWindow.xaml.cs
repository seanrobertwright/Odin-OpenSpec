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
        private bool _isSidebarExpanded = true;
        private const int AnimationDuration = 200; // milliseconds
        private const double SwipeThreshold = 50; // pixels
        private Windows.Foundation.Point _swipeStartPoint;
        private bool _isSwipeInProgress = false;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                
                // Get theme service from DI container (optional for now)
                var app = Application.Current as App;
                _themeService = app?.ServiceProvider?.GetService(typeof(IThemeService)) as IThemeService;
                
                // Set initial content and navigation state
                UpdateContentArea("Home");
                UpdateNavigationButtonStates(HomeButton);
                
                // Set up gesture handlers (temporarily disabled - will implement after basic functionality works)
                // SetupGestureHandlers();
                
                System.Diagnostics.Debug.WriteLine("MainWindow initialized successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in MainWindow constructor: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void SetupGestureHandlers()
        {
            // TODO: Gesture handlers temporarily disabled - need to find the correct Grid element
            // The root element in XAML needs to be accessed correctly after InitializeComponent()
        }

        private void RootGrid_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _swipeStartPoint = e.Position;
            _isSwipeInProgress = true;
        }

        private void RootGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (!_isSwipeInProgress) return;

            var currentPoint = e.Position;
            var deltaX = currentPoint.X - _swipeStartPoint.X;

            // Check if swipe gesture should trigger expand/collapse
            // Swipe right from left edge (< 50px from left) to expand
            if (!_isSidebarExpanded && _swipeStartPoint.X < 50 && deltaX > SwipeThreshold)
            {
                e.Complete();
                _isSwipeInProgress = false;
                _ = ExpandSidebar();
            }
            // Swipe left to collapse (only if swipe starts on the sidebar)
            else if (_isSidebarExpanded && _swipeStartPoint.X < 250 && deltaX < -SwipeThreshold)
            {
                e.Complete();
                _isSwipeInProgress = false;
                _ = CollapseSidebar();
            }
        }

        private void RootGrid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _isSwipeInProgress = false;
        }

        private async System.Threading.Tasks.Task LoadNavigationStateAsync()
        {
            try
            {
                var app = Application.Current as App;
                var dataContext = app?.ServiceProvider?.GetService(typeof(Services.IDataContext)) as Services.IDataContext;
                
                if (dataContext == null) return;

                // Get current user (assuming user 1 for now)
                var userId = 1;
                var state = await dataContext.GetNavigationStateAsync(userId);
                
                if (state != null)
                {
                    // Restore sidebar state
                    if (state.IsExpanded != _isSidebarExpanded)
                    {
                        if (state.IsExpanded)
                        {
                            await ExpandSidebar();
                        }
                        else
                        {
                            await CollapseSidebar();
                        }
                    }
                    
                    // Restore last module
                    if (!string.IsNullOrEmpty(state.LastModule))
                    {
                        _currentModule = state.LastModule;
                        UpdateContentArea(state.LastModule);
                        
                        // Find and highlight the corresponding button
                        var button = state.LastModule switch
                        {
                            "Home" => HomeButton,
                            "Calendar" => CalendarButton,
                            "Tasks" => TasksButton,
                            "Grocery" => GroceryButton,
                            "Weather" => WeatherButton,
                            "Music" => MusicButton,
                            "Settings" => SettingsButton,
                            _ => HomeButton
                        };
                        UpdateNavigationButtonStates(button);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load navigation state: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task SaveNavigationStateAsync()
        {
            try
            {
                var app = Application.Current as App;
                var dataContext = app?.ServiceProvider?.GetService(typeof(Services.IDataContext)) as Services.IDataContext;
                
                if (dataContext == null) return;

                // Save current user state (assuming user 1 for now)
                var userId = 1;
                var state = new Models.NavigationState
                {
                    UserId = userId,
                    IsExpanded = _isSidebarExpanded,
                    LastModule = _currentModule,
                    UpdatedDate = DateTime.UtcNow
                };
                
                await dataContext.UpdateNavigationStateAsync(state);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save navigation state: {ex.Message}");
            }
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

        private async void ThemeSwitcher_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_themeService == null || ThemeSwitcher == null) return;
            
            try
            {
                var selectedItem = ThemeSwitcher.SelectedItem as ComboBoxItem;
                if (selectedItem?.Tag is string themeTag)
                {
                    // Map ElementTheme to ApplicationTheme
                    Microsoft.UI.Xaml.ApplicationTheme appTheme = themeTag switch
                    {
                        "Light" => Microsoft.UI.Xaml.ApplicationTheme.Light,
                        "Dark" => Microsoft.UI.Xaml.ApplicationTheme.Dark,
                        _ => Microsoft.UI.Xaml.ApplicationTheme.Light // Default fallback
                    };
            
                    await _themeService.SetThemeAsync(appTheme);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to change theme: {ex.Message}");
            }
        }

        private async void CollapseButton_Click(object sender, RoutedEventArgs e)
        {
            await CollapseSidebar();
        }

        private async void ExpandButton_Click(object sender, RoutedEventArgs e)
        {
            await ExpandSidebar();
        }

        private async System.Threading.Tasks.Task CollapseSidebar()
        {
            if (!_isSidebarExpanded) return;

            _isSidebarExpanded = false;

            // Create storyboard for smooth animation
            var storyboard = new Microsoft.UI.Xaml.Media.Animation.Storyboard();

            // Fade out expanded sidebar
            var fadeOutAnimation = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDuration / 2)),
                EasingFunction = new Microsoft.UI.Xaml.Media.Animation.CubicEase 
                { 
                    EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseIn 
                }
            };
            
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(fadeOutAnimation, NavigationSidebar);
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(fadeOutAnimation, "Opacity");
            storyboard.Children.Add(fadeOutAnimation);

            // Start animation
            storyboard.Begin();
            
            // Wait for animation to complete
            await System.Threading.Tasks.Task.Delay(AnimationDuration / 2);
            
            // Switch visibility and update column width
            NavigationSidebar.Visibility = Visibility.Collapsed;
            CollapsedSidebar.Visibility = Visibility.Visible;
            SidebarColumn.Width = new GridLength(60);
            
            // Fade in collapsed sidebar
            var fadeInStoryboard = new Microsoft.UI.Xaml.Media.Animation.Storyboard();
            var fadeInAnimation = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDuration / 2)),
                EasingFunction = new Microsoft.UI.Xaml.Media.Animation.CubicEase 
                { 
                    EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseOut 
                }
            };
            
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(fadeInAnimation, CollapsedSidebar);
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(fadeInAnimation, "Opacity");
            fadeInStoryboard.Children.Add(fadeInAnimation);
            fadeInStoryboard.Begin();
            
            // Save state
            await SaveNavigationStateAsync();
        }

        private async System.Threading.Tasks.Task ExpandSidebar()
        {
            if (_isSidebarExpanded) return;

            _isSidebarExpanded = true;

            // Fade out collapsed sidebar
            var fadeOutStoryboard = new Microsoft.UI.Xaml.Media.Animation.Storyboard();
            var fadeOutAnimation = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDuration / 2)),
                EasingFunction = new Microsoft.UI.Xaml.Media.Animation.CubicEase 
                { 
                    EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseIn 
                }
            };
            
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(fadeOutAnimation, CollapsedSidebar);
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(fadeOutAnimation, "Opacity");
            fadeOutStoryboard.Children.Add(fadeOutAnimation);
            fadeOutStoryboard.Begin();
            
            await System.Threading.Tasks.Task.Delay(AnimationDuration / 2);
            
            // Switch visibility and update column width
            CollapsedSidebar.Visibility = Visibility.Collapsed;
            NavigationSidebar.Visibility = Visibility.Visible;
            NavigationSidebar.Opacity = 0.0;
            SidebarColumn.Width = new GridLength(250);

            // Fade in expanded sidebar
            var fadeInStoryboard = new Microsoft.UI.Xaml.Media.Animation.Storyboard();
            var fadeInAnimation = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDuration / 2)),
                EasingFunction = new Microsoft.UI.Xaml.Media.Animation.CubicEase 
                { 
                    EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseOut 
                }
            };
            
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(fadeInAnimation, NavigationSidebar);
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(fadeInAnimation, "Opacity");
            fadeInStoryboard.Children.Add(fadeInAnimation);
            fadeInStoryboard.Begin();

            // Save state
            await SaveNavigationStateAsync();
        }

        private async void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string moduleId)
            {
                _currentModule = moduleId;
                UpdateContentArea(moduleId);
                UpdateNavigationButtonStates(button);
                
                // Save navigation state
                await SaveNavigationStateAsync();
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
            HeaderContentTitle.Text = moduleId;  // Update header title too
            
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
