using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using WorkoutFitnessTrackerAPI.Repositories;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using Dapper;
using System.Linq;

namespace WorkoutFitnessTrackerAPI.Tests.Repositories
{
    public class WorkoutRepositoryTests
    {
        private readonly Mock<IDbConnection> _dbConnectionMock;
        private readonly WorkoutRepository _workoutRepository;

        public WorkoutRepositoryTests()
        {
            // Налаштування mock для IDbConnection
            _dbConnectionMock = new Mock<IDbConnection>();
            _workoutRepository = new WorkoutRepository(_dbConnectionMock.Object);
        }

        [Fact]
        public async Task GetWorkoutsAsync_ShouldReturnWorkoutsWithExercises()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Симулюємо результат з типами Int64 для SQLite
            var sqlResult = new List<dynamic>
            {
                new { Date = new DateTime(2024, 10, 1), Duration = 60, ExerciseName = "Squat", Sets = 3L, Reps = 10L },  // 'L' означає long (Int64)
                new { Date = new DateTime(2024, 10, 1), Duration = 60, ExerciseName = "Bench Press", Sets = 3L, Reps = 8L },
                new { Date = new DateTime(2024, 10, 2), Duration = 45, ExerciseName = "Deadlift", Sets = 4L, Reps = 5L }
            };

            // Налаштування mock для Dapper (підміну робимо тільки в тесті)
            _dbConnectionMock
                .Setup(db => db.QueryAsync<WorkoutDto, WorkoutExerciseDto, WorkoutDto>(
                    It.IsAny<string>(),                                 // SQL запит
                    It.IsAny<Func<WorkoutDto, WorkoutExerciseDto, WorkoutDto>>(),  // Функція мапінгу
                    It.IsAny<object?>(),                                // Параметри (nullable)
                    It.IsAny<IDbTransaction?>(),                        // Транзакція (nullable)
                    It.IsAny<bool>(),                                   // Buffered
                    It.IsAny<string>(),                                 // SplitOn
                    It.IsAny<int?>(),                                   // Тайм-аут команди
                    It.IsAny<CommandType?>()))                          // Тип команди
                .ReturnsAsync((string sql, Func<WorkoutDto, WorkoutExerciseDto, WorkoutDto> mapFunc, object param, IDbTransaction trans, bool buffered, string splitOn, int? timeout, CommandType? commandType) =>
                {
                    var workoutDtos = new List<WorkoutDto>();

                    // Симулюємо поведінку Dapper (не змінюючи логіку API)
                    foreach (var result in sqlResult)
                    {
                        var workoutDto = new WorkoutDto((DateTime)result.Date, (int)result.Duration, new List<WorkoutExerciseDto>());

                        // Кастуємо Int64 до Int32 тільки в тесті
                        var workoutExerciseDto = new WorkoutExerciseDto(
                            (string)result.ExerciseName,
                            (int)(long)result.Sets,   // Кастинг Int64 до Int32
                            (int)(long)result.Reps    // Кастинг Int64 до Int32
                        );
                        mapFunc(workoutDto, workoutExerciseDto);
                        workoutDtos.Add(workoutDto);
                    }
                    return workoutDtos;
                });

            // Act
            var workouts = await _workoutRepository.GetWorkoutsAsync(userId);

            // Assert
            Assert.NotNull(workouts);
            Assert.Equal(2, workouts.Count()); // Перевірка на 2 різні тренування

            var firstWorkout = workouts.First();
            var secondWorkout = workouts.Last();

            // Перевірка першого тренування
            Assert.Equal(new DateTime(2024, 10, 1), firstWorkout.Date);
            Assert.Equal(60, firstWorkout.Duration);
            Assert.Equal(2, firstWorkout.Exercises.Count); // Два вправи для першого тренування
            Assert.Equal("Squat", firstWorkout.Exercises.First().ExerciseName);
            Assert.Equal(3, firstWorkout.Exercises.First().Sets);
            Assert.Equal("Bench Press", firstWorkout.Exercises.Last().ExerciseName);
            Assert.Equal(8, firstWorkout.Exercises.Last().Reps);

            // Перевірка другого тренування
            Assert.Equal(new DateTime(2024, 10, 2), secondWorkout.Date);
            Assert.Equal(45, secondWorkout.Duration);
            Assert.Single(secondWorkout.Exercises); // Одна вправа для другого тренування
            Assert.Equal("Deadlift", secondWorkout.Exercises.First().ExerciseName);
            Assert.Equal(4, secondWorkout.Exercises.First().Sets);
        }
    }
}
