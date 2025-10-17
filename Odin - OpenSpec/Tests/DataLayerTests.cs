using Microsoft.Extensions.Logging;
using Odin___OpenSpec.Models;
using Odin___OpenSpec.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Odin___OpenSpec.Tests
{
    /// <summary>
    /// Basic tests for data layer operations
    /// Note: This is a simple test class for validation. In production, use a proper testing framework.
    /// </summary>
    public static class DataLayerTests
    {
        /// <summary>
        /// Run basic CRUD tests on the data layer
        /// </summary>
        public static async Task<bool> RunBasicTests()
        {
            try
            {
                // Create a logger for testing
                using var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Information));
                var logger = loggerFactory.CreateLogger<SqliteDataContext>();

                // Create test database in temp folder
                var testDbPath = Path.Combine(Path.GetTempPath(), "odin_test.db");
                if (File.Exists(testDbPath))
                    File.Delete(testDbPath);

                var dataContext = new SqliteDataContext(logger);
                
                // Initialize database
                await dataContext.InitializeAsync();
                
                // Test User operations
                var user = new User
                {
                    Name = "Test User",
                    PhotoPath = "/path/to/photo.jpg",
                    IsActive = true
                };

                // Create user
                await dataContext.CreateUserAsync(user);
                if (user.Id <= 0)
                    throw new Exception("User creation failed - ID not set");

                // Get user
                var retrievedUser = await dataContext.GetUserAsync(user.Id);
                if (retrievedUser == null || retrievedUser.Name != "Test User")
                    throw new Exception("User retrieval failed");

                // Update user
                retrievedUser.Name = "Updated Test User";
                await dataContext.UpdateUserAsync(retrievedUser);

                // Test UserPreference operations
                var preference = new UserPreference
                {
                    UserId = user.Id,
                    Key = "TestKey",
                    Value = "TestValue",
                    DataType = "string"
                };

                await dataContext.SetUserPreferenceAsync(preference);
                var retrievedPreference = await dataContext.GetUserPreferenceAsync(user.Id, "TestKey");
                if (retrievedPreference == null || retrievedPreference.Value != "TestValue")
                    throw new Exception("UserPreference operations failed");

                // Test NavigationState operations
                var navState = new NavigationState
                {
                    UserId = user.Id,
                    IsExpanded = true,
                    LastModule = "Calendar"
                };

                await dataContext.UpdateNavigationStateAsync(navState);
                var retrievedNavState = await dataContext.GetNavigationStateAsync(user.Id);
                if (retrievedNavState == null || !retrievedNavState.IsExpanded)
                    throw new Exception("NavigationState operations failed");

                // Test ThemeState operations
                var themeState = new ThemeState
                {
                    UserId = user.Id,
                    ThemeName = "Dark",
                    CustomSettings = "{\"accentColor\": \"blue\"}"
                };

                await dataContext.UpdateThemeStateAsync(themeState);
                var retrievedThemeState = await dataContext.GetThemeStateAsync(user.Id);
                if (retrievedThemeState == null || retrievedThemeState.ThemeName != "Dark")
                    throw new Exception("ThemeState operations failed");

                // Cleanup test database
                if (File.Exists(testDbPath))
                    File.Delete(testDbPath);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Data layer test failed: {ex.Message}");
                return false;
            }
        }
    }
}