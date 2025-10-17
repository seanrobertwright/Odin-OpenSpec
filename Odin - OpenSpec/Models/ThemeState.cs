using SQLite;
using System;

namespace Odin___OpenSpec.Models
{
    /// <summary>
    /// Entity model for Theme State per user
    /// </summary>
    [Table("ThemeState")]
    public class ThemeState
    {
        [PrimaryKey, NotNull]
        public int UserId { get; set; }

        [NotNull, MaxLength(50)]
        public string ThemeName { get; set; } = "Light";

        [MaxLength(2000)]
        public string? CustomSettings { get; set; }

        [NotNull]
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }
}