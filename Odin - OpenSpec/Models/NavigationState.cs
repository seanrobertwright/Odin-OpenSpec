using SQLite;
using System;

namespace Odin___OpenSpec.Models
{
    /// <summary>
    /// Entity model for Navigation State per user
    /// </summary>
    [Table("NavigationState")]
    public class NavigationState
    {
        [PrimaryKey, NotNull]
        public int UserId { get; set; }

        [NotNull]
        public bool IsExpanded { get; set; } = false;

        [MaxLength(100)]
        public string? LastModule { get; set; }

        [NotNull]
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }
}