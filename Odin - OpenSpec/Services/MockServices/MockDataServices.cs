using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Odin___OpenSpec.Services.MockServices
{
    /// <summary>
    /// Mock implementations of data services for testing and development
    /// </summary>
    public class MockDataServices : ICalendarService, ITaskService, IWeatherService, IMusicService
    {
        #region ICalendarService Implementation

        private readonly List<CalendarEvent> _events;
        private readonly List<CalendarSource> _sources;

        public event EventHandler<CalendarChangedEventArgs>? CalendarChanged;

        public async Task<List<CalendarEvent>> GetEventsAsync(DateTime startDate, DateTime endDate)
        {
            await Task.Delay(50);
            return _events.Where(e => e.StartTime >= startDate && e.StartTime <= endDate).ToList();
        }

        public async Task<List<CalendarEvent>> GetEventsForDateAsync(DateTime date)
        {
            await Task.Delay(30);
            return _events.Where(e => e.StartTime.Date == date.Date).ToList();
        }

        public async Task<CalendarEvent?> CreateEventAsync(CalendarEvent calendarEvent)
        {
            await Task.Delay(100);
            calendarEvent.Id = Guid.NewGuid().ToString();
            _events.Add(calendarEvent);

            CalendarChanged?.Invoke(this, new CalendarChangedEventArgs
            {
                EventId = calendarEvent.Id,
                ChangeType = "created",
                Event = calendarEvent
            });

            System.Diagnostics.Debug.WriteLine($"Mock: Created calendar event {calendarEvent.Title}");
            return calendarEvent;
        }

        public async Task<bool> UpdateEventAsync(CalendarEvent calendarEvent)
        {
            await Task.Delay(50);
            var existing = _events.FirstOrDefault(e => e.Id == calendarEvent.Id);
            if (existing == null) return false;

            var index = _events.IndexOf(existing);
            _events[index] = calendarEvent;

            CalendarChanged?.Invoke(this, new CalendarChangedEventArgs
            {
                EventId = calendarEvent.Id,
                ChangeType = "updated",
                Event = calendarEvent
            });

            return true;
        }

        public async Task<bool> DeleteEventAsync(string eventId)
        {
            await Task.Delay(30);
            var eventToRemove = _events.FirstOrDefault(e => e.Id == eventId);
            if (eventToRemove == null) return false;

            _events.Remove(eventToRemove);

            CalendarChanged?.Invoke(this, new CalendarChangedEventArgs
            {
                EventId = eventId,
                ChangeType = "deleted",
                Event = eventToRemove
            });

            return true;
        }

        public async Task<List<CalendarSource>> GetCalendarSourcesAsync()
        {
            await Task.Delay(20);
            return new List<CalendarSource>(_sources);
        }

        public async Task<bool> SyncCalendarsAsync()
        {
            await Task.Delay(1000);
            System.Diagnostics.Debug.WriteLine("Mock: Calendar sync completed");
            return true;
        }

        #endregion

        #region ITaskService Implementation

        private readonly List<TaskItem> _tasks;
        private readonly List<TaskCategory> _categories;

        public event EventHandler<TaskChangedEventArgs>? TaskChanged;

        public async Task<List<TaskItem>> GetTasksAsync(int userId)
        {
            await Task.Delay(30);
            return _tasks.Where(t => t.UserId == userId).ToList();
        }

        public async Task<List<TaskItem>> GetTasksByStatusAsync(int userId, TaskStatus status)
        {
            await Task.Delay(20);
            return _tasks.Where(t => t.UserId == userId && t.Status == status).ToList();
        }

        public async Task<List<TaskItem>> GetTasksDueAsync(int userId, DateTime startDate, DateTime endDate)
        {
            await Task.Delay(30);
            return _tasks.Where(t => t.UserId == userId && 
                                   t.DueDate.HasValue && 
                                   t.DueDate >= startDate && 
                                   t.DueDate <= endDate).ToList();
        }

        public async Task<TaskItem?> CreateTaskAsync(TaskItem task)
        {
            await Task.Delay(50);
            task.Id = Guid.NewGuid().ToString();
            task.CreatedDate = DateTime.UtcNow;
            _tasks.Add(task);

            TaskChanged?.Invoke(this, new TaskChangedEventArgs
            {
                TaskId = task.Id,
                ChangeType = "created",
                Task = task
            });

            System.Diagnostics.Debug.WriteLine($"Mock: Created task {task.Title}");
            return task;
        }

        public async Task<bool> UpdateTaskAsync(TaskItem task)
        {
            await Task.Delay(30);
            var existing = _tasks.FirstOrDefault(t => t.Id == task.Id);
            if (existing == null) return false;

            var index = _tasks.IndexOf(existing);
            _tasks[index] = task;

            TaskChanged?.Invoke(this, new TaskChangedEventArgs
            {
                TaskId = task.Id,
                ChangeType = "updated",
                Task = task
            });

            return true;
        }

        public async Task<bool> DeleteTaskAsync(string taskId)
        {
            await Task.Delay(20);
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null) return false;

            _tasks.Remove(task);

            TaskChanged?.Invoke(this, new TaskChangedEventArgs
            {
                TaskId = taskId,
                ChangeType = "deleted",
                Task = task
            });

            return true;
        }

        public async Task<bool> CompleteTaskAsync(string taskId)
        {
            await Task.Delay(20);
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null) return false;

            task.Status = TaskStatus.Completed;

            TaskChanged?.Invoke(this, new TaskChangedEventArgs
            {
                TaskId = taskId,
                ChangeType = "completed",
                Task = task
            });

            return true;
        }

        public async Task<List<TaskCategory>> GetCategoriesAsync()
        {
            await Task.Delay(10);
            return new List<TaskCategory>(_categories);
        }

        #endregion

        #region IWeatherService Implementation

        public event EventHandler<WeatherUpdatedEventArgs>? WeatherUpdated;

        public async Task<WeatherInfo?> GetCurrentWeatherAsync(string location)
        {
            await Task.Delay(200);

            // Mock weather data
            var random = new Random();
            return new WeatherInfo
            {
                Location = location,
                Temperature = random.Next(15, 85),
                Condition = GetRandomCondition(),
                IconCode = "sunny",
                Humidity = random.Next(30, 90),
                WindSpeed = random.Next(0, 25),
                LastUpdated = DateTime.UtcNow
            };
        }

        public async Task<List<WeatherForecast>> GetWeatherForecastAsync(string location, int days = 7)
        {
            await Task.Delay(300);

            var forecasts = new List<WeatherForecast>();
            var random = new Random();

            for (int i = 0; i < days; i++)
            {
                forecasts.Add(new WeatherForecast
                {
                    Date = DateTime.Today.AddDays(i),
                    HighTemperature = random.Next(60, 85),
                    LowTemperature = random.Next(40, 65),
                    Condition = GetRandomCondition(),
                    IconCode = "partly-cloudy",
                    ChanceOfRain = random.Next(0, 100)
                });
            }

            return forecasts;
        }

        public async Task<List<LocationWeather>> GetSavedLocationsWeatherAsync(int userId)
        {
            await Task.Delay(150);

            return new List<LocationWeather>
            {
                new LocationWeather
                {
                    Location = "Seattle, WA",
                    IsDefault = true,
                    CurrentWeather = await GetCurrentWeatherAsync("Seattle, WA")
                },
                new LocationWeather
                {
                    Location = "New York, NY",
                    IsDefault = false,
                    CurrentWeather = await GetCurrentWeatherAsync("New York, NY")
                }
            };
        }

        public async Task<bool> SaveLocationAsync(int userId, string location)
        {
            await Task.Delay(50);
            System.Diagnostics.Debug.WriteLine($"Mock: Saved weather location {location} for user {userId}");
            return true;
        }

        public async Task<bool> RemoveLocationAsync(int userId, string location)
        {
            await Task.Delay(30);
            System.Diagnostics.Debug.WriteLine($"Mock: Removed weather location {location} for user {userId}");
            return true;
        }

        public async Task<List<WeatherAlert>> GetWeatherAlertsAsync(int userId)
        {
            await Task.Delay(100);

            // Mock alerts
            return new List<WeatherAlert>
            {
                new WeatherAlert
                {
                    Id = "alert1",
                    Location = "Seattle, WA",
                    Title = "Heavy Rain Warning",
                    Description = "Heavy rainfall expected tonight",
                    Severity = AlertSeverity.Moderate,
                    StartTime = DateTime.Now.AddHours(2),
                    EndTime = DateTime.Now.AddHours(8)
                }
            };
        }

        private string GetRandomCondition()
        {
            var conditions = new[] { "Sunny", "Partly Cloudy", "Cloudy", "Rainy", "Stormy" };
            return conditions[new Random().Next(conditions.Length)];
        }

        #endregion

        #region IMusicService Implementation

        private TrackInfo? _currentTrack;
        private bool _isPlaying = false;
        private int _volume = 75;

        public bool IsPlaying => _isPlaying;

        public event EventHandler<TrackChangedEventArgs>? TrackChanged;
        public event EventHandler<bool>? PlaybackStateChanged;

        public async Task<TrackInfo?> GetCurrentTrackAsync()
        {
            await Task.Delay(10);
            return _currentTrack;
        }

        public async Task<bool> PlayPauseAsync()
        {
            await Task.Delay(50);
            _isPlaying = !_isPlaying;

            PlaybackStateChanged?.Invoke(this, _isPlaying);
            System.Diagnostics.Debug.WriteLine($"Mock: Music {(_isPlaying ? "playing" : "paused")}");

            return true;
        }

        public async Task<bool> NextTrackAsync()
        {
            await Task.Delay(100);

            var previousTrack = _currentTrack;
            _currentTrack = GetRandomTrack();

            TrackChanged?.Invoke(this, new TrackChangedEventArgs
            {
                PreviousTrack = previousTrack,
                NewTrack = _currentTrack
            });

            System.Diagnostics.Debug.WriteLine($"Mock: Next track - {_currentTrack?.Title}");
            return true;
        }

        public async Task<bool> PreviousTrackAsync()
        {
            await Task.Delay(100);

            var previousTrack = _currentTrack;
            _currentTrack = GetRandomTrack();

            TrackChanged?.Invoke(this, new TrackChangedEventArgs
            {
                PreviousTrack = previousTrack,
                NewTrack = _currentTrack
            });

            System.Diagnostics.Debug.WriteLine($"Mock: Previous track - {_currentTrack?.Title}");
            return true;
        }

        public async Task<bool> SetVolumeAsync(int volume)
        {
            await Task.Delay(20);
            _volume = Math.Clamp(volume, 0, 100);
            System.Diagnostics.Debug.WriteLine($"Mock: Volume set to {_volume}");
            return true;
        }

        public async Task<int> GetVolumeAsync()
        {
            await Task.Delay(5);
            return _volume;
        }

        public async Task<List<MusicSource>> GetMusicSourcesAsync()
        {
            await Task.Delay(50);

            return new List<MusicSource>
            {
                new MusicSource
                {
                    Id = "spotify",
                    Name = "Spotify",
                    Type = "spotify",
                    IsAvailable = true,
                    IconPath = "/Icons/spotify.png"
                },
                new MusicSource
                {
                    Id = "apple_music",
                    Name = "Apple Music",
                    Type = "apple",
                    IsAvailable = false,
                    IconPath = "/Icons/apple_music.png"
                },
                new MusicSource
                {
                    Id = "windows_media",
                    Name = "Windows Media Player",
                    Type = "media_player",
                    IsAvailable = true,
                    IconPath = "/Icons/media_player.png"
                }
            };
        }

        public async Task<bool> SetActiveMusicSourceAsync(string sourceId)
        {
            await Task.Delay(100);
            System.Diagnostics.Debug.WriteLine($"Mock: Set active music source to {sourceId}");
            return true;
        }

        private TrackInfo GetRandomTrack()
        {
            var tracks = new[]
            {
                new TrackInfo { Id = "1", Title = "Sample Song 1", Artist = "Artist A", Album = "Album X", Duration = TimeSpan.FromMinutes(3.5) },
                new TrackInfo { Id = "2", Title = "Demo Track", Artist = "Artist B", Album = "Album Y", Duration = TimeSpan.FromMinutes(4.2) },
                new TrackInfo { Id = "3", Title = "Test Music", Artist = "Artist C", Album = "Album Z", Duration = TimeSpan.FromMinutes(2.8) }
            };

            return tracks[new Random().Next(tracks.Length)];
        }

        #endregion

        #region Constructor

        public MockDataServices()
        {
            // Initialize mock data
            _events = new List<CalendarEvent>
            {
                new CalendarEvent
                {
                    Id = "1",
                    Title = "Team Meeting",
                    Description = "Weekly team sync",
                    StartTime = DateTime.Today.AddHours(10),
                    EndTime = DateTime.Today.AddHours(11),
                    Location = "Conference Room A",
                    SourceId = "work"
                },
                new CalendarEvent
                {
                    Id = "2",
                    Title = "Doctor Appointment",
                    Description = "Annual checkup",
                    StartTime = DateTime.Today.AddDays(1).AddHours(14),
                    EndTime = DateTime.Today.AddDays(1).AddHours(15),
                    Location = "Medical Center",
                    SourceId = "personal"
                }
            };

            _sources = new List<CalendarSource>
            {
                new CalendarSource { Id = "personal", Name = "Personal", Type = "personal", Color = "#0078D4" },
                new CalendarSource { Id = "work", Name = "Work", Type = "work", Color = "#107C10" }
            };

            _tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Id = "1",
                    Title = "Complete project documentation",
                    Description = "Finish writing the user manual",
                    Status = TaskStatus.InProgress,
                    Priority = TaskPriority.High,
                    DueDate = DateTime.Today.AddDays(3),
                    UserId = 1,
                    CategoryId = "work"
                },
                new TaskItem
                {
                    Id = "2",
                    Title = "Buy groceries",
                    Description = "Weekly grocery shopping",
                    Status = TaskStatus.Pending,
                    Priority = TaskPriority.Medium,
                    DueDate = DateTime.Today.AddDays(1),
                    UserId = 1,
                    CategoryId = "personal"
                }
            };

            _categories = new List<TaskCategory>
            {
                new TaskCategory { Id = "work", Name = "Work", Color = "#107C10", Icon = "work" },
                new TaskCategory { Id = "personal", Name = "Personal", Color = "#0078D4", Icon = "person" },
                new TaskCategory { Id = "shopping", Name = "Shopping", Color = "#FF8C00", Icon = "shopping" }
            };

            _currentTrack = GetRandomTrack();
        }

        #endregion
    }
}