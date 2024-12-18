﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutFitnessTrackerAPI.Data.Configurations.Extensions;
using WorkoutFitnessTrackerAPI.Models;

namespace WorkoutFitnessTrackerAPI.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasMany(u => u.Workouts)
                   .WithOne(w => w.User)
                   .HasForeignKey(w => w.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.WorkoutPlans)
                   .WithOne(wp => wp.User)
                   .HasForeignKey(wp => wp.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.ProgressRecords)
                   .WithOne(pr => pr.User)
                   .HasForeignKey(pr => pr.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
