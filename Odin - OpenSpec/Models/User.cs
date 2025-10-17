using SQLite;
using System;

namespace Odin___OpenSpec.Models
{
    /// <summary>
    /// Entity model for User data
    /// </summary>
    [Table("Users")]
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? PhotoPath { get; set; }

        [NotNull]
        public bool IsActive { get; set; } = true;

        [NotNull]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}