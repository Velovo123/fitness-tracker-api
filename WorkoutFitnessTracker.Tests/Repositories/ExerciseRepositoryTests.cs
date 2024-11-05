using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkoutFitnessTracker.API.Mappings;
using WorkoutFitnessTracker.API.Models.Dto_s.Exercise;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using Xunit;

namespace WorkoutFitnessTracker.Tests.Repositories
{
    public class ExerciseRepositoryTests
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly WFTDbContext _context;
        private readonly IMapper _mapper;

        public ExerciseRepositoryTests()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ExerciseProfile());
            });
            _mapper = mapperConfig.CreateMapper();

            var options = new DbContextOptionsBuilder<WFTDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new WFTDbContext(options);
            _exerciseRepository = new ExerciseRepository(_context, _mapper);
        }

        [Fact]
        public async Task AddAsync_ShouldAddExercise_WhenExerciseIsValid()
        {
            // Arrange
            var exerciseDto = new ExerciseDto
            {
                Name = "Push Up",
                Type = "Strength"
            };

            // Act
            var result = await _exerciseRepository.AddAsync(exerciseDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Push Up", result.Name);
            Assert.Equal("Strength", result.Type);

            // Verify that it was added to the database
            var addedExercise = await _context.Exercises.FirstOrDefaultAsync(e => e.Name == "Push Up");
            Assert.NotNull(addedExercise);
        }

        [Fact]
        public async Task AddAsync_ShouldThrowException_WhenDuplicateExerciseNameExists()
        {
            // Arrange
            var exerciseDto = new ExerciseDto { Name = "Bench Press", Type = "Strength" };
            await _exerciseRepository.AddAsync(exerciseDto);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _exerciseRepository.AddAsync(exerciseDto));
        }

        [Fact]
        public async Task GetByNameAsync_ShouldReturnExercise_WhenExerciseExists()
        {
            // Arrange
            var exercise = new Exercise { Name = "Squat", Type = "Strength" };
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            // Act
            var result = await _exerciseRepository.GetByNameAsync("Squat");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Squat", result.Name);
            Assert.Equal("Strength", result.Type);
        }

        [Fact]
        public async Task GetByNameAsync_ShouldReturnNull_WhenExerciseDoesNotExist()
        {
            // Act
            var result = await _exerciseRepository.GetByNameAsync("NonExistentExercise");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllExercisesAsync_ShouldReturnAllExercises()
        {
            // Arrange
            var exercises = new List<Exercise>
            {
                new Exercise { Name = "Push Up", Type = "Strength" },
                new Exercise { Name = "Pull Up", Type = "Strength" },
                new Exercise { Name = "Squat", Type = "Strength" }
            };
            _context.Exercises.AddRange(exercises);
            await _context.SaveChangesAsync();

            // Act
            var result = await _exerciseRepository.GetAllExercisesAsync();

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Contains(result, e => e.Name == "Push Up");
            Assert.Contains(result, e => e.Name == "Pull Up");
            Assert.Contains(result, e => e.Name == "Squat");
        }
    }
}
