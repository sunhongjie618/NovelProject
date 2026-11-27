using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovelProject.Migrations
{
    public partial class UpdateCoverUrlLength : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "RegisterTime",
                table: "T_Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 208, DateTimeKind.Utc).AddTicks(7046),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 585, DateTimeKind.Utc).AddTicks(7780));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastReadTime",
                table: "T_UserReadingRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 210, DateTimeKind.Utc).AddTicks(4615),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 586, DateTimeKind.Utc).AddTicks(6821));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_UserFavorites",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 209, DateTimeKind.Utc).AddTicks(932),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 586, DateTimeKind.Utc).AddTicks(876));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 207, DateTimeKind.Utc).AddTicks(9025),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 585, DateTimeKind.Utc).AddTicks(1245));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 207, DateTimeKind.Utc).AddTicks(9306),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 585, DateTimeKind.Utc).AddTicks(1521));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_NovelComments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 205, DateTimeKind.Utc).AddTicks(2816),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 582, DateTimeKind.Utc).AddTicks(5917));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_CommentLikes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 202, DateTimeKind.Utc).AddTicks(9022),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 580, DateTimeKind.Utc).AddTicks(3390));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Chapters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 201, DateTimeKind.Utc).AddTicks(8693),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 579, DateTimeKind.Utc).AddTicks(4311));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "RegisterTime",
                table: "T_Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 585, DateTimeKind.Utc).AddTicks(7780),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 208, DateTimeKind.Utc).AddTicks(7046));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastReadTime",
                table: "T_UserReadingRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 586, DateTimeKind.Utc).AddTicks(6821),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 210, DateTimeKind.Utc).AddTicks(4615));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_UserFavorites",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 586, DateTimeKind.Utc).AddTicks(876),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 209, DateTimeKind.Utc).AddTicks(932));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 585, DateTimeKind.Utc).AddTicks(1245),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 207, DateTimeKind.Utc).AddTicks(9025));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 585, DateTimeKind.Utc).AddTicks(1521),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 207, DateTimeKind.Utc).AddTicks(9306));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_NovelComments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 582, DateTimeKind.Utc).AddTicks(5917),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 205, DateTimeKind.Utc).AddTicks(2816));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_CommentLikes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 580, DateTimeKind.Utc).AddTicks(3390),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 202, DateTimeKind.Utc).AddTicks(9022));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Chapters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 579, DateTimeKind.Utc).AddTicks(4311),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 201, DateTimeKind.Utc).AddTicks(8693));
        }
    }
}
