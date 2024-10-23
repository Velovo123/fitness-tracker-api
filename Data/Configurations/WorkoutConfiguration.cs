using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutFitnessTrackerAPI.Data.Configurations.Extensions;
using WorkoutFitnessTrackerAPI.Models;

namespace WorkoutFitnessTrackerAPI.Data.Configurations
{
    public class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
    {
        public void Configure(EntityTypeBuilder<Workout> builder)
        {
            builder.ApplyIdConfiguration();

            builder.HasMany(w => w.WorkoutExercises)
               .WithOne(we => we.Workout)
               .HasForeignKey(we => we.WorkoutId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(w => w.User)
               .WithMany(u => u.Workouts)
               .HasForeignKey(w => w.UserId)
               .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}
