using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutFitnessTrackerAPI.Data.Configurations.Extensions;
using WorkoutFitnessTrackerAPI.Models;

namespace WorkoutFitnessTrackerAPI.Data.Configurations
{
    public class WorkoutExerciseConfiguration : IEntityTypeConfiguration<WorkoutExercise>
    {
        public void Configure(EntityTypeBuilder<WorkoutExercise> builder)
        {
            builder.ApplyIdConfiguration();

            builder.HasOne(we => we.Workout)
                .WithMany(w => w.WorkoutExercises)
                .HasForeignKey(we => we.WorkoutId)
                .OnDelete(DeleteBehavior.Cascade); 

            builder.HasOne(we => we.Exercise)
                   .WithMany(e => e.WorkoutExercises)
                   .HasForeignKey(we => we.ExerciseId)
                   .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
