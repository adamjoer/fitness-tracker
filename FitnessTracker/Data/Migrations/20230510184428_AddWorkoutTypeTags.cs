using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessTracker.Data.Migrations
{
    public partial class AddWorkoutTypeTags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkoutTypeTags",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PlanId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TypeId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutTypeTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutTypeTags_FitnessPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "FitnessPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkoutTypeTags_WorkoutTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "WorkoutTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutTypeTags_PlanId",
                table: "WorkoutTypeTags",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutTypeTags_TypeId",
                table: "WorkoutTypeTags",
                column: "TypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkoutTypeTags");
        }
    }
}
