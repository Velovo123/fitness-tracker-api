namespace WorkoutFitnessTrackerAPI.Models.Dto_s.IDto_s
{
    public interface IExerciseDto
    {
        string ExerciseName { get; }
        int Sets { get; }
        int Reps { get; }
    }
}
