using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkoutFitnessTrackerAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddMuscleGroupToExercise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MuscleGroup",
                table: "Exercises",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MuscleGroup",
                table: "Exercises");
        }
    }
}
