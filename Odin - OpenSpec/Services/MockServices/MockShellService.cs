using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

namespace Odin___OpenSpec.Services.MockServices
{
    /// <summary>
    /// Mock implementation of IShellService for testing and development
    /// </summary>
    public class MockShellService : IShellService
    {
        private bool _isNavigationVisible = true;
        private bool _isDialogOpen = false;

        public Window? MainWindow { get; private set; }
        public bool IsNavigationVisible => _isNavigationVisible;

        public event EventHandler<bool>? NavigationVisibilityChanged;
        public event EventHandler<bool>? DialogStateChanged;

        public async Task InitializeAsync()
        {
            // Mock initialization
            await Task.Delay(100);
            // In a real implementation, this would set up the main window
        }

        public void SetFullscreen(bool isFullscreen)
        {
            // Mock fullscreen toggle
            System.Diagnostics.Debug.WriteLine($"Mock: Setting fullscreen to {isFullscreen}");
        }

        public void SetNavigationVisible(bool isVisible)
        {
            if (_isNavigationVisible != isVisible)
            {
                _isNavigationVisible = isVisible;
                NavigationVisibilityChanged?.Invoke(this, isVisible);
                System.Diagnostics.Debug.WriteLine($"Mock: Navigation visibility set to {isVisible}");
            }
        }

        public async Task<TResult?> ShowDialogAsync<TResult>(UIElement dialogContent)
        {
            _isDialogOpen = true;
            DialogStateChanged?.Invoke(this, true);
            
            // Mock dialog display
            await Task.Delay(500);
            System.Diagnostics.Debug.WriteLine("Mock: Showing dialog");
            
            return default(TResult);
        }

        public void CloseDialog()
        {
            if (_isDialogOpen)
            {
                _isDialogOpen = false;
                DialogStateChanged?.Invoke(this, false);
                System.Diagnostics.Debug.WriteLine("Mock: Closing dialog");
            }
        }

        public void SetStatusMessage(string message, bool isError = false)
        {
            var type = isError ? "ERROR" : "INFO";
            System.Diagnostics.Debug.WriteLine($"Mock Status [{type}]: {message}");
        }
    }
}