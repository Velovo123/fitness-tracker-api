using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkoutFitnessTrackerAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangeExerciseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryMuscle",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "SecondaryMuscle",
                table: "Exercises");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryMuscles",
                table: "Exercises",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "SecondaryMuscles",
                table: "Exercises",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryMuscles",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "SecondaryMuscles",
                table: "Exercises");

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
    }
}
