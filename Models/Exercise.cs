using System.ComponentModel.DataAnnotations;

namespace WorkoutFitnessTrackerAPI.Models
{
    public class Exercise : BaseEntity
    {
        public Guid? UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Type { get; set; } = string.Empty;

        public User? User { get; set; }
        public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = [];
        public ICollection<WorkoutPlanExercise> WorkoutPlanExercises { get; set; } = [];
        public ICollection<ProgressRecord> ProgressRecords { get; set; } = [];
    }
}