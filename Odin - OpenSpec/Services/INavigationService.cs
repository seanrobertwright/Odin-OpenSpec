using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Odin___OpenSpec.Services
{
    /// <summary>
    /// Interface for navigation service providing module routing and state management
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Get all available navigation modules
        /// </summary>
        List<NavigationModule> GetModules();

        /// <summary>
        /// Navigate to a specific module
        /// </summary>
        Task<bool> NavigateToAsync(string moduleId, object? parameters = null);

        /// <summary>
        /// Get the currently active module
        /// </summary>
        NavigationModule? CurrentModule { get; }

        /// <summary>
        /// Check if a module can be navigated to
        /// </summary>
        bool CanNavigateTo(string moduleId);

        /// <summary>
        /// Go back to the previous module if possible
        /// </summary>
        Task<bool> GoBackAsync();

        /// <summary>
        /// Check if backward navigation is possible
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Get navigation history
        /// </summary>
        List<NavigationHistoryItem> GetHistory();

        /// <summary>
        /// Clear navigation history
        /// </summary>
        void ClearHistory();

        /// <summary>
        /// Set the expanded/collapsed state of navigation
        /// </summary>
        Task SetNavigationExpandedAsync(bool isExpanded);

        /// <summary>
        /// Get the current navigation expanded state
        /// </summary>
        bool IsNavigationExpanded { get; }

        /// <summary>
        /// Event fired when navigation occurs
        /// </summary>
        event EventHandler<NavigationEventArgs>? Navigated;

        /// <summary>
        /// Event fired when navigation state changes (expanded/collapsed)
        /// </summary>
        event EventHandler<bool>? NavigationStateChanged;
    }

    /// <summary>
    /// Represents a navigation module
    /// </summary>
    public class NavigationModule
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public int Order { get; set; } = 0;
        public Type? ViewType { get; set; }
        public Type? ViewModelType { get; set; }
    }

    /// <summary>
    /// Represents a navigation history item
    /// </summary>
    public class NavigationHistoryItem
    {
        public string ModuleId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public object? Parameters { get; set; }
    }

    /// <summary>
    /// Event arguments for navigation events
    /// </summary>
    public class NavigationEventArgs : EventArgs
    {
        public string FromModuleId { get; set; } = string.Empty;
        public string ToModuleId { get; set; } = string.Empty;
        public object? Parameters { get; set; }
        public bool Success { get; set; } = true;
    }
}