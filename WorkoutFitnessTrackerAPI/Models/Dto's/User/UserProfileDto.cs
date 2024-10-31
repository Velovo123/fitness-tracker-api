namespace WorkoutFitnessTrackerAPI.Models.Dto_s.User
{
    public record UserProfileDto
    {
        public string Name { get; init; } = default!;

        public string Email { get; init; } = default!;
    }
}
