using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Odin___OpenSpec.Services;
using Odin___OpenSpec.Models;

namespace Odin___OpenSpec.Services
{
    /// <summary>
    /// Service for managing application themes and theme persistence
    /// </summary>
    public class ThemeService : IThemeService, INotifyPropertyChanged
    {
        private readonly ILogger<ThemeService> _logger;
        private readonly IDataContext _dataContext;
        private ApplicationTheme _currentTheme = ApplicationTheme.Light;
        private bool _followSystemTheme = true;
        private const int DefaultUserId = 1; // For now, use a default user ID

        public event PropertyChangedEventHandler? PropertyChanged;

        public ThemeService(ILogger<ThemeService> logger, IDataContext dataContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        public ApplicationTheme CurrentTheme
        {
            get => _currentTheme;
            private set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    OnPropertyChanged(nameof(CurrentTheme));
                    OnPropertyChanged(nameof(IsDarkTheme));
                    OnPropertyChanged(nameof(IsLightTheme));
                }
            }
        }

        public bool FollowSystemTheme
        {
            get => _followSystemTheme;
            private set
            {
                if (_followSystemTheme != value)
                {
                    _followSystemTheme = value;
                    OnPropertyChanged(nameof(FollowSystemTheme));
                }
            }
        }

        public bool IsDarkTheme => CurrentTheme == ApplicationTheme.Dark;
        public bool IsLightTheme => CurrentTheme == ApplicationTheme.Light;

        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing theme service");

                // Load theme preferences from database
                var themeState = await _dataContext.GetThemeStateAsync(DefaultUserId);
                if (themeState != null)
                {
                    var themeName = themeState.ThemeName;
                    if (themeName == "System")
                    {
                        FollowSystemTheme = true;
                    }
                    else
                    {
                        FollowSystemTheme = false;
                        CurrentTheme = themeName == "Dark" ? ApplicationTheme.Dark : ApplicationTheme.Light;
                    }
                }

                // Apply initial theme
                if (FollowSystemTheme)
                {
                    await DetectAndApplySystemThemeAsync();
                }
                else
                {
                    await ApplyThemeAsync(CurrentTheme);
                }

                _logger.LogInformation("Theme service initialized with theme: {Theme}, FollowSystem: {FollowSystem}", 
                    CurrentTheme, FollowSystemTheme);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize theme service");
                // Fallback to light theme
                await ApplyThemeAsync(ApplicationTheme.Light);
            }
        }

        public async Task SetThemeAsync(ApplicationTheme theme)
        {
            try
            {
                _logger.LogInformation("Setting theme to: {Theme}", theme);

                FollowSystemTheme = false;
                await ApplyThemeAsync(theme);
                await SaveThemeStateAsync();

                _logger.LogInformation("Theme successfully changed to: {Theme}", theme);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set theme to: {Theme}", theme);
                throw;
            }
        }

        public async Task SetFollowSystemThemeAsync(bool followSystem)
        {
            try
            {
                _logger.LogInformation("Setting follow system theme to: {FollowSystem}", followSystem);

                FollowSystemTheme = followSystem;

                if (followSystem)
                {
                    await DetectAndApplySystemThemeAsync();
                }

                await SaveThemeStateAsync();

                _logger.LogInformation("Follow system theme successfully set to: {FollowSystem}", followSystem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set follow system theme to: {FollowSystem}", followSystem);
                throw;
            }
        }

        public async Task ToggleThemeAsync()
        {
            var newTheme = CurrentTheme == ApplicationTheme.Light 
                ? ApplicationTheme.Dark 
                : ApplicationTheme.Light;

            await SetThemeAsync(newTheme);
        }

        private async Task DetectAndApplySystemThemeAsync()
        {
            try
            {
                // Get system theme from WinUI Application
                var systemTheme = Application.Current.RequestedTheme;
                var detectedTheme = systemTheme == ApplicationTheme.Dark 
                    ? ApplicationTheme.Dark 
                    : ApplicationTheme.Light;

                _logger.LogDebug("Detected system theme: {SystemTheme} -> {DetectedTheme}", systemTheme, detectedTheme);

                await ApplyThemeAsync(detectedTheme);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to detect system theme, falling back to light theme");
                await ApplyThemeAsync(ApplicationTheme.Light);
            }
        }

        private async Task ApplyThemeAsync(ApplicationTheme theme)
        {
            try
            {
                // Update current theme
                CurrentTheme = theme;

                // Apply theme to WinUI Application - this will affect the app-level theme
                Application.Current.RequestedTheme = theme;

                _logger.LogDebug("Applied theme {Theme} to application", theme);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to apply theme: {Theme}", theme);
                throw;
            }
        }

        private async Task SaveThemeStateAsync()
        {
            try
            {
                var themeName = FollowSystemTheme ? "System" : 
                    (CurrentTheme == ApplicationTheme.Dark ? "Dark" : "Light");

                var themeState = new ThemeState
                {
                    UserId = DefaultUserId,
                    ThemeName = themeName,
                    UpdatedDate = DateTime.UtcNow
                };

                await _dataContext.UpdateThemeStateAsync(themeState);
                _logger.LogDebug("Theme state saved: {Theme}, FollowSystem: {FollowSystem}", 
                    CurrentTheme, FollowSystemTheme);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save theme state");
                // Don't throw - theme changes should still work even if persistence fails
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}