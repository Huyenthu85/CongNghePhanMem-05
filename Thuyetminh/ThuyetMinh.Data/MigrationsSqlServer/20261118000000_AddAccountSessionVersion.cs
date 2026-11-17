using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThuyetMinh.Data.MigrationsSqlServer;

public partial class AddAccountSessionVersion : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "SessionVersion", table: "Users", type: "int",
            nullable: false, defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "SessionVersion", table: "Users");
    }
}
