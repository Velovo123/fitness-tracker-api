namespace WorkoutFitnessTrackerAPI.Helpers
{
    public static class NameNormalizationHelper
    {
        public static string NormalizeName(string name)
        {
            return string.Concat(name.ToLower().Where(char.IsLetterOrDigit));
        }
    }
}
