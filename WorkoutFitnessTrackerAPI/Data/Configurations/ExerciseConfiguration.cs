using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutFitnessTrackerAPI.Data.Configurations.Extensions;
using WorkoutFitnessTrackerAPI.Models;

namespace WorkoutFitnessTrackerAPI.Data.Configurations
{
    public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
    {
        public void Configure(EntityTypeBuilder<Exercise> builder)
        {
            builder.ApplyIdConfiguration();

            builder.HasMany(e => e.WorkoutExercises)
                   .WithOne(we => we.Exercise)
                   .HasForeignKey(we => we.ExerciseId)
                   .OnDelete(DeleteBehavior.Restrict); 

            builder.HasMany(e => e.WorkoutPlanExercises)
                   .WithOne(wpe => wpe.Exercise)
                   .HasForeignKey(wpe => wpe.ExerciseId)
                   .OnDelete(DeleteBehavior.Restrict); 

            builder.HasMany(e => e.ProgressRecords)
                   .WithOne(pr => pr.Exercise)
                   .HasForeignKey(pr => pr.ExerciseId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => e.Name).IsUnique();
        }
    }
}
