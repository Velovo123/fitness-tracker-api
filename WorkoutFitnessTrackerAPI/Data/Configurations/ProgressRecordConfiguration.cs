using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutFitnessTrackerAPI.Data.Configurations.Extensions;
using WorkoutFitnessTrackerAPI.Models;

namespace WorkoutFitnessTrackerAPI.Data.Configurations
{
    public class ProgressRecordConfiguration : IEntityTypeConfiguration<ProgressRecord>
    {
        public void Configure(EntityTypeBuilder<ProgressRecord> builder)
        {
            builder.ApplyIdConfiguration();

            builder.HasOne(pr => pr.User)
                   .WithMany(u => u.ProgressRecords)
                   .HasForeignKey(pr => pr.UserId)
                   .OnDelete(DeleteBehavior.Cascade); 

            builder.HasOne(pr => pr.Exercise)
                   .WithMany(e => e.ProgressRecords)
                   .HasForeignKey(pr => pr.ExerciseId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
