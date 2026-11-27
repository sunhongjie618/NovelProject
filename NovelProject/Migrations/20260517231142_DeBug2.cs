using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovelProject.Migrations
{
    public partial class DeBug2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_Chapters_NovelId_ChapterNumber_IsDeleted",
                table: "T_Chapters");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RegisterTime",
                table: "T_Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 455, DateTimeKind.Utc).AddTicks(7038),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 208, DateTimeKind.Utc).AddTicks(9445));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastReadTime",
                table: "T_UserReadingRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 457, DateTimeKind.Utc).AddTicks(1828),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 210, DateTimeKind.Utc).AddTicks(2159));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_UserFavorites",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 456, DateTimeKind.Utc).AddTicks(2233),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 209, DateTimeKind.Utc).AddTicks(4533));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 454, DateTimeKind.Utc).AddTicks(8310),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 208, DateTimeKind.Utc).AddTicks(1597));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 454, DateTimeKind.Utc).AddTicks(8710),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 208, DateTimeKind.Utc).AddTicks(1877));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_NovelComments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 451, DateTimeKind.Utc).AddTicks(4947),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 205, DateTimeKind.Utc).AddTicks(7986));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_CommentLikes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 449, DateTimeKind.Utc).AddTicks(1021),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 203, DateTimeKind.Utc).AddTicks(6916));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Chapters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 448, DateTimeKind.Utc).AddTicks(1397),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 202, DateTimeKind.Utc).AddTicks(8978));

            migrationBuilder.CreateIndex(
                name: "IX_T_Chapters_NovelId_ChapterNumber",
                table: "T_Chapters",
                columns: new[] { "NovelId", "ChapterNumber" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_Chapters_NovelId_ChapterNumber",
                table: "T_Chapters");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RegisterTime",
                table: "T_Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 208, DateTimeKind.Utc).AddTicks(9445),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 455, DateTimeKind.Utc).AddTicks(7038));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastReadTime",
                table: "T_UserReadingRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 210, DateTimeKind.Utc).AddTicks(2159),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 457, DateTimeKind.Utc).AddTicks(1828));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_UserFavorites",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 209, DateTimeKind.Utc).AddTicks(4533),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 456, DateTimeKind.Utc).AddTicks(2233));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 208, DateTimeKind.Utc).AddTicks(1597),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 454, DateTimeKind.Utc).AddTicks(8310));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 208, DateTimeKind.Utc).AddTicks(1877),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 454, DateTimeKind.Utc).AddTicks(8710));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_NovelComments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 205, DateTimeKind.Utc).AddTicks(7986),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 451, DateTimeKind.Utc).AddTicks(4947));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_CommentLikes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 203, DateTimeKind.Utc).AddTicks(6916),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 449, DateTimeKind.Utc).AddTicks(1021));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Chapters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 202, DateTimeKind.Utc).AddTicks(8978),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 448, DateTimeKind.Utc).AddTicks(1397));

            migrationBuilder.CreateIndex(
                name: "IX_T_Chapters_NovelId_ChapterNumber_IsDeleted",
                table: "T_Chapters",
                columns: new[] { "NovelId", "ChapterNumber", "IsDeleted" },
                unique: true);
        }
    }
}
