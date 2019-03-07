using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PipeVision.Data.Migrations.PipelineArchive
{
    public partial class Archive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Arc_ChangeLists",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    UserName = table.Column<string>(nullable: true),
                    Comment = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arc_ChangeLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Arc_Pipelines",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Counter = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    InProgress = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arc_Pipelines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Arc_Tests",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arc_Tests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Arc_PipelineChangeLists",
                columns: table => new
                {
                    PipelineId = table.Column<int>(nullable: false),
                    ChangelistId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arc_PipelineChangeLists", x => new { x.PipelineId, x.ChangelistId });
                    table.ForeignKey(
                        name: "FK_Arc_PipelineChangeLists_Arc_ChangeLists_ChangelistId",
                        column: x => x.ChangelistId,
                        principalTable: "Arc_ChangeLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Arc_PipelineChangeLists_Arc_Pipelines_PipelineId",
                        column: x => x.PipelineId,
                        principalTable: "Arc_Pipelines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Arc_PipelineJobs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    TestType = table.Column<int>(nullable: false),
                    LogStatus = table.Column<int>(nullable: false),
                    Result = table.Column<string>(nullable: true),
                    StageName = table.Column<string>(nullable: true),
                    StageCounter = table.Column<int>(nullable: false),
                    Agent = table.Column<string>(nullable: true),
                    PipelineId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arc_PipelineJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Arc_PipelineJobs_Arc_Pipelines_PipelineId",
                        column: x => x.PipelineId,
                        principalTable: "Arc_Pipelines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Arc_TestRuns",
                columns: table => new
                {
                    TestId = table.Column<int>(nullable: false),
                    PipelineJobId = table.Column<int>(nullable: false),
                    Error = table.Column<string>(nullable: true),
                    CallStack = table.Column<string>(nullable: true),
                    Duration = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arc_TestRuns", x => new { x.TestId, x.PipelineJobId });
                    table.ForeignKey(
                        name: "FK_Arc_TestRuns_Arc_PipelineJobs_PipelineJobId",
                        column: x => x.PipelineJobId,
                        principalTable: "Arc_PipelineJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Arc_TestRuns_Arc_Tests_TestId",
                        column: x => x.TestId,
                        principalTable: "Arc_Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Arc_PipelineChangeLists_ChangelistId",
                table: "Arc_PipelineChangeLists",
                column: "ChangelistId");

            migrationBuilder.CreateIndex(
                name: "IX_Arc_PipelineJobs_PipelineId",
                table: "Arc_PipelineJobs",
                column: "PipelineId");

            migrationBuilder.CreateIndex(
                name: "IX_Arc_TestRuns_PipelineJobId",
                table: "Arc_TestRuns",
                column: "PipelineJobId");

            migrationBuilder.CreateIndex(
                name: "IX_Arc_Tests_Name",
                table: "Arc_Tests",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Arc_PipelineChangeLists");

            migrationBuilder.DropTable(
                name: "Arc_TestRuns");

            migrationBuilder.DropTable(
                name: "Arc_ChangeLists");

            migrationBuilder.DropTable(
                name: "Arc_PipelineJobs");

            migrationBuilder.DropTable(
                name: "Arc_Tests");

            migrationBuilder.DropTable(
                name: "Arc_Pipelines");
        }
    }
}
