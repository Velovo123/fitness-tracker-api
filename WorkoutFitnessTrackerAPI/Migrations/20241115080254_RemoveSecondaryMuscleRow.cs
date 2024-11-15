using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkoutFitnessTrackerAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSecondaryMuscleRow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecondaryMuscles",
                table: "Exercises");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecondaryMuscles",
                table: "Exercises",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
