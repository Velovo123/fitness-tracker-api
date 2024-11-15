using System.ComponentModel.DataAnnotations;

namespace WorkoutFitnessTrackerAPI.Models
{
    public class Exercise : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Category { get; set; }

        public string PrimaryMuscles { get; set; } = string.Empty;
        public ICollection<UserExercise> UserExercises { get; set; } = [];  
        public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = [];
        public ICollection<WorkoutPlanExercise> WorkoutPlanExercises { get; set; } = [];
        public ICollection<ProgressRecord> ProgressRecords { get; set; } = [];
    }
}