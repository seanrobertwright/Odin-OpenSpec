using Odin___OpenSpec.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Odin___OpenSpec.Services
{
    /// <summary>
    /// Interface for data context providing access to all database operations
    /// </summary>
    public interface IDataContext
    {
        /// <summary>
        /// Initialize the database and create tables if they don't exist
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Get all active users
        /// </summary>
        Task<List<User>> GetUsersAsync();

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        Task<User?> GetUserAsync(int id);

        /// <summary>
        /// Create a new user
        /// </summary>
        Task<int> CreateUserAsync(User user);

        /// <summary>
        /// Update an existing user
        /// </summary>
        Task<int> UpdateUserAsync(User user);

        /// <summary>
        /// Delete a user (mark as inactive)
        /// </summary>
        Task<int> DeleteUserAsync(int id);

        /// <summary>
        /// Get user preferences for a specific user
        /// </summary>
        Task<List<UserPreference>> GetUserPreferencesAsync(int userId);

        /// <summary>
        /// Get a specific user preference
        /// </summary>
        Task<UserPreference?> GetUserPreferenceAsync(int userId, string key);

        /// <summary>
        /// Set a user preference
        /// </summary>
        Task<int> SetUserPreferenceAsync(UserPreference preference);

        /// <summary>
        /// Get navigation state for a user
        /// </summary>
        Task<NavigationState?> GetNavigationStateAsync(int userId);

        /// <summary>
        /// Update navigation state for a user
        /// </summary>
        Task<int> UpdateNavigationStateAsync(NavigationState state);

        /// <summary>
        /// Get theme state for a user
        /// </summary>
        Task<ThemeState?> GetThemeStateAsync(int userId);

        /// <summary>
        /// Update theme state for a user
        /// </summary>
        Task<int> UpdateThemeStateAsync(ThemeState state);
    }
}