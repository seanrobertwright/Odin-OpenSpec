using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Odin___OpenSpec.Models;
using System.Collections.Generic;

namespace Odin_OpenSpec.Infrastructure
{
    public static class ProfileBackupHelper
    {
        // Export user profile and preferences to a file (JSON, encrypted)
        public static async Task ExportProfileAsync(User user, List<UserPreference> preferences, string filePath)
        {
            var exportObj = new {
                User = user,
                Preferences = preferences
            };
            var json = JsonSerializer.Serialize(exportObj);
            var encrypted = SecureStorageHelper.Protect(json);
            await File.WriteAllBytesAsync(filePath, encrypted);
        }
    }
}
