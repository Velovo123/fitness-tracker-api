using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkoutFitnessTrackerAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExerciseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MuscleGroup",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Exercises");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Exercises",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryMuscle",
                table: "Exercises",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondaryMuscle",
                table: "Exercises",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "PrimaryMuscle",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "SecondaryMuscle",
                table: "Exercises");

            migrationBuilder.AddColumn<string>(
                name: "MuscleGroup",
                table: "Exercises",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Exercises",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
