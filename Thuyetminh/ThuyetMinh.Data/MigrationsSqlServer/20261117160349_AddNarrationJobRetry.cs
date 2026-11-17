using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThuyetMinh.Data.MigrationsSqlServer
{
    /// <inheritdoc />
    public partial class AddNarrationJobRetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                table: "ProcessingJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAttemptAt",
                table: "ProcessingJobs",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttemptCount",
                table: "ProcessingJobs");

            migrationBuilder.DropColumn(
                name: "LastAttemptAt",
                table: "ProcessingJobs");
        }
    }
}
