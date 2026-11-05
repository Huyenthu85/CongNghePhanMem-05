using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThuyetMinh.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddListeningAndPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnonymousVisitors",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TokenHash = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnonymousVisitors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ListeningBalances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnonymousVisitorId = table.Column<long>(type: "INTEGER", nullable: false),
                    RemainingSeconds = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                name: "ListeningSessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnonymousVisitorId = table.Column<long>(type: "INTEGER", nullable: false),
                    ShopId = table.Column<long>(type: "INTEGER", nullable: false),
                    AudioId = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastHeartbeatAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ChargedSeconds = table.Column<long>(type: "INTEGER", nullable: false)
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
                name: "PaymentOrders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderCode = table.Column<string>(type: "TEXT", nullable: false),
                    AnonymousVisitorId = table.Column<long>(type: "INTEGER", nullable: false),
                    AmountVnd = table.Column<long>(type: "INTEGER", nullable: false),
                    GrantedSeconds = table.Column<long>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GrantedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ProviderTransactionId = table.Column<string>(type: "TEXT", nullable: true)
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
                name: "ListeningTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnonymousVisitorId = table.Column<long>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    ChangeSeconds = table.Column<long>(type: "INTEGER", nullable: false),
                    BalanceAfterSeconds = table.Column<long>(type: "INTEGER", nullable: false),
                    PaymentOrderId = table.Column<long>(type: "INTEGER", nullable: true),
                    ListeningSessionId = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListeningTransactions", x => x.Id);
                    table.CheckConstraint("CK_ListeningTransaction_BalanceAfter", "BalanceAfterSeconds >= 0");
                    table.CheckConstraint("CK_ListeningTransaction_Change", "(\"Type\" = 'PaymentCredit' AND \"ChangeSeconds\" > 0 AND \"PaymentOrderId\" IS NOT NULL AND \"ListeningSessionId\" IS NULL) OR (\"Type\" = 'ListeningDebit' AND \"ChangeSeconds\" < 0 AND \"ListeningSessionId\" IS NOT NULL AND \"PaymentOrderId\" IS NULL)");
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

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousVisitors_TokenHash",
                table: "AnonymousVisitors",
                column: "TokenHash",
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
                filter: "\"Status\" <> 'Ended'");

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
                filter: "\"PaymentOrderId\" IS NOT NULL");

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
                name: "ListeningSessions");

            migrationBuilder.DropTable(
                name: "PaymentOrders");

            migrationBuilder.DropTable(
                name: "AnonymousVisitors");
        }
    }
}
