using SQLite;
using Odin___OpenSpec.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Odin___OpenSpec.Services
{
    /// <summary>
    /// SQLite implementation of the data context interface
    /// </summary>
    public class SqliteDataContext : IDataContext
    {
        private readonly SQLiteAsyncConnection _database;
        private readonly ILogger<SqliteDataContext> _logger;

        public SqliteDataContext(ILogger<SqliteDataContext> logger)
        {
            _logger = logger;

            // Create database path in user's app data folder
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbFolder = Path.Combine(appDataPath, "OdinOpenSpec");
            Directory.CreateDirectory(dbFolder);
            var dbPath = Path.Combine(dbFolder, "odin.db");

            _logger.LogInformation("Initializing SQLite database at: {DbPath}", dbPath);
            _database = new SQLiteAsyncConnection(dbPath);
        }

        /// <summary>
        /// Initialize the database and create tables if they don't exist
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                await _database.CreateTableAsync<User>();
                await _database.CreateTableAsync<UserPreference>();
                await _database.CreateTableAsync<NavigationState>();
                await _database.CreateTableAsync<ThemeState>();

                _logger.LogInformation("Database tables created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize database tables");
                throw;
            }
        }

        /// <summary>
        /// Get all active users
        /// </summary>
        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                return await _database.Table<User>()
                    .Where(u => u.IsActive)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get users");
                throw;
            }
        }

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        public async Task<User?> GetUserAsync(int id)
        {
            try
            {
                return await _database.Table<User>()
                    .Where(u => u.Id == id && u.IsActive)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user with ID {UserId}", id);
                throw;
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        public async Task<int> CreateUserAsync(User user)
        {
            try
            {
                user.CreatedDate = DateTime.UtcNow;
                return await _database.InsertAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user: {UserName}", user.Name);
                throw;
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        public async Task<int> UpdateUserAsync(User user)
        {
            try
            {
                return await _database.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user with ID {UserId}", user.Id);
                throw;
            }
        }

        /// <summary>
        /// Delete a user (mark as inactive)
        /// </summary>
        public async Task<int> DeleteUserAsync(int id)
        {
            try
            {
                var user = await GetUserAsync(id);
                if (user != null)
                {
                    user.IsActive = false;
                    return await UpdateUserAsync(user);
                }
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user with ID {UserId}", id);
                throw;
            }
        }

        /// <summary>
        /// Get user preferences for a specific user
        /// </summary>
        public async Task<List<UserPreference>> GetUserPreferencesAsync(int userId)
        {
            try
            {
                return await _database.Table<UserPreference>()
                    .Where(p => p.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get preferences for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get a specific user preference
        /// </summary>
        public async Task<UserPreference?> GetUserPreferenceAsync(int userId, string key)
        {
            try
            {
                return await _database.Table<UserPreference>()
                    .Where(p => p.UserId == userId && p.Key == key)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get preference {Key} for user {UserId}", key, userId);
                throw;
            }
        }

        /// <summary>
        /// Set a user preference
        /// </summary>
        public async Task<int> SetUserPreferenceAsync(UserPreference preference)
        {
            try
            {
                preference.UpdatedDate = DateTime.UtcNow;

                var existing = await GetUserPreferenceAsync(preference.UserId, preference.Key);
                if (existing != null)
                {
                    preference.Id = existing.Id;
                    return await _database.UpdateAsync(preference);
                }
                else
                {
                    return await _database.InsertAsync(preference);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set preference {Key} for user {UserId}", preference.Key, preference.UserId);
                throw;
            }
        }

        /// <summary>
        /// Get navigation state for a user
        /// </summary>
        public async Task<NavigationState?> GetNavigationStateAsync(int userId)
        {
            try
            {
                return await _database.Table<NavigationState>()
                    .Where(n => n.UserId == userId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get navigation state for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Update navigation state for a user
        /// </summary>
        public async Task<int> UpdateNavigationStateAsync(NavigationState state)
        {
            try
            {
                state.UpdatedDate = DateTime.UtcNow;

                var existing = await GetNavigationStateAsync(state.UserId);
                if (existing != null)
                {
                    return await _database.UpdateAsync(state);
                }
                else
                {
                    return await _database.InsertAsync(state);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update navigation state for user {UserId}", state.UserId);
                throw;
            }
        }

        /// <summary>
        /// Get theme state for a user
        /// </summary>
        public async Task<ThemeState?> GetThemeStateAsync(int userId)
        {
            try
            {
                return await _database.Table<ThemeState>()
                    .Where(t => t.UserId == userId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get theme state for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Update theme state for a user
        /// </summary>
        public async Task<int> UpdateThemeStateAsync(ThemeState state)
        {
            try
            {
                state.UpdatedDate = DateTime.UtcNow;

                var existing = await GetThemeStateAsync(state.UserId);
                if (existing != null)
                {
                    return await _database.UpdateAsync(state);
                }
                else
                {
                    return await _database.InsertAsync(state);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update theme state for user {UserId}", state.UserId);
                throw;
            }
        }
    }
}