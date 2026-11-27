using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovelProject.Migrations
{
    /// <inheritdoc />
    public partial class AddChatRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "RegisterTime",
                table: "T_Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 11, 27, 5, 1, 7, 994, DateTimeKind.Utc).AddTicks(1571),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 455, DateTimeKind.Utc).AddTicks(7038));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastReadTime",
                table: "T_UserReadingRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 11, 27, 5, 1, 7, 996, DateTimeKind.Utc).AddTicks(4407),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 457, DateTimeKind.Utc).AddTicks(1828));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_UserFavorites",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 11, 27, 5, 1, 7, 995, DateTimeKind.Utc).AddTicks(1433),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 456, DateTimeKind.Utc).AddTicks(2233));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 11, 27, 5, 1, 7, 992, DateTimeKind.Utc).AddTicks(9815),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 454, DateTimeKind.Utc).AddTicks(8310));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 11, 27, 5, 1, 7, 993, DateTimeKind.Utc).AddTicks(635),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 454, DateTimeKind.Utc).AddTicks(8710));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_NovelComments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 11, 27, 5, 1, 7, 988, DateTimeKind.Utc).AddTicks(6750),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 451, DateTimeKind.Utc).AddTicks(4947));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_CommentLikes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 11, 27, 5, 1, 7, 984, DateTimeKind.Utc).AddTicks(3192),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 449, DateTimeKind.Utc).AddTicks(1021));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Chapters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 11, 27, 5, 1, 7, 982, DateTimeKind.Utc).AddTicks(7993),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 448, DateTimeKind.Utc).AddTicks(1397));

            migrationBuilder.CreateTable(
                name: "T_Messages",
                columns: table => new
                {
                    MessageId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToUserId = table.Column<long>(type: "bigint", nullable: false),
                    FromUserId = table.Column<long>(type: "bigint", nullable: false),
                    FromUserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ConversationId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Messages", x => x.MessageId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_Messages_ConversationId_MessageId",
                table: "T_Messages",
                columns: new[] { "ConversationId", "MessageId" });

            migrationBuilder.CreateIndex(
                name: "IX_T_Messages_ToUserId_IsRead",
                table: "T_Messages",
                columns: new[] { "ToUserId", "IsRead" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_Messages");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RegisterTime",
                table: "T_Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 455, DateTimeKind.Utc).AddTicks(7038),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 11, 27, 5, 1, 7, 994, DateTimeKind.Utc).AddTicks(1571));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastReadTime",
                table: "T_UserReadingRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 457, DateTimeKind.Utc).AddTicks(1828),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 11, 27, 5, 1, 7, 996, DateTimeKind.Utc).AddTicks(4407));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_UserFavorites",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 456, DateTimeKind.Utc).AddTicks(2233),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 11, 27, 5, 1, 7, 995, DateTimeKind.Utc).AddTicks(1433));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 454, DateTimeKind.Utc).AddTicks(8310),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 11, 27, 5, 1, 7, 992, DateTimeKind.Utc).AddTicks(9815));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "T_Novels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 454, DateTimeKind.Utc).AddTicks(8710),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 11, 27, 5, 1, 7, 993, DateTimeKind.Utc).AddTicks(635));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_NovelComments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 451, DateTimeKind.Utc).AddTicks(4947),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 11, 27, 5, 1, 7, 988, DateTimeKind.Utc).AddTicks(6750));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "T_CommentLikes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 449, DateTimeKind.Utc).AddTicks(1021),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 11, 27, 5, 1, 7, 984, DateTimeKind.Utc).AddTicks(3192));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishTime",
                table: "T_Chapters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 23, 11, 42, 448, DateTimeKind.Utc).AddTicks(1397),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 11, 27, 5, 1, 7, 982, DateTimeKind.Utc).AddTicks(7993));
        }
    }
}
