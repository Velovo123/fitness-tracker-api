using AutoMapper;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkoutFitnessTracker.API.Models.Dto_s.Exercise;

namespace WorkoutFitnessTrackerAPI.Repositories
{
    public class ExerciseRepository : IExerciseRepository
    {
        private readonly WFTDbContext _context;
        private readonly IMapper _mapper;

        public ExerciseRepository(WFTDbContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ExerciseDto?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Exercise name cannot be null or whitespace.", nameof(name));

            var exercise = await _context.Exercises
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Name == name);

            return exercise == null ? null : _mapper.Map<ExerciseDto>(exercise);
        }

        public async Task<ExerciseDto> AddAsync(ExerciseDto exerciseDto)
        {
            if (exerciseDto == null)
                throw new ArgumentNullException(nameof(exerciseDto), "Cannot add a null exercise DTO.");

            var existingExercise = await _context.Exercises
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Name == exerciseDto.Name);

            if (existingExercise != null)
                throw new InvalidOperationException($"An exercise with the name '{exerciseDto.Name}' already exists.");

            var exercise = _mapper.Map<Exercise>(exerciseDto);
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            return _mapper.Map<ExerciseDto>(exercise); // Return the mapped DTO
        }

        public async Task<IEnumerable<ExerciseDto>> GetAllExercisesAsync()
        {
            var exercises = await _context.Exercises
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<ExerciseDto>>(exercises);
        }
    }
}
