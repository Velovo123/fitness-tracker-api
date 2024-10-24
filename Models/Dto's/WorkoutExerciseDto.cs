namespace WorkoutFitnessTrackerAPI.Models.Dto_s
{
    public record WorkoutExerciseDto(string ExerciseName, int Sets, int Reps, string? Type = null);
}