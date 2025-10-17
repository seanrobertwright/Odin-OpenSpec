using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Odin___OpenSpec.Services.MockServices
{
    /// <summary>
    /// Mock implementation of INavigationService for testing and development
    /// </summary>
    public class MockNavigationService : INavigationService
    {
        private readonly List<NavigationModule> _modules;
        private readonly List<NavigationHistoryItem> _history;
        private NavigationModule? _currentModule;
        private bool _isNavigationExpanded = false;

        public NavigationModule? CurrentModule => _currentModule;
        public bool CanGoBack => _history.Count > 1;
        public bool IsNavigationExpanded => _isNavigationExpanded;

        public event EventHandler<NavigationEventArgs>? Navigated;
        public event EventHandler<bool>? NavigationStateChanged;

        public MockNavigationService()
        {
            _modules = new List<NavigationModule>
            {
                new NavigationModule
                {
                    Id = "calendar",
                    Title = "Calendar",
                    IconPath = "/Icons/calendar.png",
                    Description = "View and manage calendar events",
                    IsEnabled = true,
                    Order = 1
                },
                new NavigationModule
                {
                    Id = "tasks",
                    Title = "Tasks",
                    IconPath = "/Icons/tasks.png",
                    Description = "Manage your task list",
                    IsEnabled = true,
                    Order = 2
                },
                new NavigationModule
                {
                    Id = "weather",
                    Title = "Weather",
                    IconPath = "/Icons/weather.png",
                    Description = "Current weather and forecast",
                    IsEnabled = true,
                    Order = 3
                },
                new NavigationModule
                {
                    Id = "music",
                    Title = "Music",
                    IconPath = "/Icons/music.png",
                    Description = "Music player controls",
                    IsEnabled = true,
                    Order = 4
                },
                new NavigationModule
                {
                    Id = "settings",
                    Title = "Settings",
                    IconPath = "/Icons/settings.png",
                    Description = "Application settings",
                    IsEnabled = true,
                    Order = 5
                }
            };

            _history = new List<NavigationHistoryItem>();
        }

        public List<NavigationModule> GetModules()
        {
            return _modules.Where(m => m.IsEnabled).OrderBy(m => m.Order).ToList();
        }

        public async Task<bool> NavigateToAsync(string moduleId, object? parameters = null)
        {
            var module = _modules.FirstOrDefault(m => m.Id == moduleId && m.IsEnabled);
            if (module == null)
            {
                return false;
            }

            var previousModule = _currentModule;
            _currentModule = module;

            // Add to history
            _history.Add(new NavigationHistoryItem
            {
                ModuleId = moduleId,
                Title = module.Title,
                Timestamp = DateTime.UtcNow,
                Parameters = parameters
            });

            // Mock navigation delay
            await Task.Delay(50);

            var eventArgs = new NavigationEventArgs
            {
                FromModuleId = previousModule?.Id ?? string.Empty,
                ToModuleId = moduleId,
                Parameters = parameters,
                Success = true
            };

            Navigated?.Invoke(this, eventArgs);
            System.Diagnostics.Debug.WriteLine($"Mock: Navigated to {module.Title}");

            return true;
        }

        public bool CanNavigateTo(string moduleId)
        {
            return _modules.Any(m => m.Id == moduleId && m.IsEnabled);
        }

        public async Task<bool> GoBackAsync()
        {
            if (!CanGoBack) return false;

            // Remove current from history
            if (_history.Count > 0)
                _history.RemoveAt(_history.Count - 1);

            if (_history.Count > 0)
            {
                var lastItem = _history[_history.Count - 1];
                return await NavigateToAsync(lastItem.ModuleId, lastItem.Parameters);
            }

            return false;
        }

        public List<NavigationHistoryItem> GetHistory()
        {
            return new List<NavigationHistoryItem>(_history);
        }

        public void ClearHistory()
        {
            _history.Clear();
            System.Diagnostics.Debug.WriteLine("Mock: Navigation history cleared");
        }

        public async Task SetNavigationExpandedAsync(bool isExpanded)
        {
            if (_isNavigationExpanded != isExpanded)
            {
                _isNavigationExpanded = isExpanded;
                NavigationStateChanged?.Invoke(this, isExpanded);
                System.Diagnostics.Debug.WriteLine($"Mock: Navigation expanded set to {isExpanded}");
            }

            // Mock save to user preferences
            await Task.Delay(10);
        }
    }
}