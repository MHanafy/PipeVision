using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PipeVision.Data.Migrations
{
    public partial class CreateTestRunsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tests_PipelineJobs_PipelineJobId",
                table: "Tests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tests",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "PipelineJobId",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "CallStack",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "Error",
                table: "Tests");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tests",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Tests",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tests",
                table: "Tests",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "TestRuns",
                columns: table => new
                {
                    Test1Id = table.Column<int>(nullable: false),
                    PipelineJobId = table.Column<int>(nullable: false),
                    Error = table.Column<string>(nullable: true),
                    CallStack = table.Column<string>(nullable: true),
                    Duration = table.Column<long>(nullable: false),
                    TestId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestRuns", x => new { x.Test1Id, x.PipelineJobId });
                    table.ForeignKey(
                        name: "FK_TestRuns_PipelineJobs_PipelineJobId",
                        column: x => x.PipelineJobId,
                        principalTable: "PipelineJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestRuns_Tests_TestId",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tests_Name",
                table: "Tests",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TestRuns_PipelineJobId",
                table: "TestRuns",
                column: "PipelineJobId");

            migrationBuilder.CreateIndex(
                name: "IX_TestRuns_TestId",
                table: "TestRuns",
                column: "TestId");

            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestRuns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tests",
                table: "Tests");

            migrationBuilder.DropIndex(
                name: "IX_Tests_Name",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Tests");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tests",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PipelineJobId",
                table: "Tests",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CallStack",
                table: "Tests",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Duration",
                table: "Tests",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Error",
                table: "Tests",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tests",
                table: "Tests",
                columns: new[] { "PipelineJobId", "Name" });

            migrationBuilder.AddForeignKey(
                name: "FK_Tests_PipelineJobs_PipelineJobId",
                table: "Tests",
                column: "PipelineJobId",
                principalTable: "PipelineJobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
