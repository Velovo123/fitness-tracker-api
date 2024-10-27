namespace WorkoutFitnessTrackerAPI.Models
{
    public class UserExercise
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid ExerciseId { get; set; }
        public Exercise Exercise { get; set; } = null!;
    }
}
