using System.Text.Json.Serialization;

namespace WorkoutFitnessTracker.API.Models.Dto_s.Exercise
{
    public record ExerciseDto
    {
        public string Name { get; init; } = string.Empty;

        public string? Category { get; init; }

        public string PrimaryMuscles { get; init; } = string.Empty;

    }
}
