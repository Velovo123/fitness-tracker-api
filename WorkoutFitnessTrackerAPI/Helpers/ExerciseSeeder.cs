using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;

namespace WorkoutFitnessTracker.API.Helpers
{
    public static class ExerciseSeeder
    {
        public static async Task SeedExercisesAsync(DbContext context, string scriptPath)
        {
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"SQL seed file not found at {scriptPath}");
            }

            var sql = await File.ReadAllTextAsync(scriptPath);

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await context.Database.ExecuteSqlRawAsync(sql);
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
