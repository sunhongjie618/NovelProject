using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovelProject.Migrations
{
    public partial class AddCommentLikeTab : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "RegisterTime",
                table: "T_Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 585, DateTimeKind.Utc).AddTicks(7780),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 9, 11, 50, 14, 979, DateTimeKind.Utc).AddTicks(7986));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastReadTime",
                table: "T_UserReadingRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 586, DateTimeKind.Utc).AddTicks(6821),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 9, 11, 50, 14, 980, DateTimeKind.Utc).AddTicks(5220));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_UserFavorites",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 586, DateTimeKind.Utc).AddTicks(876),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 9, 11, 50, 14, 980, DateTimeKind.Utc).AddTicks(590));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 585, DateTimeKind.Utc).AddTicks(1245),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 9, 11, 50, 14, 979, DateTimeKind.Utc).AddTicks(2652));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 585, DateTimeKind.Utc).AddTicks(1521),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 9, 11, 50, 14, 979, DateTimeKind.Utc).AddTicks(2950));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_NovelComments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 582, DateTimeKind.Utc).AddTicks(5917),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 9, 11, 50, 14, 977, DateTimeKind.Utc).AddTicks(1667));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Chapters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 579, DateTimeKind.Utc).AddTicks(4311),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 9, 11, 50, 14, 976, DateTimeKind.Utc).AddTicks(4257));

            migrationBuilder.CreateTable(
                name: "T_CommentLikes",
                columns: table => new
                {
                    LikeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CommentId = table.Column<long>(type: "bigint", nullable: false),
                    NovelId = table.Column<long>(type: "bigint", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 580, DateTimeKind.Utc).AddTicks(3390))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_CommentLikes", x => x.LikeId);
                    table.ForeignKey(
                        name: "FK_T_CommentLikes_T_NovelComments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "T_NovelComments",
                        principalColumn: "CommentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_T_CommentLikes_T_Novels_NovelId",
                        column: x => x.NovelId,
                        principalTable: "T_Novels",
                        principalColumn: "NovelId");
                    table.ForeignKey(
                        name: "FK_T_CommentLikes_T_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "T_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_CommentLikes_CommentId",
                table: "T_CommentLikes",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_T_CommentLikes_NovelId",
                table: "T_CommentLikes",
                column: "NovelId");

            migrationBuilder.CreateIndex(
                name: "IX_T_CommentLikes_UserId_CommentId",
                table: "T_CommentLikes",
                columns: new[] { "UserId", "CommentId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_CommentLikes");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RegisterTime",
                table: "T_Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 9, 11, 50, 14, 979, DateTimeKind.Utc).AddTicks(7986),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 585, DateTimeKind.Utc).AddTicks(7780));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastReadTime",
                table: "T_UserReadingRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 9, 11, 50, 14, 980, DateTimeKind.Utc).AddTicks(5220),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 586, DateTimeKind.Utc).AddTicks(6821));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_UserFavorites",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 9, 11, 50, 14, 980, DateTimeKind.Utc).AddTicks(590),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 586, DateTimeKind.Utc).AddTicks(876));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 9, 11, 50, 14, 979, DateTimeKind.Utc).AddTicks(2652),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 585, DateTimeKind.Utc).AddTicks(1245));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 9, 11, 50, 14, 979, DateTimeKind.Utc).AddTicks(2950),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 585, DateTimeKind.Utc).AddTicks(1521));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_NovelComments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 9, 11, 50, 14, 977, DateTimeKind.Utc).AddTicks(1667),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 582, DateTimeKind.Utc).AddTicks(5917));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Chapters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 9, 11, 50, 14, 976, DateTimeKind.Utc).AddTicks(4257),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 10, 13, 51, 54, 579, DateTimeKind.Utc).AddTicks(4311));
        }
    }
}
