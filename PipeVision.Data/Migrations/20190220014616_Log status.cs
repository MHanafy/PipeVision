using Microsoft.EntityFrameworkCore.Migrations;

namespace PipeVision.Data.Migrations
{
    public partial class Logstatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LogStatus",
                table: "PipelineJobs",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogStatus",
                table: "PipelineJobs");
        }
    }
}
