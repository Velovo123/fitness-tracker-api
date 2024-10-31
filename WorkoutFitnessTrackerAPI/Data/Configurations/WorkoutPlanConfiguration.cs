using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutFitnessTrackerAPI.Data.Configurations.Extensions;
using WorkoutFitnessTrackerAPI.Models;

namespace WorkoutFitnessTrackerAPI.Data.Configurations
{
    public class WorkoutPlanConfiguration : IEntityTypeConfiguration<WorkoutPlan>
    {
        public void Configure(EntityTypeBuilder<WorkoutPlan> builder)
        {
            builder.ApplyIdConfiguration();

            builder.HasMany(wp => wp.WorkoutPlanExercises)
                   .WithOne(wpe => wpe.WorkoutPlan)
                   .HasForeignKey(wpe => wpe.WorkoutPlanId)
                   .OnDelete(DeleteBehavior.Cascade); 

            builder.HasOne(wp => wp.User)
                   .WithMany(u => u.WorkoutPlans)
                   .HasForeignKey(wp => wp.UserId)
                   .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}
