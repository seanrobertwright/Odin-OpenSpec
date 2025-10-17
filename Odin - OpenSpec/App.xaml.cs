using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Odin___OpenSpec.Services;
using Odin___OpenSpec.Services.MockServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Odin___OpenSpec
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;
        private ServiceProvider? _serviceProvider;

        /// <summary>
        /// Gets the current application instance.
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the service provider for dependency injection.
        /// </summary>
        public ServiceProvider ServiceProvider => _serviceProvider ?? throw new InvalidOperationException("Service provider not initialized");

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            ConfigureServices();
        }

        /// <summary>
        /// Configures the dependency injection container.
        /// </summary>
        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Configure logging
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Register data services
            services.AddSingleton<IDataContext, SqliteDataContext>();

            // Register core services
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

            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                _window = new MainWindow();
                
                // Initialize database
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var dataContext = ServiceProvider.GetRequiredService<IDataContext>();
                        await dataContext.InitializeAsync();
                    }
                    catch (Exception ex)
                    {
                        // Log error - in production this should be handled appropriately
                        System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex.Message}");
                    }
                });
                
                _window.Activate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnLaunched failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
