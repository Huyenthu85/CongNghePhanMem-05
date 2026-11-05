using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ThuyetMinh.Data.MigrationsSqlServer
{
    /// <inheritdoc />
    public partial class InitialSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnonymousVisitors",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TokenHash = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnonymousVisitors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ListeningBalances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnonymousVisitorId = table.Column<long>(type: "bigint", nullable: false),
                    RemainingSeconds = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListeningBalances", x => x.Id);
                    table.CheckConstraint("CK_ListeningBalance_RemainingSeconds", "RemainingSeconds >= 0");
                    table.ForeignKey(
                        name: "FK_ListeningBalances_AnonymousVisitors_AnonymousVisitorId",
                        column: x => x.AnonymousVisitorId,
                        principalTable: "AnonymousVisitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentOrders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AnonymousVisitorId = table.Column<long>(type: "bigint", nullable: false),
                    AmountVnd = table.Column<long>(type: "bigint", nullable: false),
                    GrantedSeconds = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GrantedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProviderTransactionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentOrders_AnonymousVisitors_AnonymousVisitorId",
                        column: x => x.AnonymousVisitorId,
                        principalTable: "AnonymousVisitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Shops",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    ActivationRadiusMeters = table.Column<double>(type: "float", nullable: false),
                    PublishStatus = table.Column<int>(type: "int", nullable: false),
                    PublishedVersionId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shops_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopSubmissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShopId = table.Column<long>(type: "bigint", nullable: false),
                    SubmittedById = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    ActivationRadiusMeters = table.Column<double>(type: "float", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrlsRaw = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedById = table.Column<long>(type: "bigint", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUpdateRequest = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopSubmissions_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShopSubmissions_Users_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopSubmissions_Users_SubmittedById",
                        column: x => x.SubmittedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcessingJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShopSubmissionId = table.Column<long>(type: "bigint", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessingJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessingJobs_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessingJobs_ShopSubmissions_ShopSubmissionId",
                        column: x => x.ShopSubmissionId,
                        principalTable: "ShopSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Translations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShopSubmissionId = table.Column<long>(type: "bigint", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    TranslatedText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Translations_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Translations_ShopSubmissions_ShopSubmissionId",
                        column: x => x.ShopSubmissionId,
                        principalTable: "ShopSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Audios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TranslationId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Audios_Translations_TranslationId",
                        column: x => x.TranslationId,
                        principalTable: "Translations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ListeningSessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnonymousVisitorId = table.Column<long>(type: "bigint", nullable: false),
                    ShopId = table.Column<long>(type: "bigint", nullable: false),
                    AudioId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastHeartbeatAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChargedSeconds = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListeningSessions", x => x.Id);
                    table.CheckConstraint("CK_ListeningSession_ChargedSeconds", "ChargedSeconds >= 0");
                    table.ForeignKey(
                        name: "FK_ListeningSessions_AnonymousVisitors_AnonymousVisitorId",
                        column: x => x.AnonymousVisitorId,
                        principalTable: "AnonymousVisitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ListeningSessions_Audios_AudioId",
                        column: x => x.AudioId,
                        principalTable: "Audios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ListeningSessions_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ListeningTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnonymousVisitorId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangeSeconds = table.Column<long>(type: "bigint", nullable: false),
                    BalanceAfterSeconds = table.Column<long>(type: "bigint", nullable: false),
                    PaymentOrderId = table.Column<long>(type: "bigint", nullable: true),
                    ListeningSessionId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListeningTransactions", x => x.Id);
                    table.CheckConstraint("CK_ListeningTransaction_BalanceAfter", "BalanceAfterSeconds >= 0");
                    table.CheckConstraint("CK_ListeningTransaction_Change", "([Type] = 'PaymentCredit' AND [ChangeSeconds] > 0 AND [PaymentOrderId] IS NOT NULL AND [ListeningSessionId] IS NULL) OR ([Type] = 'ListeningDebit' AND [ChangeSeconds] < 0 AND [ListeningSessionId] IS NOT NULL AND [PaymentOrderId] IS NULL)");
                    table.ForeignKey(
                        name: "FK_ListeningTransactions_AnonymousVisitors_AnonymousVisitorId",
                        column: x => x.AnonymousVisitorId,
                        principalTable: "AnonymousVisitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ListeningTransactions_ListeningSessions_ListeningSessionId",
                        column: x => x.ListeningSessionId,
                        principalTable: "ListeningSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ListeningTransactions_PaymentOrders_PaymentOrderId",
                        column: x => x.PaymentOrderId,
                        principalTable: "PaymentOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Languages",
                columns: new[] { "Id", "Code", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "vi", true, "Tiếng Việt" },
                    { 2, "en", true, "Tiếng Anh" },
                    { 3, "zh", true, "Tiếng Trung" },
                    { 4, "km", true, "Tiếng Khmer" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousVisitors_TokenHash",
                table: "AnonymousVisitors",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Audios_TranslationId",
                table: "Audios",
                column: "TranslationId");

            migrationBuilder.CreateIndex(
                name: "IX_Languages_Code",
                table: "Languages",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListeningBalances_AnonymousVisitorId",
                table: "ListeningBalances",
                column: "AnonymousVisitorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListeningSessions_AnonymousVisitorId",
                table: "ListeningSessions",
                column: "AnonymousVisitorId",
                unique: true,
                filter: "[Status] <> 'Ended'");

            migrationBuilder.CreateIndex(
                name: "IX_ListeningSessions_AudioId",
                table: "ListeningSessions",
                column: "AudioId");

            migrationBuilder.CreateIndex(
                name: "IX_ListeningSessions_ShopId",
                table: "ListeningSessions",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ListeningTransactions_AnonymousVisitorId",
                table: "ListeningTransactions",
                column: "AnonymousVisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_ListeningTransactions_ListeningSessionId",
                table: "ListeningTransactions",
                column: "ListeningSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ListeningTransactions_PaymentOrderId",
                table: "ListeningTransactions",
                column: "PaymentOrderId",
                unique: true,
                filter: "[PaymentOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_AnonymousVisitorId",
                table: "PaymentOrders",
                column: "AnonymousVisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_OrderCode",
                table: "PaymentOrders",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_ProviderTransactionId",
                table: "PaymentOrders",
                column: "ProviderTransactionId",
                unique: true,
                filter: "[ProviderTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingJobs_LanguageId",
                table: "ProcessingJobs",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingJobs_ShopSubmissionId",
                table: "ProcessingJobs",
                column: "ShopSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Shops_OwnerId",
                table: "Shops",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSubmissions_ReviewedById",
                table: "ShopSubmissions",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSubmissions_ShopId",
                table: "ShopSubmissions",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSubmissions_SubmittedById",
                table: "ShopSubmissions",
                column: "SubmittedById");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_LanguageId",
                table: "Translations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_ShopSubmissionId_LanguageId",
                table: "Translations",
                columns: new[] { "ShopSubmissionId", "LanguageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListeningBalances");

            migrationBuilder.DropTable(
                name: "ListeningTransactions");

            migrationBuilder.DropTable(
                name: "ProcessingJobs");

            migrationBuilder.DropTable(
                name: "ListeningSessions");

            migrationBuilder.DropTable(
                name: "PaymentOrders");

            migrationBuilder.DropTable(
                name: "Audios");

            migrationBuilder.DropTable(
                name: "AnonymousVisitors");

            migrationBuilder.DropTable(
                name: "Translations");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "ShopSubmissions");

            migrationBuilder.DropTable(
                name: "Shops");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
