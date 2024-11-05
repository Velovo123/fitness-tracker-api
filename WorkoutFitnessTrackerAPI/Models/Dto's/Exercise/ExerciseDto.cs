namespace WorkoutFitnessTracker.API.Models.Dto_s.Exercise
{
    public record ExerciseDto
    {
        public string Name { get; init; } = string.Empty;
        public string? Type { get; init; }
    }
}
