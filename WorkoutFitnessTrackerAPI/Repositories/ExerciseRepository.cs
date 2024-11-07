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
    }
}
