using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Odin___OpenSpec.Services;
using Odin___OpenSpec.Models;
using System.Collections.Generic;

namespace Odin___OpenSpec.Tests
{
    [TestClass]
    public class ProfileBackupTests
    {
        [TestMethod]
        public async Task TestProfileExportImport()
        {
            var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<SqliteDataContext>();
            var context = new SqliteDataContext(logger);
            await context.InitializeAsync();

            // Create test user and preferences
            var user = new User { Name = "Test User", IsActive = true };
            await context.CreateUserAsync(user);
            var pref = new UserPreference { UserId = user.Id, Key = "test_key", Value = "test_value", DataType = "string" };
            await context.SetUserPreferenceAsync(pref);

            // Export profile
            string exportPath = Path.Combine(Path.GetTempPath(), "profile_export_test.bin");
            await context.ExportProfileAsync(user.Id, exportPath);

            // Delete user and preferences
            await context.DeleteUserAsync(user.Id);

            // Import profile
            bool importResult = await context.ImportProfileAsync(exportPath);

            // Validate import
            var importedUser = await context.GetUserAsync(user.Id);
            var importedPrefs = await context.GetUserPreferencesAsync(user.Id);

            Assert.IsTrue(importResult, "Profile import should succeed");
            Assert.IsNotNull(importedUser, "Imported user should not be null");
            Assert.AreEqual(user.Name, importedUser.Name, "Imported user name should match");
            Assert.AreEqual(1, importedPrefs.Count, "Imported preferences count should be 1");
            Assert.AreEqual(pref.Key, importedPrefs[0].Key, "Imported preference key should match");
            Assert.AreEqual(pref.Value, importedPrefs[0].Value, "Imported preference value should match");
        }
    }
}
