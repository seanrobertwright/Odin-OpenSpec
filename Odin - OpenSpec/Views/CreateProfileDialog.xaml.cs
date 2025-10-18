using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Odin___OpenSpec.Views
{
    public sealed partial class CreateProfileDialog : ContentDialog
    {
        private string? _selectedPhotoPath;
        private bool _isValid;

        public string UserName { get; private set; } = string.Empty;
        public string? PhotoPath => _selectedPhotoPath;
        public string ProfileType { get; private set; } = "Personal";

        public CreateProfileDialog()
        {
            this.InitializeComponent();
            UpdateValidation();
        }

        private async void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create file picker
                var picker = new FileOpenPicker();
                
                // Get the window handle for the picker
                var mainWindow = App.Current.MainWindow;
                if (mainWindow != null)
                {
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);
                    WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
                }

                // Configure picker
                picker.ViewMode = PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".bmp");

                // Pick file
                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    await LoadPhotoAsync(file);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error selecting photo: {ex.Message}");
                
                // Show error to user
                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to select photo: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private async Task LoadPhotoAsync(StorageFile file)
        {
            try
            {
                // Copy file to app data
                var appData = ApplicationData.Current.LocalFolder;
                var profilePhotos = await appData.CreateFolderAsync("ProfilePhotos", CreationCollisionOption.OpenIfExists);
                
                // Generate unique filename
                var fileName = $"profile_{Guid.NewGuid()}{Path.GetExtension(file.Name)}";
                var destFile = await profilePhotos.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                
                // Copy file
                await file.CopyAndReplaceAsync(destFile);
                
                // Update photo path
                _selectedPhotoPath = destFile.Path;
                
                // Load image for preview
                using var stream = await destFile.OpenAsync(FileAccessMode.Read);
                var bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(stream);
                ProfilePicture.ProfilePicture = bitmapImage;
                
                RemovePhotoButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading photo: {ex.Message}");
                throw;
            }
        }

        private void RemovePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedPhotoPath = null;
            ProfilePicture.ProfilePicture = null;
            
            // Update initials if we have a name
            if (!string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ProfilePicture.Initials = GetInitials(NameTextBox.Text);
            }
            else
            {
                ProfilePicture.Initials = "?";
            }
            
            RemovePhotoButton.IsEnabled = false;
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateValidation();
            
            // Update initials preview
            if (!string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ProfilePicture.Initials = GetInitials(NameTextBox.Text);
                ProfilePicture.DisplayName = NameTextBox.Text;
            }
            else
            {
                ProfilePicture.Initials = "?";
                ProfilePicture.DisplayName = "New User";
            }
        }

        private void UpdateValidation()
        {
            var name = NameTextBox.Text?.Trim();
            _isValid = !string.IsNullOrWhiteSpace(name) && name.Length >= 2;
            
            // Show/hide error message
            NameErrorText.Visibility = string.IsNullOrWhiteSpace(name) && NameTextBox.FocusState != FocusState.Unfocused
                ? Visibility.Visible
                : Visibility.Collapsed;
            
            // Enable/disable primary button
            IsPrimaryButtonEnabled = _isValid;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Validate before closing
            if (!_isValid)
            {
                args.Cancel = true;
                NameErrorText.Visibility = Visibility.Visible;
                return;
            }

            // Store values
            UserName = NameTextBox.Text.Trim();
            ProfileType = (ProfileTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Personal";
        }

        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Clean up temp photo if user cancels
            if (!string.IsNullOrEmpty(_selectedPhotoPath) && File.Exists(_selectedPhotoPath))
            {
                try
                {
                    File.Delete(_selectedPhotoPath);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "?";

            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

            return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
        }
    }
}
