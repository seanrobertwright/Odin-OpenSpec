using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Odin___OpenSpec.Services
{
    /// <summary>
    /// Interface for theme service providing theme switching and persistence
    /// </summary>
    public interface IThemeService : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the current application theme
        /// </summary>
        ApplicationTheme CurrentTheme { get; }

        /// <summary>
        /// Gets whether the application follows the system theme
        /// </summary>
        bool FollowSystemTheme { get; }

        /// <summary>
        /// Gets whether the current theme is dark
        /// </summary>
        bool IsDarkTheme { get; }

        /// <summary>
        /// Gets whether the current theme is light
        /// </summary>
        bool IsLightTheme { get; }

        /// <summary>
        /// Initializes the theme service and applies saved theme preferences
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Sets a specific theme and disables system theme following
        /// </summary>
        Task SetThemeAsync(ApplicationTheme theme);

        /// <summary>
        /// Sets whether to follow the system theme
        /// </summary>
        Task SetFollowSystemThemeAsync(bool followSystem);

        /// <summary>
        /// Toggles between light and dark themes
        /// </summary>
        Task ToggleThemeAsync();
    }
}