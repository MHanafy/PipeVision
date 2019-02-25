using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PipeVision.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChangeLists",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    UserName = table.Column<string>(nullable: true),
                    Comment = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pipelines",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Counter = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    InProgress = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pipelines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PipelineChangeLists",
                columns: table => new
                {
                    PipelineId = table.Column<int>(nullable: false),
                    ChangelistId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineChangeLists", x => new { x.PipelineId, x.ChangelistId });
                    table.ForeignKey(
                        name: "FK_PipelineChangeLists_ChangeLists_ChangelistId",
                        column: x => x.ChangelistId,
                        principalTable: "ChangeLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PipelineChangeLists_Pipelines_PipelineId",
                        column: x => x.PipelineId,
                        principalTable: "Pipelines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PipelineJobs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    TestType = table.Column<int>(nullable: false),
                    Result = table.Column<string>(nullable: true),
                    StageName = table.Column<string>(nullable: true),
                    StageCounter = table.Column<int>(nullable: false),
                    Agent = table.Column<string>(nullable: true),
                    PipelineId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PipelineJobs_Pipelines_PipelineId",
                        column: x => x.PipelineId,
                        principalTable: "Pipelines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tests",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    PipelineJobId = table.Column<int>(nullable: false),
                    Error = table.Column<string>(nullable: true),
                    CallStack = table.Column<string>(nullable: true),
                    Duration = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests", x => new { x.PipelineJobId, x.Name });
                    table.ForeignKey(
                        name: "FK_Tests_PipelineJobs_PipelineJobId",
                        column: x => x.PipelineJobId,
                        principalTable: "PipelineJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PipelineChangeLists_ChangelistId",
                table: "PipelineChangeLists",
                column: "ChangelistId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineJobs_PipelineId",
                table: "PipelineJobs",
                column: "PipelineId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PipelineChangeLists");

            migrationBuilder.DropTable(
                name: "Tests");

            migrationBuilder.DropTable(
                name: "ChangeLists");

            migrationBuilder.DropTable(
                name: "PipelineJobs");

            migrationBuilder.DropTable(
                name: "Pipelines");
        }
    }
}
