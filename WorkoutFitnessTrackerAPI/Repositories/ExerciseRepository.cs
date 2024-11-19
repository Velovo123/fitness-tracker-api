using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkoutFitnessTrackerAPI.Repositories
{
    public class ExerciseRepository : IExerciseRepository
    {
        private readonly WFTDbContext _context;

        public ExerciseRepository(WFTDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<string>> GetAllExerciseNamesAsync()
        {
            return await _context.Exercises
                .AsNoTracking()
                .Select(e => e.Name)
                .ToListAsync();
        }

        public async Task<Exercise?> GetByNameAsync(string name)
        {
            return await _context.Exercises
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Name == name);
        }

        public async Task<Exercise> AddAsync(Exercise exercise)
        {
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();
            return exercise;
        }

        public async Task<IEnumerable<Exercise>> GetAllExercisesAsync()
        {
            return await _context.Exercises
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task EnsureUserExerciseLinkAsync(Guid userId, Guid exerciseId)
        {
            // Check if the link between the user and exercise already exists
            var linkExists = await _context.UserExercises
                .AnyAsync(ue => ue.UserId == userId && ue.ExerciseId == exerciseId);

            // If the link does not exist, create it
            if (!linkExists)
            {
                _context.UserExercises.Add(new UserExercise
                {
                    UserId = userId,
                    ExerciseId = exerciseId
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Exercise>> GetUserLinkedExercisesAsync(Guid userId)
        {
            return await _context.UserExercises
                .Where(ue => ue.UserId == userId)
                .Include(ue => ue.Exercise)
                .Select(ue => ue.Exercise)
                .AsNoTracking()
                .ToListAsync();
        }

    }
}
