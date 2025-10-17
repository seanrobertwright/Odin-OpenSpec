using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Odin___OpenSpec.Services;
using Odin___OpenSpec.Services.MockServices;
using System;
using System.Threading.Tasks;

namespace Odin___OpenSpec.Tests
{
    /// <summary>
    /// Basic tests for service interfaces to validate they can be resolved and work correctly
    /// Note: This is a simple test class for validation. In production, use a proper testing framework.
    /// </summary>
    public static class ServiceInterfaceTests
    {
        /// <summary>
        /// Run basic tests on all service interfaces
        /// </summary>
        public static async Task<bool> RunBasicTests()
        {
            try
            {
                // Create a service provider like the app does
                var services = new ServiceCollection();

                // Configure logging
                services.AddLogging(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Information);
                });

                // Register data services
                services.AddSingleton<IDataContext, SqliteDataContext>();

                // Register core services with mock implementations
                services.AddSingleton<IShellService, MockShellService>();
                services.AddSingleton<INavigationService, MockNavigationService>();
                services.AddSingleton<IThemeService, ThemeService>();
                services.AddSingleton<IUserService, MockUserService>();

                // Register data service abstractions with mock implementations
                var mockDataServices = new MockDataServices();
                services.AddSingleton<ICalendarService>(mockDataServices);
                services.AddSingleton<ITaskService>(mockDataServices);
                services.AddSingleton<IWeatherService>(mockDataServices);
                services.AddSingleton<IMusicService>(mockDataServices);

                var serviceProvider = services.BuildServiceProvider();

                // Test IShellService
                var shellService = serviceProvider.GetRequiredService<IShellService>();
                await shellService.InitializeAsync();
                shellService.SetNavigationVisible(true);
                shellService.SetStatusMessage("Test message");

                // Test INavigationService
                var navigationService = serviceProvider.GetRequiredService<INavigationService>();
                var modules = navigationService.GetModules();
                if (modules.Count == 0)
                    throw new Exception("Navigation service should return modules");

                await navigationService.NavigateToAsync("calendar");
                if (navigationService.CurrentModule?.Id != "calendar")
                    throw new Exception("Navigation to calendar failed");

                // Test IThemeService
                var themeService = serviceProvider.GetRequiredService<IThemeService>();
                await themeService.InitializeAsync();
                
                var initialTheme = themeService.CurrentTheme;
                if (initialTheme != ApplicationTheme.Light && initialTheme != ApplicationTheme.Dark)
                    throw new Exception("Theme service should have a valid initial theme");

                await themeService.SetThemeAsync(ApplicationTheme.Dark);
                if (themeService.CurrentTheme != ApplicationTheme.Dark)
                    throw new Exception("Theme application to Dark failed");

                await themeService.ToggleThemeAsync();
                if (themeService.CurrentTheme != ApplicationTheme.Light)
                    throw new Exception("Theme toggle failed");

                // Test IUserService
                var userService = serviceProvider.GetRequiredService<IUserService>();
                var users = await userService.GetUsersAsync();
                if (users.Count == 0)
                    throw new Exception("User service should return users");

                await userService.SetUserPreferenceAsync(1, "TestKey", "TestValue");
                var preference = await userService.GetUserPreferenceAsync<string>(1, "TestKey");
                if (preference != "TestValue")
                    throw new Exception("User preference setting/getting failed");

                // Test ICalendarService
                var calendarService = serviceProvider.GetRequiredService<ICalendarService>();
                var events = await calendarService.GetEventsForDateAsync(DateTime.Today);
                // Events may be empty, that's ok

                var calendarSources = await calendarService.GetCalendarSourcesAsync();
                if (calendarSources.Count == 0)
                    throw new Exception("Calendar service should return sources");

                // Test ITaskService
                var taskService = serviceProvider.GetRequiredService<ITaskService>();
                var tasks = await taskService.GetTasksAsync(1);
                // Tasks may be empty, that's ok

                var categories = await taskService.GetCategoriesAsync();
                if (categories.Count == 0)
                    throw new Exception("Task service should return categories");

                // Test IWeatherService
                var weatherService = serviceProvider.GetRequiredService<IWeatherService>();
                var weather = await weatherService.GetCurrentWeatherAsync("Seattle, WA");
                if (weather == null)
                    throw new Exception("Weather service should return weather data");

                var forecast = await weatherService.GetWeatherForecastAsync("Seattle, WA", 3);
                if (forecast.Count != 3)
                    throw new Exception("Weather forecast should return requested number of days");

                // Test IMusicService
                var musicService = serviceProvider.GetRequiredService<IMusicService>();
                var musicSources = await musicService.GetMusicSourcesAsync();
                if (musicSources.Count == 0)
                    throw new Exception("Music service should return sources");

                await musicService.PlayPauseAsync();
                await musicService.SetVolumeAsync(50);
                var volume = await musicService.GetVolumeAsync();
                if (volume != 50)
                    throw new Exception("Music volume setting failed");

                System.Diagnostics.Debug.WriteLine("All service interface tests passed!");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Service interface test failed: {ex.Message}");
                return false;
            }
        }
    }
}