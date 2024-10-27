using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkoutFitnessTrackerAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserExerciseRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_AspNetUsers_UserId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_UserId",
                table: "Exercises");

            migrationBuilder.CreateTable(
                name: "UserExercises",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserExercises", x => new { x.UserId, x.ExerciseId });
                    table.ForeignKey(
                        name: "FK_UserExercises_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserExercises_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Name",
                table: "Exercises",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserExercises_ExerciseId",
                table: "UserExercises",
                column: "ExerciseId");

            migrationBuilder.Sql(@"
                INSERT INTO UserExercises (UserId, ExerciseId)
                SELECT UserId, Id FROM Exercises WHERE UserId IS NOT NULL;");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Exercises");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserExercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_Name",
                table: "Exercises");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Exercises",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_UserId",
                table: "Exercises",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_AspNetUsers_UserId",
                table: "Exercises",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
