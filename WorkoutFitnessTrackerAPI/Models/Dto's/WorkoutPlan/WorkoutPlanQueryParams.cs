﻿using System.ComponentModel.DataAnnotations;

namespace WorkoutFitnessTrackerAPI.Models.Dto_s
{
    public class WorkoutPlanQueryParams
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0.")]
        public int? PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100.")]
        public int? PageSize { get; set; } = 10;

        [MaxLength(50, ErrorMessage = "Sort by cannot exceed 50 characters.")]
        public string? SortBy { get; set; }

        public bool? SortDescending { get; set; } = false;

        public string? Goal { get; init; }
    }
}