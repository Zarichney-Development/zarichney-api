using Microsoft.EntityFrameworkCore.Migrations;

namespace Zarichney.Server.Auth.Migrations;

public partial class AddDeviceInfoToRefreshTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "DeviceName",
            table: "RefreshTokens",
            nullable: true);
            
        migrationBuilder.AddColumn<string>(
            name: "DeviceIp",
            table: "RefreshTokens",
            nullable: true);
            
        migrationBuilder.AddColumn<string>(
            name: "UserAgent",
            table: "RefreshTokens",
            nullable: true);
            
        migrationBuilder.AddColumn<DateTime>(
            name: "LastUsedAt",
            table: "RefreshTokens",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DeviceName",
            table: "RefreshTokens");
            
        migrationBuilder.DropColumn(
            name: "DeviceIp",
            table: "RefreshTokens");
            
        migrationBuilder.DropColumn(
            name: "UserAgent",
            table: "RefreshTokens");
            
        migrationBuilder.DropColumn(
            name: "LastUsedAt",
            table: "RefreshTokens");
    }
}