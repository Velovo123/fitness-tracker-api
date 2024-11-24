namespace WorkoutFitnessTracker.API.Models.Dto_s.Summary
{
    public record ExerciseProgressDto
    {
        public string ExerciseName { get; init; }
        public int PlannedSets { get; init; }
        public int PlannedReps { get; init; }
        public int ActualSets { get; init; }
        public int ActualReps { get; init; }
    }
}
