using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovelProject.Migrations
{
    public partial class DeBugChapter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                oldDefaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 208, DateTimeKind.Utc).AddTicks(7046));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastReadTime",
                table: "T_UserReadingRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 210, DateTimeKind.Utc).AddTicks(2159),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 210, DateTimeKind.Utc).AddTicks(4615));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_UserFavorites",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 209, DateTimeKind.Utc).AddTicks(4533),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 209, DateTimeKind.Utc).AddTicks(932));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 208, DateTimeKind.Utc).AddTicks(1597),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 207, DateTimeKind.Utc).AddTicks(9025));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 208, DateTimeKind.Utc).AddTicks(1877),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 207, DateTimeKind.Utc).AddTicks(9306));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_NovelComments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 205, DateTimeKind.Utc).AddTicks(7986),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 205, DateTimeKind.Utc).AddTicks(2816));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_CommentLikes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 203, DateTimeKind.Utc).AddTicks(6916),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 202, DateTimeKind.Utc).AddTicks(9022));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Chapters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 202, DateTimeKind.Utc).AddTicks(8978),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 201, DateTimeKind.Utc).AddTicks(8693));

            migrationBuilder.CreateIndex(
                name: "IX_T_Chapters_NovelId_ChapterNumber_IsDeleted",
                table: "T_Chapters",
                columns: new[] { "NovelId", "ChapterNumber", "IsDeleted" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_Chapters_NovelId_ChapterNumber_IsDeleted",
                table: "T_Chapters");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RegisterTime",
                table: "T_Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 208, DateTimeKind.Utc).AddTicks(7046),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 208, DateTimeKind.Utc).AddTicks(9445));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastReadTime",
                table: "T_UserReadingRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 210, DateTimeKind.Utc).AddTicks(4615),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 210, DateTimeKind.Utc).AddTicks(2159));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_UserFavorites",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 209, DateTimeKind.Utc).AddTicks(932),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 209, DateTimeKind.Utc).AddTicks(4533));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 207, DateTimeKind.Utc).AddTicks(9025),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 208, DateTimeKind.Utc).AddTicks(1597));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 207, DateTimeKind.Utc).AddTicks(9306),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 208, DateTimeKind.Utc).AddTicks(1877));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_NovelComments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 205, DateTimeKind.Utc).AddTicks(2816),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 205, DateTimeKind.Utc).AddTicks(7986));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_CommentLikes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 202, DateTimeKind.Utc).AddTicks(9022),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 203, DateTimeKind.Utc).AddTicks(6916));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Chapters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 16, 3, 18, 5, 201, DateTimeKind.Utc).AddTicks(8693),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 0, 49, 202, DateTimeKind.Utc).AddTicks(8978));

            migrationBuilder.CreateIndex(
                name: "IX_T_Chapters_NovelId_ChapterNumber",
                table: "T_Chapters",
                columns: new[] { "NovelId", "ChapterNumber" },
                unique: true);
        }
    }
}
