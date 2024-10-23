using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkoutFitnessTrackerAPI.Data.Configurations;
using WorkoutFitnessTrackerAPI.Models;

namespace WorkoutFitnessTrackerAPI.Data
{
    public class WFTDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public WFTDbContext(DbContextOptions<WFTDbContext> options)
                    : base(options)
        {
        }

        public DbSet<Workout> Workouts { get; set; }
        public DbSet<WorkoutExercise> WorkoutExercises { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<WorkoutPlan> WorkoutPlans { get; set; }
        public DbSet<WorkoutPlanExercise> WorkoutPlanExercises { get; set; }
        public DbSet<ProgressRecord> ProgressRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new WorkoutExerciseConfiguration());
            modelBuilder.ApplyConfiguration(new ExerciseConfiguration());
            modelBuilder.ApplyConfiguration(new WorkoutPlanConfiguration());
            modelBuilder.ApplyConfiguration(new WorkoutPlanExerciseConfiguration());
            modelBuilder.ApplyConfiguration(new ProgressRecordConfiguration());
            modelBuilder.ApplyConfiguration(new WorkoutConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
