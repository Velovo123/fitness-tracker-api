using System.ComponentModel.DataAnnotations;

namespace WorkoutFitnessTrackerAPI.Models
{
    public class WorkoutPlan : BaseEntity
    {
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Goal { get; set; } = string.Empty;
        [Required]
        public User User { get; set; } = null!;
        public ICollection<WorkoutPlanExercise> WorkoutPlanExercises { get; set; } = [];
    }
}