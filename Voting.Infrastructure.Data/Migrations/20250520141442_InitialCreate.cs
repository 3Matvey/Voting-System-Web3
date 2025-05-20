using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Voting.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BlockchainAddress = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    VerificationLevel = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VotingSessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Mode = table.Column<string>(type: "text", nullable: false),
                    RequiredVerificationLevel = table.Column<int>(type: "integer", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VotingActive = table.Column<bool>(type: "boolean", nullable: false),
                    StartTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RegisteredUserIds = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    VotedUserIds = table.Column<Guid[]>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VotingSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VotingSessionCandidates",
                columns: table => new
                {
                    CandidateId = table.Column<long>(type: "bigint", nullable: false),
                    SessionId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                    VoteCount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VotingSessionCandidates", x => new { x.SessionId, x.CandidateId });
                    table.ForeignKey(
                        name: "FK_VotingSessionCandidates_VotingSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "VotingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "VotingSessionCandidates");

            migrationBuilder.DropTable(
                name: "VotingSessions");
        }
    }
}
