using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Odin___OpenSpec.Services;
using System;

namespace Odin___OpenSpec.Views
{
    public sealed partial class CreateEventDialog : ContentDialog
    {
        private readonly ICalendarService _calendarService;
        private DateTime? _initialDate;

        public CreateEventDialog(ICalendarService calendarService, DateTime? initialDate = null)
        {
            this.InitializeComponent();
            _calendarService = calendarService;
            _initialDate = initialDate ?? DateTime.Today;
            
            InitializeDefaults();
        }

        private void InitializeDefaults()
        {
            // Set default date to today or provided date
            var startDate = _initialDate ?? DateTime.Today;
            StartDatePicker.Date = startDate;
            EndDatePicker.Date = startDate;

            // Set default times (next hour for 1 hour duration)
            var now = DateTime.Now;
            var nextHour = now.AddHours(1);
            var startTime = new TimeSpan(nextHour.Hour, 0, 0);
            var endTime = startTime.Add(TimeSpan.FromHours(1));

            StartTimePicker.Time = startTime;
            EndTimePicker.Time = endTime;
        }

        private void AllDayCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            bool isAllDay = AllDayCheckBox.IsChecked == true;
            StartTimePicker.IsEnabled = !isAllDay;
            EndTimePicker.IsEnabled = !isAllDay;

            if (isAllDay)
            {
                StartTimePicker.Time = TimeSpan.Zero;
                EndTimePicker.Time = new TimeSpan(23, 59, 59);
            }
        }

        private async void CreateButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                args.Cancel = true;
                await ShowErrorAsync("Please enter an event title");
                return;
            }

            if (!StartDatePicker.Date.HasValue || !EndDatePicker.Date.HasValue)
            {
                args.Cancel = true;
                await ShowErrorAsync("Please select start and end dates");
                return;
            }

            try
            {
                // Build start and end DateTimes
                var startDate = StartDatePicker.Date.Value.DateTime;
                var endDate = EndDatePicker.Date.Value.DateTime;
                
                var startTime = AllDayCheckBox.IsChecked == true ? TimeSpan.Zero : StartTimePicker.Time;
                var endTime = AllDayCheckBox.IsChecked == true ? new TimeSpan(23, 59, 59) : EndTimePicker.Time;

                var startDateTime = startDate.Add(startTime);
                var endDateTime = endDate.Add(endTime);

                // Validate end is after start
                if (endDateTime <= startDateTime)
                {
                    args.Cancel = true;
                    await ShowErrorAsync("End time must be after start time");
                    return;
                }

                // Get selected calendar source
                var selectedSource = CalendarSourceComboBox.SelectedItem as ComboBoxItem;
                var sourceId = selectedSource?.Tag?.ToString() ?? "personal";

                // Create event
                var newEvent = new CalendarEvent
                {
                    Title = TitleTextBox.Text.Trim(),
                    Description = DescriptionTextBox.Text.Trim(),
                    StartTime = startDateTime,
                    EndTime = endDateTime,
                    Location = LocationTextBox.Text.Trim(),
                    IsAllDay = AllDayCheckBox.IsChecked == true,
                    SourceId = sourceId
                };

                var createdEvent = await _calendarService.CreateEventAsync(newEvent);
                
                if (createdEvent == null)
                {
                    args.Cancel = true;
                    await ShowErrorAsync("Failed to create event");
                }
            }
            catch (Exception ex)
            {
                args.Cancel = true;
                await ShowErrorAsync($"Error creating event: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task ShowErrorAsync(string message)
        {
            var errorDialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
        }
    }
}
