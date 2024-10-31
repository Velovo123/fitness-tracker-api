using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutFitnessTrackerAPI.Data.Configurations.Extensions;
using WorkoutFitnessTrackerAPI.Models;

namespace WorkoutFitnessTrackerAPI.Data.Configurations
{
    public class WorkoutPlanExerciseConfiguration : IEntityTypeConfiguration<WorkoutPlanExercise>
    {
        public void Configure(EntityTypeBuilder<WorkoutPlanExercise> builder)
        {
            builder.ApplyIdConfiguration();

            builder.HasOne(wpe => wpe.WorkoutPlan)
                   .WithMany(wp => wp.WorkoutPlanExercises)
                   .HasForeignKey(wpe => wpe.WorkoutPlanId)
                   .OnDelete(DeleteBehavior.Cascade); 

            builder.HasOne(wpe => wpe.Exercise)
                   .WithMany(e => e.WorkoutPlanExercises)
                   .HasForeignKey(wpe => wpe.ExerciseId)
                   .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
