using System.ComponentModel.DataAnnotations;

namespace WorkoutFitnessTrackerAPI.Models
{
    public class Workout : BaseEntity
    {
        public Guid UserId { get; set; }

        [Required]
        public DateTime Date { get; set; }
        [Required]
        public int Duration { get; set; }
        [Required]
        public User User { get; set; } = null!;
        public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = [];
    }
}