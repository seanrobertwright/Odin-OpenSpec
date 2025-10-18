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

        /// <summary>
        /// Restores user session after app initialization - called from App.xaml.cs
        /// </summary>
        public async System.Threading.Tasks.Task RestoreUserSessionAsync()
        {
            try
            {
                var app = Application.Current as App;
                var userService = app?.ServiceProvider?.GetService(typeof(IUserService)) as IUserService;

                if (userService?.CurrentUser != null)
                {
                    // Update UI with current user
                    await UpdateProfileDisplayAsync(userService.CurrentUser);
                    
                    // Restore navigation state for this user
                    await LoadNavigationStateAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring user session: {ex.Message}");
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
                    if (themeTag == "Default")
                    {
                        // System/Default theme - follow system preference
                        await _themeService.SetFollowSystemThemeAsync(true);
                    }
                    else
                    {
                        // Specific theme selected
                        Microsoft.UI.Xaml.ApplicationTheme appTheme = themeTag switch
                        {
                            "Light" => Microsoft.UI.Xaml.ApplicationTheme.Light,
                            "Dark" => Microsoft.UI.Xaml.ApplicationTheme.Dark,
                            _ => Microsoft.UI.Xaml.ApplicationTheme.Light
                        };
                
                        await _themeService.SetThemeAsync(appTheme);
                    }
                    
                    // Force refresh of current content to apply theme
                    UpdateContentArea(_currentModule);
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
            // Update header title
            HeaderContentTitle.Text = moduleId;
            
            // Navigate to appropriate view
            switch (moduleId)
            {
                case "Home":
                    ContentFrame.Content = CreateHomePlaceholder();
                    break;
                case "Calendar":
                    ContentFrame.Content = new Views.CalendarView();
                    break;
                case "Tasks":
                    ContentFrame.Content = CreatePlaceholder("Tasks", "Track your to-do lists and tasks.");
                    break;
                case "Grocery":
                    ContentFrame.Content = CreatePlaceholder("Grocery", "Manage your grocery lists and shopping needs.");
                    break;
                case "Weather":
                    ContentFrame.Content = CreatePlaceholder("Weather", "Check current weather conditions and forecasts.");
                    break;
                case "Music":
                    ContentFrame.Content = CreatePlaceholder("Music", "Control your music playback and playlists.");
                    break;
                case "Settings":
                    ContentFrame.Content = CreatePlaceholder("Settings", "Configure application preferences and settings.");
                    break;
                default:
                    ContentFrame.Content = CreatePlaceholder("Unknown", "Module not found.");
                    break;
            }
        }

        private UIElement CreateHomePlaceholder()
        {
            var stackPanel = new StackPanel
            {
                Spacing = 24,
                Padding = new Thickness(24)
            };

            var title = new TextBlock
            {
                Text = "Welcome to Odin - OpenSpec",
                FontSize = 32,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold
            };
            stackPanel.Children.Add(title);

            var subtitle = new TextBlock
            {
                Text = "Your personal productivity hub",
                FontSize = 18,
                Opacity = 0.7
            };
            stackPanel.Children.Add(subtitle);

            var description = new TextBlock
            {
                Text = "Select a module from the navigation sidebar to get started.\n\nAvailable modules:\n• Calendar - View and manage events\n• Tasks - Track your to-do lists\n• Settings - Configure preferences",
                FontSize = 16,
                Opacity = 0.8,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Margin = new Thickness(0, 16, 0, 0)
            };
            stackPanel.Children.Add(description);

            return stackPanel;
        }

        private UIElement CreatePlaceholder(string title, string message)
        {
            var stackPanel = new StackPanel
            {
                Spacing = 16,
                Padding = new Thickness(24),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var titleText = new TextBlock
            {
                Text = title,
                FontSize = 28,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stackPanel.Children.Add(titleText);

            var messageText = new TextBlock
            {
                Text = message + "\n\nThis module is coming soon!",
                FontSize = 16,
                Opacity = 0.7,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = Microsoft.UI.Xaml.TextAlignment.Center
            };
            stackPanel.Children.Add(messageText);

            return stackPanel;
        }

        private async void ProfileSwitcherButton_Click(object sender, RoutedEventArgs e)
        {
            // This method is no longer needed - flyout handles the click
            // Keeping for backward compatibility but can be removed
        }

        private async void ProfileSwitcherFlyout_Opening(object sender, object e)
        {
            try
            {
                var app = Application.Current as App;
                var userService = app?.ServiceProvider?.GetService(typeof(IUserService)) as IUserService;

                if (userService == null) return;

                // Get all users
                var users = await userService.GetUsersAsync();
                var currentUserId = userService.CurrentUser?.Id ?? 0;

                // Remove old dynamic profile items (items between the two separators)
                var itemsToRemove = new List<MenuFlyoutItemBase>();
                var startRemoving = false;
                
                foreach (var item in ProfileSwitcherFlyout.Items)
                {
                    if (item == ProfileListSeparator)
                    {
                        startRemoving = true;
                        continue;
                    }
                    
                    // Stop at the second separator (before Edit/Delete)
                    if (startRemoving && item is MenuFlyoutSeparator)
                    {
                        break;
                    }
                    
                    if (startRemoving)
                    {
                        itemsToRemove.Add(item);
                    }
                }

                foreach (var item in itemsToRemove)
                {
                    ProfileSwitcherFlyout.Items.Remove(item);
                }

                // Show/hide separator based on whether we have users
                ProfileListSeparator.Visibility = users.Any() ? Visibility.Visible : Visibility.Collapsed;

                // Add profile items
                var insertIndex = ProfileSwitcherFlyout.Items.IndexOf(ProfileListSeparator) + 1;
                
                foreach (var user in users)
                {
                    var menuItem = new MenuFlyoutItem
                    {
                        Text = user.Name,
                        Tag = user.Id
                    };

                    // Add checkmark icon if this is the current user
                    if (user.Id == currentUserId)
                    {
                        menuItem.Icon = new FontIcon { Glyph = "\uE73E" }; // Checkmark
                    }

                    menuItem.Click += ProfileMenuItem_Click;
                    ProfileSwitcherFlyout.Items.Insert(insertIndex++, menuItem);
                }

                // Enable/disable Edit and Delete based on whether there's a current user
                EditProfileMenuItem.IsEnabled = currentUserId > 0;
                DeleteProfileMenuItem.IsEnabled = currentUserId > 0 && users.Count() > 1; // Don't allow deleting last profile
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error populating profile menu: {ex.Message}");
            }
        }

        private async void ProfileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuFlyoutItem menuItem && menuItem.Tag is int userId)
                {
                    var app = Application.Current as App;
                    var userService = app?.ServiceProvider?.GetService(typeof(IUserService)) as IUserService;

                    if (userService == null) return;

                    // Don't switch if already current user
                    if (userService.CurrentUser?.Id == userId)
                        return;

                    // Save current user's navigation state before switching
                    await SaveNavigationStateAsync();

                    // Switch to the selected user
                    await userService.SetCurrentUserAsync(userId);
                    
                    var user = userService.CurrentUser;
                    if (user != null)
                    {
                        // Update profile display
                        await UpdateProfileDisplayAsync(user);
                        
                        // Restore navigation state for new user
                        await LoadNavigationStateAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error switching profile: {ex.Message}");
                
                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to switch profile: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private async void CreateProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create and show profile dialog
                var dialog = new Views.CreateProfileDialog
                {
                    XamlRoot = this.Content.XamlRoot
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    // Get service for creating user
                    var app = Application.Current as App;
                    var userService = app?.ServiceProvider?.GetService(typeof(IUserService)) as IUserService;

                    if (userService != null)
                    {
                        // Create the profile
                        var user = await userService.CreateUserAsync(dialog.UserName, dialog.PhotoPath);

                        if (user != null)
                        {
                            // Set as current user
                            await userService.SetCurrentUserAsync(user.Id);

                            // Update UI
                            await UpdateProfileDisplayAsync(user);

                            // Show success message
                            var successDialog = new ContentDialog
                            {
                                Title = "Profile Created",
                                Content = $"Welcome, {dialog.UserName}!",
                                CloseButtonText = "OK",
                                XamlRoot = this.Content.XamlRoot
                            };
                            await successDialog.ShowAsync();
                        }
                        else
                        {
                            // Show error
                            var errorDialog = new ContentDialog
                            {
                                Title = "Error",
                                Content = "Failed to create profile. Please try again.",
                                CloseButtonText = "OK",
                                XamlRoot = this.Content.XamlRoot
                            };
                            await errorDialog.ShowAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating profile: {ex.Message}");
                
                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"An error occurred: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private async void EditProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var app = Application.Current as App;
                var userService = app?.ServiceProvider?.GetService(typeof(IUserService)) as IUserService;

                if (userService?.CurrentUser == null)
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = "No active profile to edit.",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                var currentUser = userService.CurrentUser;
                var dialog = new Views.EditProfileDialog(currentUser)
                {
                    XamlRoot = this.Content.XamlRoot
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    // Update user
                    currentUser.Name = dialog.UserName;
                    
                    if (dialog.PhotoChanged)
                    {
                        await userService.SetUserPhotoAsync(currentUser.Id, dialog.PhotoPath ?? string.Empty);
                        currentUser.PhotoPath = dialog.PhotoPath;
                    }
                    
                    await userService.UpdateUserAsync(currentUser);

                    // Update UI
                    await UpdateProfileDisplayAsync(currentUser);

                    // Show success message
                    var successDialog = new ContentDialog
                    {
                        Title = "Profile Updated",
                        Content = "Your profile has been updated successfully.",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await successDialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error editing profile: {ex.Message}");
                
                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"An error occurred: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private async void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var app = Application.Current as App;
                var userService = app?.ServiceProvider?.GetService(typeof(IUserService)) as IUserService;

                if (userService?.CurrentUser == null)
                {
                    return;
                }

                // Confirm deletion
                var confirmDialog = new ContentDialog
                {
                    Title = "Delete Profile",
                    Content = $"Are you sure you want to delete the profile '{userService.CurrentUser.Name}'?\n\nThis action cannot be undone.",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.Content.XamlRoot
                };

                var result = await confirmDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    var userIdToDelete = userService.CurrentUser.Id;
                    var photoPath = userService.CurrentUser.PhotoPath;
                    
                    // Delete the profile
                    await userService.DeleteUserAsync(userIdToDelete);

                    // Delete photo if exists
                    if (!string.IsNullOrEmpty(photoPath) && File.Exists(photoPath))
                    {
                        try
                        {
                            File.Delete(photoPath);
                        }
                        catch
                        {
                            // Ignore photo deletion errors
                        }
                    }

                    // Switch to another profile or create default
                    var users = await userService.GetUsersAsync();
                    if (users.Any())
                    {
                        var nextUser = users.First();
                        await userService.SetCurrentUserAsync(nextUser.Id);
                        await UpdateProfileDisplayAsync(nextUser);
                    }
                    else
                    {
                        // No users left - reset to default
                        UserDisplayName.Text = "User";
                        UserProfilePicture.Initials = "U";
                        UserProfilePicture.DisplayName = "User";
                        UserProfilePicture.ProfilePicture = null;
                    }

                    // Show success message
                    var successDialog = new ContentDialog
                    {
                        Title = "Profile Deleted",
                        Content = "The profile has been deleted successfully.",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await successDialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting profile: {ex.Message}");
                
                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"An error occurred: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private async System.Threading.Tasks.Task UpdateProfileDisplayAsync(Models.User user)
        {
            UserDisplayName.Text = user.Name;
            
            // Load photo if available
            if (!string.IsNullOrEmpty(user.PhotoPath) && File.Exists(user.PhotoPath))
            {
                try
                {
                    var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(user.PhotoPath);
                    using var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                    var bitmapImage = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
                    await bitmapImage.SetSourceAsync(stream);
                    UserProfilePicture.ProfilePicture = bitmapImage;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load profile photo: {ex.Message}");
                    // Fall back to initials
                    UserProfilePicture.Initials = GetInitials(user.Name);
                }
            }
            else
            {
                // Clear photo and show initials
                UserProfilePicture.ProfilePicture = null;
                UserProfilePicture.Initials = GetInitials(user.Name);
            }
            
            UserProfilePicture.DisplayName = user.Name;
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "?";

            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

            return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
        }
    }
}
