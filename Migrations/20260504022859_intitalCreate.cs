using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovelProject.Migrations
{
    public partial class intitalCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_Categories",
                columns: table => new
                {
                    CategoryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsEnable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "T_Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NickName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RegisterTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastLoginTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_T_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "T_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_T_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "T_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_T_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "T_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_T_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "T_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_Novels",
                columns: table => new
                {
                    NovelId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    AuthorId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AuthorName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CoverUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    WordCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ChapterCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PublishTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastUpdateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ClickCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FavoriteCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Novels", x => x.NovelId);
                    table.ForeignKey(
                        name: "FK_T_Novels_T_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "T_Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_T_Novels_T_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "T_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "T_Chapters",
                columns: table => new
                {
                    ChapterId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NovelId = table.Column<long>(type: "bigint", nullable: false),
                    ChapterNumber = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: false),
                    WordCount = table.Column<int>(type: "int", nullable: false),
                    PublishTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Chapters", x => x.ChapterId);
                    table.ForeignKey(
                        name: "FK_T_Chapters_T_Novels_NovelId",
                        column: x => x.NovelId,
                        principalTable: "T_Novels",
                        principalColumn: "NovelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_UserFavorites",
                columns: table => new
                {
                    FavoriteId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    NovelId = table.Column<long>(type: "bigint", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_UserFavorites", x => x.FavoriteId);
                    table.ForeignKey(
                        name: "FK_T_UserFavorites_T_Novels_NovelId",
                        column: x => x.NovelId,
                        principalTable: "T_Novels",
                        principalColumn: "NovelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_T_UserFavorites_T_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "T_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_NovelComments",
                columns: table => new
                {
                    CommentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    NovelId = table.Column<long>(type: "bigint", nullable: false),
                    ChapterId = table.Column<long>(type: "bigint", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LikeCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_NovelComments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_T_NovelComments_T_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "T_Chapters",
                        principalColumn: "ChapterId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_T_NovelComments_T_Novels_NovelId",
                        column: x => x.NovelId,
                        principalTable: "T_Novels",
                        principalColumn: "NovelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_T_NovelComments_T_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "T_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "T_UserReadingRecords",
                columns: table => new
                {
                    RecordId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    NovelId = table.Column<long>(type: "bigint", nullable: false),
                    LastReadChapterId = table.Column<long>(type: "bigint", nullable: true),
                    LastReadTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_UserReadingRecords", x => x.RecordId);
                    table.ForeignKey(
                        name: "FK_T_UserReadingRecords_T_Chapters_LastReadChapterId",
                        column: x => x.LastReadChapterId,
                        principalTable: "T_Chapters",
                        principalColumn: "ChapterId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_T_UserReadingRecords_T_Novels_NovelId",
                        column: x => x.NovelId,
                        principalTable: "T_Novels",
                        principalColumn: "NovelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_T_UserReadingRecords_T_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "T_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Categories_CategoryName",
                table: "T_Categories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_Categories_IsEnable",
                table: "T_Categories",
                column: "IsEnable");

            migrationBuilder.CreateIndex(
                name: "IX_T_Chapters_NovelId",
                table: "T_Chapters",
                column: "NovelId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Chapters_NovelId_ChapterNumber",
                table: "T_Chapters",
                columns: new[] { "NovelId", "ChapterNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_NovelComments_ChapterId",
                table: "T_NovelComments",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_T_NovelComments_CreateTime",
                table: "T_NovelComments",
                column: "CreateTime");

            migrationBuilder.CreateIndex(
                name: "IX_T_NovelComments_NovelId",
                table: "T_NovelComments",
                column: "NovelId");

            migrationBuilder.CreateIndex(
                name: "IX_T_NovelComments_UserId",
                table: "T_NovelComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Novels_AuthorId",
                table: "T_Novels",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Novels_CategoryId",
                table: "T_Novels",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Novels_LastUpdateTime",
                table: "T_Novels",
                column: "LastUpdateTime");

            migrationBuilder.CreateIndex(
                name: "IX_T_Novels_Status",
                table: "T_Novels",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_T_Novels_Title",
                table: "T_Novels",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_T_UserFavorites_NovelId",
                table: "T_UserFavorites",
                column: "NovelId");

            migrationBuilder.CreateIndex(
                name: "IX_T_UserFavorites_UserId",
                table: "T_UserFavorites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_T_UserFavorites_UserId_NovelId",
                table: "T_UserFavorites",
                columns: new[] { "UserId", "NovelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_UserReadingRecords_LastReadChapterId",
                table: "T_UserReadingRecords",
                column: "LastReadChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_T_UserReadingRecords_NovelId",
                table: "T_UserReadingRecords",
                column: "NovelId");

            migrationBuilder.CreateIndex(
                name: "IX_T_UserReadingRecords_UserId",
                table: "T_UserReadingRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_T_UserReadingRecords_UserId_NovelId",
                table: "T_UserReadingRecords",
                columns: new[] { "UserId", "NovelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "T_Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_T_Users_IsDeleted",
                table: "T_Users",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_T_Users_NickName",
                table: "T_Users",
                column: "NickName");

            migrationBuilder.CreateIndex(
                name: "IX_T_Users_RegisterTime",
                table: "T_Users",
                column: "RegisterTime");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "T_Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "T_NovelComments");

            migrationBuilder.DropTable(
                name: "T_UserFavorites");

            migrationBuilder.DropTable(
                name: "T_UserReadingRecords");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "T_Chapters");

            migrationBuilder.DropTable(
                name: "T_Novels");

            migrationBuilder.DropTable(
                name: "T_Categories");

            migrationBuilder.DropTable(
                name: "T_Users");
        }
    }
}
