using System.ComponentModel.DataAnnotations;

namespace WorkoutFitnessTrackerAPI.Models
{
    public class WorkoutPlanExercise : BaseEntity
    {
        public Guid WorkoutPlanId { get; set; }

        public Guid ExerciseId { get; set; }

        [Required]
        public int Sets { get; set; }

        [Required]
        public int Reps { get; set; }
        [Required]
        public WorkoutPlan WorkoutPlan { get; set; } = null!;
        [Required]
        public Exercise Exercise { get; set; } = null!;
    }
}