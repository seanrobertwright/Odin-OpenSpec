using Odin___OpenSpec.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Odin___OpenSpec.Services
{
    /// <summary>
    /// Interface for user service providing profile management and authentication
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Get all active users
        /// </summary>
        Task<List<User>> GetUsersAsync();

        /// <summary>
        /// Get the currently active/selected user
        /// </summary>
        User? CurrentUser { get; }

        /// <summary>
        /// Set the current active user
        /// </summary>
        Task<bool> SetCurrentUserAsync(int userId);

        /// <summary>
        /// Create a new user profile
        /// </summary>
        Task<User?> CreateUserAsync(string name, string? photoPath = null);

        /// <summary>
        /// Update an existing user profile
        /// </summary>
        Task<bool> UpdateUserAsync(User user);

        /// <summary>
        /// Delete a user profile (mark as inactive)
        /// </summary>
        Task<bool> DeleteUserAsync(int userId);

        /// <summary>
        /// Get user profile by ID
        /// </summary>
        Task<User?> GetUserAsync(int userId);

        /// <summary>
        /// Set user photo
        /// </summary>
        Task<bool> SetUserPhotoAsync(int userId, string photoPath);

        /// <summary>
        /// Get user preferences
        /// </summary>
        Task<Dictionary<string, object>> GetUserPreferencesAsync(int userId);

        /// <summary>
        /// Set a user preference
        /// </summary>
        Task<bool> SetUserPreferenceAsync(int userId, string key, object value);

        /// <summary>
        /// Get a specific user preference
        /// </summary>
        Task<T?> GetUserPreferenceAsync<T>(int userId, string key);

        /// <summary>
        /// Initialize default user if none exists
        /// </summary>
        Task<User> InitializeDefaultUserAsync();

        /// <summary>
        /// Validate user credentials (for future authentication)
        /// </summary>
        Task<bool> ValidateUserAsync(int userId, string? credentials = null);

        /// <summary>
        /// Get user statistics
        /// </summary>
        Task<UserStatistics> GetUserStatisticsAsync(int userId);

        /// <summary>
        /// Event fired when current user changes
        /// </summary>
        event EventHandler<UserChangedEventArgs>? CurrentUserChanged;

        /// <summary>
        /// Event fired when user profile is updated
        /// </summary>
        event EventHandler<User>? UserProfileUpdated;

        /// <summary>
        /// Event fired when user preferences change
        /// </summary>
        event EventHandler<UserPreferenceChangedEventArgs>? UserPreferenceChanged;
    }

    /// <summary>
    /// User statistics information
    /// </summary>
    public class UserStatistics
    {
        public int UserId { get; set; }
        public DateTime LastLoginDate { get; set; }
        public TimeSpan TotalUsageTime { get; set; }
        public int SessionCount { get; set; }
        public string MostUsedModule { get; set; } = string.Empty;
        public Dictionary<string, int> ModuleUsageCount { get; set; } = new();
        public DateTime ProfileCreatedDate { get; set; }
    }

    /// <summary>
    /// Event arguments for user change events
    /// </summary>
    public class UserChangedEventArgs : EventArgs
    {
        public User? PreviousUser { get; set; }
        public User? NewUser { get; set; }
        public bool WasSystemSwitch { get; set; } = false;
    }

    /// <summary>
    /// Event arguments for user preference change events
    /// </summary>
    public class UserPreferenceChangedEventArgs : EventArgs
    {
        public int UserId { get; set; }
        public string Key { get; set; } = string.Empty;
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
    }
}