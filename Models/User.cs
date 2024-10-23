using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WorkoutFitnessTrackerAPI.Models
{
    public class User : IdentityUser<Guid>
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public ICollection<Workout> Workouts { get; set; } = [];
        public ICollection<WorkoutPlan> WorkoutPlans { get; set; } = [];
        public ICollection<Exercise> Exercises { get; set; } = [];
        public ICollection<ProgressRecord> ProgressRecords { get; set; } = [];
    }
}
