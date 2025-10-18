using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Odin___OpenSpec.Models;
using Odin___OpenSpec.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Odin___OpenSpec.Views
{
    public enum CalendarViewMode
    {
        Day,
        WorkWeek,
        Week,
        Month,
        SplitView
    }

    public sealed partial class CalendarView : UserControl
    {
        private readonly ICalendarService _calendarService;
        private DateTime _currentMonth;
        private DateTime _today;
        private List<CalendarEvent> _allEvents;
        private List<CalendarSource> _calendarSources;
        private CalendarViewMode _currentViewMode;

        public CalendarView()
        {
            this.InitializeComponent();
            _calendarService = (ICalendarService?)((App)Application.Current).ServiceProvider?.GetService(typeof(ICalendarService)) ?? throw new InvalidOperationException("ICalendarService not found");
            _today = DateTime.Today;
            _currentMonth = new DateTime(_today.Year, _today.Month, 1);
            _allEvents = new List<CalendarEvent>();
            _calendarSources = new List<CalendarSource>();
            _currentViewMode = CalendarViewMode.Month; // Default to month view
            
            this.Loaded += CalendarView_Loaded;
        }

        private async void CalendarView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Small delay to ensure all XAML elements are fully initialized
                await System.Threading.Tasks.Task.Delay(100);
                
                System.Diagnostics.Debug.WriteLine("CalendarView_Loaded started");
                System.Diagnostics.Debug.WriteLine($"MiniCalendar null? {MiniCalendar == null}");
                System.Diagnostics.Debug.WriteLine($"CalendarGrid null? {CalendarGrid == null}");
                System.Diagnostics.Debug.WriteLine($"CurrentMonthText null? {CurrentMonthText == null}");
                System.Diagnostics.Debug.WriteLine($"PersonalCalendarCheck null? {PersonalCalendarCheck == null}");
                System.Diagnostics.Debug.WriteLine($"WorkCalendarCheck null? {WorkCalendarCheck == null}");
                
                if (MiniCalendar != null)
                {
                    MiniCalendar.SetDisplayDate(_currentMonth);
                    MiniCalendar.SelectedDates.Add(_today);
                }
                
                await LoadCalendarSourcesAsync();
                await LoadEventsAsync();
                RenderMonthGrid();
                
                System.Diagnostics.Debug.WriteLine("CalendarView_Loaded completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading calendar: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private async System.Threading.Tasks.Task LoadCalendarSourcesAsync()
        {
            try
            {
                _calendarSources = await _calendarService.GetCalendarSourcesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading calendar sources: {ex.Message}");
                _calendarSources = new List<CalendarSource>();
            }
        }

        private async System.Threading.Tasks.Task LoadEventsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoadEventsAsync started");
                
                var startOfMonth = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                
                System.Diagnostics.Debug.WriteLine($"Getting events from {startOfMonth} to {endOfMonth}");
                
                if (_calendarService == null)
                {
                    System.Diagnostics.Debug.WriteLine("_calendarService is null!");
                    _allEvents = new List<CalendarEvent>();
                    return;
                }
                
                _allEvents = await _calendarService.GetEventsAsync(startOfMonth, endOfMonth);
                System.Diagnostics.Debug.WriteLine($"Loaded {_allEvents?.Count ?? 0} events");
                
                if (_allEvents == null)
                {
                    System.Diagnostics.Debug.WriteLine("_allEvents is null after GetEventsAsync!");
                    _allEvents = new List<CalendarEvent>();
                    return;
                }
                
                // Filter by selected calendars if checkboxes are initialized
                if (PersonalCalendarCheck != null && WorkCalendarCheck != null && _calendarSources != null && _calendarSources.Any())
                {
                    System.Diagnostics.Debug.WriteLine("Filtering events by calendar sources");
                    var enabledSourceIds = _calendarSources
                        .Where(s => (s.Name == "Personal" && PersonalCalendarCheck.IsChecked == true) ||
                                    (s.Name == "Work" && WorkCalendarCheck.IsChecked == true))
                        .Select(s => s.Id)
                        .ToList();
                    
                    System.Diagnostics.Debug.WriteLine($"Enabled source IDs: {string.Join(", ", enabledSourceIds)}");
                    
                    if (enabledSourceIds.Any())
                    {
                        _allEvents = _allEvents.Where(ev => ev != null && enabledSourceIds.Contains(ev.SourceId)).ToList();
                        System.Diagnostics.Debug.WriteLine($"Filtered to {_allEvents.Count} events");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Skipping filtering - checkboxes or sources not ready");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading events: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                _allEvents = new List<CalendarEvent>();
            }
        }

        private void RenderMonthGrid()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("RenderMonthGrid started");
                
                if (CalendarGrid == null)
                {
                    System.Diagnostics.Debug.WriteLine("CalendarGrid is null - cannot render");
                    return;
                }
                
                if (CurrentMonthText == null)
                {
                    System.Diagnostics.Debug.WriteLine("CurrentMonthText is null - cannot update header");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("Clearing CalendarGrid");
                CalendarGrid.Children.Clear();
                CalendarGrid.RowDefinitions.Clear();
                CalendarGrid.ColumnDefinitions.Clear();

                // Create 7 columns (days of week)
                for (int i = 0; i < 7; i++)
                {
                    CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                // Calculate calendar layout
                var firstDayOfMonth = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                var firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek; // 0 = Sunday
                var daysInMonth = lastDayOfMonth.Day;

                // Calculate number of weeks to display
                var totalCells = firstDayOfWeek + daysInMonth;
                var numWeeks = (int)Math.Ceiling(totalCells / 7.0);

                System.Diagnostics.Debug.WriteLine($"Rendering {numWeeks} weeks for {_currentMonth:MMMM yyyy}");

                // Create rows (weeks)
                for (int i = 0; i < numWeeks; i++)
                {
                    CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(120, GridUnitType.Pixel) });
                }

                // Update month header
                CurrentMonthText.Text = _currentMonth.ToString("MMMM yyyy");

                // Render day cells
                int dayCounter = 1;
                for (int week = 0; week < numWeeks; week++)
                {
                    for (int day = 0; day < 7; day++)
                    {
                        int cellIndex = week * 7 + day;
                        
                        // Determine if this cell should show a day number
                        if (cellIndex >= firstDayOfWeek && dayCounter <= daysInMonth)
                        {
                            var currentDate = new DateTime(_currentMonth.Year, _currentMonth.Month, dayCounter);
                            var dayCell = CreateDayCell(currentDate);
                            
                            if (dayCell != null)
                            {
                                Grid.SetRow(dayCell, week);
                                Grid.SetColumn(dayCell, day);
                                CalendarGrid.Children.Add(dayCell);
                            }
                            
                            dayCounter++;
                        }
                        else
                        {
                            // Empty cell for days outside current month
                            var emptyCell = CreateEmptyCell(week, day);
                            if (emptyCell != null)
                            {
                                Grid.SetRow(emptyCell, week);
                                Grid.SetColumn(emptyCell, day);
                                CalendarGrid.Children.Add(emptyCell);
                            }
                        }
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"RenderMonthGrid completed - added {CalendarGrid.Children.Count} cells");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error rendering month grid: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private Border CreateDayCell(DateTime date)
        {
            try
            {
                var isToday = date.Date == _today.Date;
                var dayEvents = _allEvents?.Where(e => e.StartTime.Date == date.Date).OrderBy(e => e.StartTime).ToList() ?? new List<CalendarEvent>();

                var border = new Border
                {
                    BorderBrush = (SolidColorBrush?)Application.Current.Resources["CardStrokeColorDefaultBrush"] ?? new SolidColorBrush(Colors.Gray),
                    BorderThickness = new Thickness(1, 1, 0, 0),
                    Padding = new Thickness(4),
                    Background = (SolidColorBrush?)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"] ?? new SolidColorBrush(Colors.White)
                };

                if (isToday)
                {
                    border.BorderBrush = new SolidColorBrush(Colors.DodgerBlue);
                    border.BorderThickness = new Thickness(2);
                }

                var contentStack = new StackPanel { Spacing = 2 };

                // Day number
                var dayNumber = new TextBlock
                {
                    Text = date.Day.ToString(),
                    FontWeight = isToday ? Microsoft.UI.Text.FontWeights.Bold : Microsoft.UI.Text.FontWeights.Normal,
                    Foreground = isToday ? new SolidColorBrush(Colors.DodgerBlue) : (SolidColorBrush?)Application.Current.Resources["TextFillColorPrimaryBrush"] ?? new SolidColorBrush(Colors.Black),
                    Margin = new Thickness(4, 2, 0, 4)
                };
                contentStack.Children.Add(dayNumber);

                // Render up to 3 events, then show "+N more"
                int maxEventsToShow = 3;
                for (int i = 0; i < Math.Min(dayEvents.Count, maxEventsToShow); i++)
                {
                    var eventBar = CreateEventBar(dayEvents[i]);
                    if (eventBar != null)
                    {
                        contentStack.Children.Add(eventBar);
                    }
                }

                if (dayEvents.Count > maxEventsToShow)
                {
                    var moreText = new TextBlock
                    {
                        Text = $"+{dayEvents.Count - maxEventsToShow} more",
                        FontSize = 10,
                        Foreground = (SolidColorBrush?)Application.Current.Resources["TextFillColorSecondaryBrush"] ?? new SolidColorBrush(Colors.Gray),
                        Margin = new Thickness(4, 2, 0, 0)
                    };
                    contentStack.Children.Add(moreText);
                }

                border.Child = contentStack;
                return border;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating day cell: {ex.Message}");
                return new Border { Background = new SolidColorBrush(Colors.White) };
            }
        }

        private Border CreateEmptyCell(int week, int day)
        {
            try
            {
                var border = new Border
                {
                    BorderBrush = (SolidColorBrush?)Application.Current.Resources["CardStrokeColorDefaultBrush"] ?? new SolidColorBrush(Colors.Gray),
                    BorderThickness = new Thickness(1, 1, 0, 0),
                    Background = (SolidColorBrush?)Application.Current.Resources["LayerFillColorDefaultBrush"] ?? new SolidColorBrush(Colors.LightGray)
                };
                
                return border;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating empty cell: {ex.Message}");
                return new Border { Background = new SolidColorBrush(Colors.LightGray) };
            }
        }

        private Border? CreateEventBar(CalendarEvent calEvent)
        {
            try
            {
                if (calEvent == null || _calendarSources == null)
                {
                    return null;
                }

                var source = _calendarSources.FirstOrDefault(s => s.Id == calEvent.SourceId);
                var color = source?.Color ?? "#0078D4";
                var brush = ColorFromHex(color);

                var border = new Border
                {
                    Background = brush,
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(6, 3, 6, 3),
                    Margin = new Thickness(2, 1, 2, 1),
                    Height = 22,
                    Tag = calEvent // Store event reference
                };

                // Make it clickable
                border.Tapped += EventBar_Tapped;
                border.PointerEntered += (s, e) => { if (s is Border b) b.Opacity = 0.8; };
                border.PointerExited += (s, e) => { if (s is Border b) b.Opacity = 1.0; };

                var stack = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 4
                };

                // Time text
                var timeText = new TextBlock
                {
                    Text = calEvent.StartTime.ToString("h:mm tt"),
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.White),
                    VerticalAlignment = VerticalAlignment.Center
                };
                stack.Children.Add(timeText);

                // Person icon (if applicable)
                var personIcon = new FontIcon
                {
                    Glyph = "\uE716",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.White)
                };
                stack.Children.Add(personIcon);

                // Event title (truncated)
                var titleText = new TextBlock
                {
                    Text = calEvent.Title ?? "Untitled Event",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.White),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    VerticalAlignment = VerticalAlignment.Center
                };
                stack.Children.Add(titleText);

                border.Child = stack;
                return border;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating event bar: {ex.Message}");
                return null;
            }
        }

        private SolidColorBrush ColorFromHex(string hex)
        {
            hex = hex.Replace("#", "");
            byte a = 255;
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            return new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
        }

        #region View Rendering Methods

        private void RenderDayView()
        {
            try
            {
                if (CalendarGrid == null || CurrentMonthText == null) return;

                CalendarGrid.Children.Clear();
                CalendarGrid.RowDefinitions.Clear();
                CalendarGrid.ColumnDefinitions.Clear();

                // Single column for day view
                CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                // Header
                CurrentMonthText.Text = _today.ToString("dddd, MMMM dd, yyyy");

                // Get today's events
                var todayEvents = _allEvents?.Where(e => e.StartTime.Date == _today.Date).OrderBy(e => e.StartTime).ToList() ?? new List<CalendarEvent>();

                // Time slots (6 AM to 10 PM in 1-hour increments)
                for (int hour = 6; hour <= 22; hour++)
                {
                    CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60, GridUnitType.Pixel) });

                    var timeSlot = new Border
                    {
                        BorderBrush = (SolidColorBrush?)Application.Current.Resources["CardStrokeColorDefaultBrush"] ?? new SolidColorBrush(Colors.Gray),
                        BorderThickness = new Thickness(0, 1, 0, 0),
                        Padding = new Thickness(8)
                    };

                    var grid = new Grid();
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    // Time label
                    var timeText = new TextBlock
                    {
                        Text = new DateTime(2000, 1, 1, hour, 0, 0).ToString("h tt"),
                        Foreground = (SolidColorBrush?)Application.Current.Resources["TextFillColorSecondaryBrush"] ?? new SolidColorBrush(Colors.Gray),
                        VerticalAlignment = VerticalAlignment.Top
                    };
                    Grid.SetColumn(timeText, 0);
                    grid.Children.Add(timeText);

                    // Events in this hour
                    var eventsInHour = todayEvents.Where(e => e.StartTime.Hour == hour).ToList();
                    if (eventsInHour.Any())
                    {
                        var eventStack = new StackPanel { Spacing = 4 };
                        foreach (var evt in eventsInHour)
                        {
                            var eventCard = CreateDayViewEventCard(evt);
                            if (eventCard != null)
                            {
                                eventStack.Children.Add(eventCard);
                            }
                        }
                        Grid.SetColumn(eventStack, 1);
                        grid.Children.Add(eventStack);
                    }

                    timeSlot.Child = grid;
                    Grid.SetRow(timeSlot, hour - 6);
                    Grid.SetColumn(timeSlot, 0);
                    CalendarGrid.Children.Add(timeSlot);
                }

                System.Diagnostics.Debug.WriteLine($"Day view rendered with {todayEvents.Count} events");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error rendering day view: {ex.Message}");
            }
        }

        private Border? CreateDayViewEventCard(CalendarEvent evt)
        {
            try
            {
                var source = _calendarSources?.FirstOrDefault(s => s.Id == evt.SourceId);
                var color = source?.Color ?? "#0078D4";
                var brush = ColorFromHex(color);

                var border = new Border
                {
                    Background = brush,
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(12, 8, 12, 8),
                    Margin = new Thickness(4, 2, 4, 2),
                    Tag = evt // Store event reference
                };

                // Make it clickable
                border.Tapped += EventBar_Tapped;
                border.PointerEntered += (s, e) => { if (s is Border b) b.Opacity = 0.8; };
                border.PointerExited += (s, e) => { if (s is Border b) b.Opacity = 1.0; };

                var stack = new StackPanel { Spacing = 4 };

                var titleText = new TextBlock
                {
                    Text = evt.Title ?? "Untitled",
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Colors.White)
                };
                stack.Children.Add(titleText);

                var timeText = new TextBlock
                {
                    Text = $"{evt.StartTime:h:mm tt} - {evt.EndTime:h:mm tt}",
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Colors.White),
                    Opacity = 0.9
                };
                stack.Children.Add(timeText);

                if (!string.IsNullOrEmpty(evt.Location))
                {
                    var locationStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
                    locationStack.Children.Add(new FontIcon
                    {
                        Glyph = "\uE707",
                        FontSize = 12,
                        Foreground = new SolidColorBrush(Colors.White)
                    });
                    locationStack.Children.Add(new TextBlock
                    {
                        Text = evt.Location,
                        FontSize = 12,
                        Foreground = new SolidColorBrush(Colors.White),
                        Opacity = 0.9
                    });
                    stack.Children.Add(locationStack);
                }

                border.Child = stack;
                return border;
            }
            catch
            {
                return null;
            }
        }

        private void RenderWorkWeekView()
        {
            RenderWeekViewInternal(workWeekOnly: true);
        }

        private void RenderWeekView()
        {
            RenderWeekViewInternal(workWeekOnly: false);
        }

        private void RenderWeekViewInternal(bool workWeekOnly)
        {
            try
            {
                if (CalendarGrid == null || CurrentMonthText == null) return;

                CalendarGrid.Children.Clear();
                CalendarGrid.RowDefinitions.Clear();
                CalendarGrid.ColumnDefinitions.Clear();

                // Get start of week (Sunday or Monday)
                var startOfWeek = _today.AddDays(-(int)_today.DayOfWeek);
                var daysToShow = workWeekOnly ? 5 : 7; // Mon-Fri for work week, Sun-Sat for full week
                var startDay = workWeekOnly ? startOfWeek.AddDays(1) : startOfWeek; // Start Monday for work week

                CurrentMonthText.Text = $"{startDay:MMM dd} - {startDay.AddDays(daysToShow - 1):MMM dd, yyyy}";

                // Create columns: Time + Days
                CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) }); // Time column
                for (int i = 0; i < daysToShow; i++)
                {
                    CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                // Header row
                CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
                for (int day = 0; day < daysToShow; day++)
                {
                    var date = startDay.AddDays(day);
                    var isToday = date.Date == _today.Date;

                    var headerBorder = new Border
                    {
                        BorderBrush = (SolidColorBrush?)Application.Current.Resources["CardStrokeColorDefaultBrush"] ?? new SolidColorBrush(Colors.Gray),
                        BorderThickness = new Thickness(1, 0, 0, 1),
                        Background = isToday ? new SolidColorBrush(Colors.DodgerBlue) { Opacity = 0.1 } : null,
                        Padding = new Thickness(8)
                    };

                    var headerStack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
                    headerStack.Children.Add(new TextBlock
                    {
                        Text = date.ToString("ddd"),
                        FontSize = 12,
                        Foreground = isToday ? new SolidColorBrush(Colors.DodgerBlue) : (SolidColorBrush?)Application.Current.Resources["TextFillColorPrimaryBrush"] ?? new SolidColorBrush(Colors.Black)
                    });
                    headerStack.Children.Add(new TextBlock
                    {
                        Text = date.Day.ToString(),
                        FontSize = 16,
                        FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                        Foreground = isToday ? new SolidColorBrush(Colors.DodgerBlue) : (SolidColorBrush?)Application.Current.Resources["TextFillColorPrimaryBrush"] ?? new SolidColorBrush(Colors.Black)
                    });

                    headerBorder.Child = headerStack;
                    Grid.SetRow(headerBorder, 0);
                    Grid.SetColumn(headerBorder, day + 1);
                    CalendarGrid.Children.Add(headerBorder);
                }

                // Time slots (8 AM to 6 PM for work week view)
                for (int hour = 8; hour <= 18; hour++)
                {
                    CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60, GridUnitType.Pixel) });
                    var rowIndex = hour - 8 + 1;

                    // Time label
                    var timeLabel = new TextBlock
                    {
                        Text = new DateTime(2000, 1, 1, hour, 0, 0).ToString("h tt"),
                        Foreground = (SolidColorBrush?)Application.Current.Resources["TextFillColorSecondaryBrush"] ?? new SolidColorBrush(Colors.Gray),
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Margin = new Thickness(0, 4, 8, 0),
                        FontSize = 12
                    };
                    Grid.SetRow(timeLabel, rowIndex);
                    Grid.SetColumn(timeLabel, 0);
                    CalendarGrid.Children.Add(timeLabel);

                    // Day cells
                    for (int day = 0; day < daysToShow; day++)
                    {
                        var date = startDay.AddDays(day);
                        var cellBorder = new Border
                        {
                            BorderBrush = (SolidColorBrush?)Application.Current.Resources["CardStrokeColorDefaultBrush"] ?? new SolidColorBrush(Colors.Gray),
                            BorderThickness = new Thickness(1, 1, 0, 0),
                            Padding = new Thickness(4)
                        };

                        // Get events for this hour on this day
                        var eventsInSlot = _allEvents?.Where(e =>
                            e.StartTime.Date == date.Date &&
                            e.StartTime.Hour == hour
                        ).OrderBy(e => e.StartTime).ToList() ?? new List<CalendarEvent>();

                        if (eventsInSlot.Any())
                        {
                            var eventStack = new StackPanel { Spacing = 2 };
                            foreach (var evt in eventsInSlot.Take(2)) // Show max 2 events per slot
                            {
                                var eventBar = CreateEventBar(evt);
                                if (eventBar != null)
                                {
                                    eventStack.Children.Add(eventBar);
                                }
                            }
                            cellBorder.Child = eventStack;
                        }

                        Grid.SetRow(cellBorder, rowIndex);
                        Grid.SetColumn(cellBorder, day + 1);
                        CalendarGrid.Children.Add(cellBorder);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"{(workWeekOnly ? "Work week" : "Week")} view rendered");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error rendering week view: {ex.Message}");
            }
        }

        private void RenderSplitView()
        {
            // Split view shows month grid on left, day details on right
            // For now, just render month view (we can enhance this later)
            RenderMonthGrid();
        }

        #endregion

        // Event Handlers
        private void TodayButton_Click(object sender, RoutedEventArgs e)
        {
            _today = DateTime.Today;
            _currentMonth = new DateTime(_today.Year, _today.Month, 1);
            if (MiniCalendar != null)
            {
                MiniCalendar.SetDisplayDate(_currentMonth);
                MiniCalendar.SelectedDates.Clear();
                MiniCalendar.SelectedDates.Add(_today);
            }
            RenderCurrentView();
        }

        private void PreviousMonth_Click(object sender, RoutedEventArgs e)
        {
            switch (_currentViewMode)
            {
                case CalendarViewMode.Day:
                    _today = _today.AddDays(-1);
                    break;
                case CalendarViewMode.WorkWeek:
                case CalendarViewMode.Week:
                    _today = _today.AddDays(-7);
                    break;
                case CalendarViewMode.Month:
                case CalendarViewMode.SplitView:
                    _currentMonth = _currentMonth.AddMonths(-1);
                    _today = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
                    break;
            }
            
            if (MiniCalendar != null)
            {
                MiniCalendar.SetDisplayDate(_currentMonth);
            }
            RenderCurrentView();
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            switch (_currentViewMode)
            {
                case CalendarViewMode.Day:
                    _today = _today.AddDays(1);
                    break;
                case CalendarViewMode.WorkWeek:
                case CalendarViewMode.Week:
                    _today = _today.AddDays(7);
                    break;
                case CalendarViewMode.Month:
                case CalendarViewMode.SplitView:
                    _currentMonth = _currentMonth.AddMonths(1);
                    _today = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
                    break;
            }
            
            if (MiniCalendar != null)
            {
                MiniCalendar.SetDisplayDate(_currentMonth);
            }
            RenderCurrentView();
        }

        private void MiniCalendar_SelectedDatesChanged(Microsoft.UI.Xaml.Controls.CalendarView sender, Microsoft.UI.Xaml.Controls.CalendarViewSelectedDatesChangedEventArgs args)
        {
            if (sender.SelectedDates.Count > 0)
            {
                var selectedDate = sender.SelectedDates[0].DateTime;
                _today = selectedDate;
                _currentMonth = new DateTime(selectedDate.Year, selectedDate.Month, 1);
                RenderCurrentView();
            }
        }

        private async void CalendarFilter_Changed(object sender, RoutedEventArgs e)
        {
            await LoadEventsAsync();
            RenderCurrentView();
        }

        private void ViewMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModeComboBox == null) return;
            
            _currentViewMode = ViewModeComboBox.SelectedIndex switch
            {
                0 => CalendarViewMode.Day,
                1 => CalendarViewMode.WorkWeek,
                2 => CalendarViewMode.Week,
                3 => CalendarViewMode.Month,
                4 => CalendarViewMode.SplitView,
                _ => CalendarViewMode.Month
            };
            
            System.Diagnostics.Debug.WriteLine($"View mode changed to: {_currentViewMode}");
            RenderCurrentView();
        }

        private void RenderCurrentView()
        {
            switch (_currentViewMode)
            {
                case CalendarViewMode.Day:
                    RenderDayView();
                    break;
                case CalendarViewMode.WorkWeek:
                    RenderWorkWeekView();
                    break;
                case CalendarViewMode.Week:
                    RenderWeekView();
                    break;
                case CalendarViewMode.Month:
                    RenderMonthGrid();
                    break;
                case CalendarViewMode.SplitView:
                    RenderSplitView();
                    break;
            }
        }

        private async void AddEventButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Determine initial date based on current view mode
                DateTime? initialDate = _currentViewMode == CalendarViewMode.Month ? _currentMonth : _today;
                
                var dialog = new CreateEventDialog(_calendarService, initialDate)
                {
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                
                if (result == ContentDialogResult.Primary)
                {
                    // Reload events and refresh view
                    await LoadEventsAsync();
                    RenderCurrentView();
                    System.Diagnostics.Debug.WriteLine("Event created successfully");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing create event dialog: {ex.Message}");
            }
        }

        private async void EventBar_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Border border && border.Tag is CalendarEvent calEvent)
                {
                    var dialog = new EventDetailDialog(_calendarService, calEvent)
                    {
                        XamlRoot = this.XamlRoot
                    };

                    await dialog.ShowAsync();
                    
                    // Reload events and refresh view (event may have been edited or deleted)
                    await LoadEventsAsync();
                    RenderCurrentView();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing event detail dialog: {ex.Message}");
            }
        }

        private void ShowNotification(string message)
        {
            // TODO: Implement proper notification system
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
