namespace WorkoutFitnessTracker.API.Models.Dto_s.Summary
{
    public record MostImprovedExerciseDto
    {
        public string ExerciseName { get; init; } = string.Empty;
        public double Improvement { get; init; }
    }
}
