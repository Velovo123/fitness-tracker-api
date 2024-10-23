using System.ComponentModel.DataAnnotations;

namespace WorkoutFitnessTrackerAPI.Models
{
    public class ProgressRecord : BaseEntity
    {
        public Guid UserId { get; set; }

        public Guid ExerciseId { get; set; }

        [Required]
        public DateTime Date { get; set; }
        [Required]
        [MaxLength(100)]
        public string Progress { get; set; } = string.Empty;
        [Required]
        public User User { get; set; } = null!;
        [Required]
        public Exercise Exercise { get; set; } = null!;
    }
}