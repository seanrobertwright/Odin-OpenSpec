using SQLite;
using System;

namespace Odin___OpenSpec.Models
{
    /// <summary>
    /// Entity model for User Preferences
    /// </summary>
    [Table("UserPreferences")]
    public class UserPreference
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull, Indexed]
        public int UserId { get; set; }

        [NotNull, MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Value { get; set; }

        [NotNull, MaxLength(50)]
        public string DataType { get; set; } = "string";

        [NotNull]
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }
}