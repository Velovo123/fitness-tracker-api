using System.ComponentModel.DataAnnotations;

namespace WorkoutFitnessTrackerAPI.Models
{
    public class WorkoutExercise : BaseEntity
    {
        public Guid WorkoutId { get; set; }

        public Guid ExerciseId { get; set; }
        [Required]
        public int Sets { get; set; }
        [Required]
        public int Reps { get; set; }
        [Required]
        public Workout Workout { get; set; } = null!;
        [Required]
        public Exercise Exercise { get; set; } = null!;
    }
}