namespace WorkoutFitnessTrackerAPI.Models.Dto_s
{
    public record AuthResult(bool Success, string? Token = null, List<string>? Errors = null);
    
    
}
