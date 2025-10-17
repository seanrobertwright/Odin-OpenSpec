using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Odin___OpenSpec.Services
{
    /// <summary>
    /// Interface for calendar service providing calendar operations
    /// </summary>
    public interface ICalendarService
    {
        /// <summary>
        /// Get events for a specific date range
        /// </summary>
        Task<List<CalendarEvent>> GetEventsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get events for a specific date
        /// </summary>
        Task<List<CalendarEvent>> GetEventsForDateAsync(DateTime date);

        /// <summary>
        /// Create a new calendar event
        /// </summary>
        Task<CalendarEvent?> CreateEventAsync(CalendarEvent calendarEvent);

        /// <summary>
        /// Update an existing calendar event
        /// </summary>
        Task<bool> UpdateEventAsync(CalendarEvent calendarEvent);

        /// <summary>
        /// Delete a calendar event
        /// </summary>
        Task<bool> DeleteEventAsync(string eventId);

        /// <summary>
        /// Get available calendar sources (personal, work, etc.)
        /// </summary>
        Task<List<CalendarSource>> GetCalendarSourcesAsync();

        /// <summary>
        /// Sync with external calendar services
        /// </summary>
        Task<bool> SyncCalendarsAsync();

        /// <summary>
        /// Event fired when calendar data changes
        /// </summary>
        event EventHandler<CalendarChangedEventArgs>? CalendarChanged;
    }

    /// <summary>
    /// Interface for task service providing task management operations
    /// </summary>
    public interface ITaskService
    {
        /// <summary>
        /// Get all tasks for a user
        /// </summary>
        Task<List<TaskItem>> GetTasksAsync(int userId);

        /// <summary>
        /// Get tasks by status
        /// </summary>
        Task<List<TaskItem>> GetTasksByStatusAsync(int userId, TaskStatus status);

        /// <summary>
        /// Get tasks due within a date range
        /// </summary>
        Task<List<TaskItem>> GetTasksDueAsync(int userId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Create a new task
        /// </summary>
        Task<TaskItem?> CreateTaskAsync(TaskItem task);

        /// <summary>
        /// Update an existing task
        /// </summary>
        Task<bool> UpdateTaskAsync(TaskItem task);

        /// <summary>
        /// Delete a task
        /// </summary>
        Task<bool> DeleteTaskAsync(string taskId);

        /// <summary>
        /// Mark task as completed
        /// </summary>
        Task<bool> CompleteTaskAsync(string taskId);

        /// <summary>
        /// Get task categories
        /// </summary>
        Task<List<TaskCategory>> GetCategoriesAsync();

        /// <summary>
        /// Event fired when task data changes
        /// </summary>
        event EventHandler<TaskChangedEventArgs>? TaskChanged;
    }

    /// <summary>
    /// Interface for weather service providing weather data operations
    /// </summary>
    public interface IWeatherService
    {
        /// <summary>
        /// Get current weather for a location
        /// </summary>
        Task<WeatherInfo?> GetCurrentWeatherAsync(string location);

        /// <summary>
        /// Get weather forecast for a location
        /// </summary>
        Task<List<WeatherForecast>> GetWeatherForecastAsync(string location, int days = 7);

        /// <summary>
        /// Get weather for user's saved locations
        /// </summary>
        Task<List<LocationWeather>> GetSavedLocationsWeatherAsync(int userId);

        /// <summary>
        /// Add a location to user's saved locations
        /// </summary>
        Task<bool> SaveLocationAsync(int userId, string location);

        /// <summary>
        /// Remove a location from user's saved locations
        /// </summary>
        Task<bool> RemoveLocationAsync(int userId, string location);

        /// <summary>
        /// Get weather alerts for saved locations
        /// </summary>
        Task<List<WeatherAlert>> GetWeatherAlertsAsync(int userId);

        /// <summary>
        /// Event fired when weather data updates
        /// </summary>
        event EventHandler<WeatherUpdatedEventArgs>? WeatherUpdated;
    }

    /// <summary>
    /// Interface for music service providing music control operations
    /// </summary>
    public interface IMusicService
    {
        /// <summary>
        /// Get current playing track information
        /// </summary>
        Task<TrackInfo?> GetCurrentTrackAsync();

        /// <summary>
        /// Play/pause current track
        /// </summary>
        Task<bool> PlayPauseAsync();

        /// <summary>
        /// Skip to next track
        /// </summary>
        Task<bool> NextTrackAsync();

        /// <summary>
        /// Skip to previous track
        /// </summary>
        Task<bool> PreviousTrackAsync();

        /// <summary>
        /// Set volume (0-100)
        /// </summary>
        Task<bool> SetVolumeAsync(int volume);

        /// <summary>
        /// Get current volume
        /// </summary>
        Task<int> GetVolumeAsync();

        /// <summary>
        /// Get available music sources/apps
        /// </summary>
        Task<List<MusicSource>> GetMusicSourcesAsync();

        /// <summary>
        /// Set active music source
        /// </summary>
        Task<bool> SetActiveMusicSourceAsync(string sourceId);

        /// <summary>
        /// Check if music is currently playing
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Event fired when track changes
        /// </summary>
        event EventHandler<TrackChangedEventArgs>? TrackChanged;

        /// <summary>
        /// Event fired when playback state changes
        /// </summary>
        event EventHandler<bool>? PlaybackStateChanged;
    }

    #region Data Models

    public class CalendarEvent
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public bool IsAllDay { get; set; } = false;
        public string SourceId { get; set; } = string.Empty;
        public List<string> Attendees { get; set; } = new();
    }

    public class CalendarSource
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "personal", "work", "shared"
        public bool IsEnabled { get; set; } = true;
        public string Color { get; set; } = "#0078D4";
    }

    public class TaskItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskStatus Status { get; set; } = TaskStatus.Pending;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public DateTime? DueDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string CategoryId { get; set; } = string.Empty;
        public int UserId { get; set; }
    }

    public class TaskCategory
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#0078D4";
        public string Icon { get; set; } = string.Empty;
    }

    public class WeatherInfo
    {
        public string Location { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public string Condition { get; set; } = string.Empty;
        public string IconCode { get; set; } = string.Empty;
        public double Humidity { get; set; }
        public double WindSpeed { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class WeatherForecast
    {
        public DateTime Date { get; set; }
        public double HighTemperature { get; set; }
        public double LowTemperature { get; set; }
        public string Condition { get; set; } = string.Empty;
        public string IconCode { get; set; } = string.Empty;
        public double ChanceOfRain { get; set; }
    }

    public class LocationWeather
    {
        public string Location { get; set; } = string.Empty;
        public WeatherInfo? CurrentWeather { get; set; }
        public bool IsDefault { get; set; } = false;
    }

    public class WeatherAlert
    {
        public string Id { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; } = AlertSeverity.Minor;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class TrackInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public TimeSpan Position { get; set; }
        public string ArtworkUrl { get; set; } = string.Empty;
        public string SourceId { get; set; } = string.Empty;
    }

    public class MusicSource
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "spotify", "apple", "media_player"
        public bool IsAvailable { get; set; } = true;
        public string IconPath { get; set; } = string.Empty;
    }

    #endregion

    #region Enums

    public enum TaskStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum AlertSeverity
    {
        Minor,
        Moderate,
        Severe,
        Extreme
    }

    #endregion

    #region Event Args

    public class CalendarChangedEventArgs : EventArgs
    {
        public string EventId { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty; // "created", "updated", "deleted"
        public CalendarEvent? Event { get; set; }
    }

    public class TaskChangedEventArgs : EventArgs
    {
        public string TaskId { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty; // "created", "updated", "deleted", "completed"
        public TaskItem? Task { get; set; }
    }

    public class WeatherUpdatedEventArgs : EventArgs
    {
        public string Location { get; set; } = string.Empty;
        public WeatherInfo? WeatherInfo { get; set; }
    }

    public class TrackChangedEventArgs : EventArgs
    {
        public TrackInfo? PreviousTrack { get; set; }
        public TrackInfo? NewTrack { get; set; }
    }

    #endregion
}