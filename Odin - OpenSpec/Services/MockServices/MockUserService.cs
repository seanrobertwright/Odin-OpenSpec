using Odin___OpenSpec.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Odin___OpenSpec.Services.MockServices
{
    /// <summary>
    /// Mock implementation of IUserService for testing and development
    /// </summary>
    public class MockUserService : IUserService
    {
        private readonly List<User> _users;
        private readonly Dictionary<int, Dictionary<string, object>> _userPreferences;
        private User? _currentUser;

        public User? CurrentUser => _currentUser;

        public event EventHandler<UserChangedEventArgs>? CurrentUserChanged;
        public event EventHandler<User>? UserProfileUpdated;
        public event EventHandler<UserPreferenceChangedEventArgs>? UserPreferenceChanged;

        public MockUserService()
        {
            _users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Name = "Default User",
                    PhotoPath = null,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-30)
                }
            };

            _userPreferences = new Dictionary<int, Dictionary<string, object>>();
            _currentUser = _users.First();
        }

        public async Task<List<User>> GetUsersAsync()
        {
            await Task.Delay(10);
            return _users.Where(u => u.IsActive).ToList();
        }

        public async Task<bool> SetCurrentUserAsync(int userId)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId && u.IsActive);
            if (user == null) return false;

            var previousUser = _currentUser;
            _currentUser = user;

            await Task.Delay(10);

            var eventArgs = new UserChangedEventArgs
            {
                PreviousUser = previousUser,
                NewUser = user,
                WasSystemSwitch = false
            };

            CurrentUserChanged?.Invoke(this, eventArgs);
            System.Diagnostics.Debug.WriteLine($"Mock: Current user changed to {user.Name}");

            return true;
        }

        public async Task<User?> CreateUserAsync(string name, string? photoPath = null)
        {
            await Task.Delay(50);

            var user = new User
            {
                Id = _users.Count + 1,
                Name = name,
                PhotoPath = photoPath,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _users.Add(user);
            System.Diagnostics.Debug.WriteLine($"Mock: Created user {name}");

            return user;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser == null) return false;

            existingUser.Name = user.Name;
            existingUser.PhotoPath = user.PhotoPath;

            await Task.Delay(10);

            UserProfileUpdated?.Invoke(this, existingUser);
            System.Diagnostics.Debug.WriteLine($"Mock: Updated user {user.Name}");

            return true;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return false;

            user.IsActive = false;
            await Task.Delay(10);

            System.Diagnostics.Debug.WriteLine($"Mock: Deleted user {user.Name}");

            return true;
        }

        public async Task<User?> GetUserAsync(int userId)
        {
            await Task.Delay(5);
            return _users.FirstOrDefault(u => u.Id == userId && u.IsActive);
        }

        public async Task<bool> SetUserPhotoAsync(int userId, string photoPath)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return false;

            user.PhotoPath = photoPath;
            await Task.Delay(10);

            UserProfileUpdated?.Invoke(this, user);
            System.Diagnostics.Debug.WriteLine($"Mock: Set photo for user {user.Name}");

            return true;
        }

        public async Task<Dictionary<string, object>> GetUserPreferencesAsync(int userId)
        {
            await Task.Delay(10);

            if (!_userPreferences.ContainsKey(userId))
            {
                _userPreferences[userId] = new Dictionary<string, object>
                {
                    { "Theme", "Light" },
                    { "Language", "en-US" },
                    { "NavigationExpanded", false },
                    { "WeatherLocation", "Seattle, WA" }
                };
            }

            return new Dictionary<string, object>(_userPreferences[userId]);
        }

        public async Task<bool> SetUserPreferenceAsync(int userId, string key, object value)
        {
            await Task.Delay(5);

            if (!_userPreferences.ContainsKey(userId))
                _userPreferences[userId] = new Dictionary<string, object>();

            var oldValue = _userPreferences[userId].ContainsKey(key) 
                ? _userPreferences[userId][key] 
                : null;

            _userPreferences[userId][key] = value;

            var eventArgs = new UserPreferenceChangedEventArgs
            {
                UserId = userId,
                Key = key,
                OldValue = oldValue,
                NewValue = value
            };

            UserPreferenceChanged?.Invoke(this, eventArgs);
            System.Diagnostics.Debug.WriteLine($"Mock: Set preference {key} = {value} for user {userId}");

            return true;
        }

        public async Task<T?> GetUserPreferenceAsync<T>(int userId, string key)
        {
            await Task.Delay(5);

            if (!_userPreferences.ContainsKey(userId) || 
                !_userPreferences[userId].ContainsKey(key))
                return default(T);

            var value = _userPreferences[userId][key];
            if (value is T typedValue)
                return typedValue;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        public async Task<User> InitializeDefaultUserAsync()
        {
            await Task.Delay(10);

            if (_users.Any(u => u.IsActive))
                return _users.First(u => u.IsActive);

            var defaultUser = await CreateUserAsync("Default User");
            await SetCurrentUserAsync(defaultUser!.Id);

            System.Diagnostics.Debug.WriteLine("Mock: Initialized default user");
            return defaultUser!;
        }

        public async Task<bool> ValidateUserAsync(int userId, string? credentials = null)
        {
            // Mock validation - always return true for now
            await Task.Delay(100);
            
            var user = _users.FirstOrDefault(u => u.Id == userId && u.IsActive);
            return user != null;
        }

        public async Task<UserStatistics> GetUserStatisticsAsync(int userId)
        {
            await Task.Delay(50);

            return new UserStatistics
            {
                UserId = userId,
                LastLoginDate = DateTime.UtcNow.AddHours(-2),
                TotalUsageTime = TimeSpan.FromHours(45),
                SessionCount = 23,
                MostUsedModule = "calendar",
                ModuleUsageCount = new Dictionary<string, int>
                {
                    { "calendar", 15 },
                    { "tasks", 12 },
                    { "weather", 8 },
                    { "music", 5 }
                },
                ProfileCreatedDate = _users.FirstOrDefault(u => u.Id == userId)?.CreatedDate ?? DateTime.UtcNow
            };
        }
    }
}