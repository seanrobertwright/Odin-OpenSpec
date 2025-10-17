using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace Odin___OpenSpec.Services
{
    /// <summary>
    /// Interface for shell service providing window management and layout orchestration
    /// </summary>
    public interface IShellService
    {
        /// <summary>
        /// Initialize the shell and main window
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Set the main window to fullscreen mode
        /// </summary>
        void SetFullscreen(bool isFullscreen);

        /// <summary>
        /// Show or hide the navigation panel
        /// </summary>
        void SetNavigationVisible(bool isVisible);

        /// <summary>
        /// Get the current navigation panel visibility state
        /// </summary>
        bool IsNavigationVisible { get; }

        /// <summary>
        /// Show a modal dialog over the main content
        /// </summary>
        Task<TResult?> ShowDialogAsync<TResult>(UIElement dialogContent);

        /// <summary>
        /// Close any open modal dialog
        /// </summary>
        void CloseDialog();

        /// <summary>
        /// Update the shell's status message
        /// </summary>
        void SetStatusMessage(string message, bool isError = false);

        /// <summary>
        /// Get the main window instance
        /// </summary>
        Window? MainWindow { get; }

        /// <summary>
        /// Event fired when navigation visibility changes
        /// </summary>
        event System.EventHandler<bool>? NavigationVisibilityChanged;

        /// <summary>
        /// Event fired when a modal dialog is opened or closed
        /// </summary>
        event System.EventHandler<bool>? DialogStateChanged;
    }
}