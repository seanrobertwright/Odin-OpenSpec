using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Odin___OpenSpec.Services;
using System;
using System.Linq;

namespace Odin___OpenSpec.Views
{
    public sealed partial class EventDetailDialog : ContentDialog
    {
        private readonly ICalendarService _calendarService;
        private CalendarEvent _event;
        private CalendarSource? _eventSource;
        private bool _isEditMode = false;

        public EventDetailDialog(ICalendarService calendarService, CalendarEvent calendarEvent)
        {
            this.InitializeComponent();
            _calendarService = calendarService;
            _event = calendarEvent;
            
            LoadEventDetails();
        }

        private async void LoadEventDetails()
        {
            try
            {
                // Load calendar sources to get color
                var sources = await _calendarService.GetCalendarSourcesAsync();
                _eventSource = sources.FirstOrDefault(s => s.Id == _event.SourceId);

                // Populate view mode
                ViewTitleText.Text = _event.Title;
                ViewDateTimeText.Text = FormatDateTime(_event.StartTime, _event.EndTime, _event.IsAllDay);
                ViewAllDayText.Visibility = _event.IsAllDay ? Visibility.Visible : Visibility.Collapsed;

                if (!string.IsNullOrEmpty(_event.Location))
                {
                    ViewLocationPanel.Visibility = Visibility.Visible;
                    ViewLocationText.Text = _event.Location;
                }

                if (!string.IsNullOrEmpty(_event.Description))
                {
                    ViewDescriptionPanel.Visibility = Visibility.Visible;
                    ViewDescriptionText.Text = _event.Description;
                }

                ViewCalendarText.Text = _eventSource?.Name ?? "Unknown";
                ViewCalendarColorEllipse.Fill = new SolidColorBrush(ColorFromHex(_eventSource?.Color ?? "#0078D4"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading event details: {ex.Message}");
            }
        }

        private string FormatDateTime(DateTime start, DateTime end, bool isAllDay)
        {
            if (isAllDay)
            {
                if (start.Date == end.Date)
                {
                    return start.ToString("MMMM dd, yyyy");
                }
                return $"{start:MMMM dd} - {end:MMMM dd, yyyy}";
            }

            if (start.Date == end.Date)
            {
                return $"{start:MMMM dd, yyyy} • {start:h:mm tt} - {end:h:mm tt}";
            }
            return $"{start:MMMM dd, h:mm tt} - {end:MMMM dd, h:mm tt, yyyy}";
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EnterEditMode();
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            ExitEditMode();
        }

        private void EnterEditMode()
        {
            _isEditMode = true;
            ViewModePanel.Visibility = Visibility.Collapsed;
            EditModePanel.Visibility = Visibility.Visible;
            this.PrimaryButtonText = "Save Changes";

            // Populate edit fields
            TitleTextBox.Text = _event.Title;
            StartDatePicker.Date = _event.StartTime.Date;
            EndDatePicker.Date = _event.EndTime.Date;
            StartTimePicker.Time = _event.StartTime.TimeOfDay;
            EndTimePicker.Time = _event.EndTime.TimeOfDay;
            AllDayCheckBox.IsChecked = _event.IsAllDay;
            LocationTextBox.Text = _event.Location;
            DescriptionTextBox.Text = _event.Description;

            // Set calendar source
            CalendarSourceComboBox.SelectedIndex = _event.SourceId == "work" ? 1 : 0;
        }

        private void ExitEditMode()
        {
            _isEditMode = false;
            ViewModePanel.Visibility = Visibility.Visible;
            EditModePanel.Visibility = Visibility.Collapsed;
            this.PrimaryButtonText = "Save";
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

        private async void SaveButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (!_isEditMode)
            {
                // Just close the dialog in view mode
                return;
            }

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
                // Build updated event
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

                // Update event
                _event.Title = TitleTextBox.Text.Trim();
                _event.Description = DescriptionTextBox.Text.Trim();
                _event.StartTime = startDateTime;
                _event.EndTime = endDateTime;
                _event.Location = LocationTextBox.Text.Trim();
                _event.IsAllDay = AllDayCheckBox.IsChecked == true;
                _event.SourceId = sourceId;

                var success = await _calendarService.UpdateEventAsync(_event);
                
                if (!success)
                {
                    args.Cancel = true;
                    await ShowErrorAsync("Failed to update event");
                }
            }
            catch (Exception ex)
            {
                args.Cancel = true;
                await ShowErrorAsync($"Error updating event: {ex.Message}");
            }
        }

        private async void DeleteButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true; // Prevent immediate close

            // Confirm deletion
            var confirmDialog = new ContentDialog
            {
                Title = "Delete Event",
                Content = $"Are you sure you want to delete '{_event.Title}'?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    var success = await _calendarService.DeleteEventAsync(_event.Id);
                    if (success)
                    {
                        this.Hide(); // Close the dialog
                    }
                    else
                    {
                        await ShowErrorAsync("Failed to delete event");
                    }
                }
                catch (Exception ex)
                {
                    await ShowErrorAsync($"Error deleting event: {ex.Message}");
                }
            }
        }

        private Windows.UI.Color ColorFromHex(string hex)
        {
            hex = hex.Replace("#", "");
            byte a = 255;
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            return Windows.UI.Color.FromArgb(a, r, g, b);
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
