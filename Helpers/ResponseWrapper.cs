namespace WorkoutFitnessTrackerAPI.Helpers
{
    public class ResponseWrapper<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }

        public ResponseWrapper(bool success, T? data = default, string? message = null)
        {
            Success = success;
            Data = data;
            Message = message;
        }
    }
}
