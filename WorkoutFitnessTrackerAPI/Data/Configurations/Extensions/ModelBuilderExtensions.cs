using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutFitnessTrackerAPI.Models;

namespace WorkoutFitnessTrackerAPI.Data.Configurations.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static void ApplyIdConfiguration<T>(this EntityTypeBuilder<T> builder) where T : BaseEntity
        {
            builder.Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");
        }
    }
}
