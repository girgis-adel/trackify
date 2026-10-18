using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LinkTracksToArtists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrackDistributions_TrackId",
                table: "TrackDistributions");

            migrationBuilder.AddColumn<string>(
                name: "ArtistId",
                table: "Tracks",
                type: "nvarchar(450)",
                nullable: true);

            // These ISRC/title pairs are the original demo fixtures. Their associations are
            // new demo-data attribution, not a claim about historical ownership.
            migrationBuilder.Sql("""
                UPDATE t SET ArtistId = mappedArtist.Id
                FROM Tracks AS t
                JOIN (VALUES
                    (N'USTFY2600001', N'Copper Skies', N'MARA.VOSS@EXAMPLE.TEST'),
                    (N'USTFY2600002', N'Meridian Drive', N'NIA.OKAFOR@EXAMPLE.TEST'),
                    (N'USTFY2400003', N'Tidal Patterns', N'LUCIA.REYES@EXAMPLE.TEST'),
                    (N'USTFY2500004', N'Green Line', N'MARA.VOSS@EXAMPLE.TEST'),
                    (N'USTFY2600005', N'After the Rain', N'NIA.OKAFOR@EXAMPLE.TEST'),
                    (N'USTFY2500006', N'Lanterns at Dawn', N'LUCIA.REYES@EXAMPLE.TEST'),
                    (N'USTFY2600007', N'Northbound Lanes', N'MARA.VOSS@EXAMPLE.TEST'),
                    (N'USTFY2600008', N'Solstice Echoes', N'NIA.OKAFOR@EXAMPLE.TEST')
                ) AS fixture(Isrc, Title, NormalizedEmail)
                    ON fixture.Isrc = t.Isrc AND fixture.Title = t.Title
                CROSS APPLY (
                    SELECT TOP (1) a.Id
                    FROM Artists AS a
                    JOIN AspNetUsers AS u ON u.Id = a.UserId
                    WHERE u.NormalizedEmail = fixture.NormalizedEmail
                    ORDER BY a.Id
                ) AS mappedArtist
                WHERE t.ArtistId IS NULL;

                IF EXISTS (SELECT 1 FROM Tracks WHERE ArtistId IS NULL)
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Id = N'trackify-legacy-unattributed-user')
                        INSERT INTO AspNetUsers
                            (Id, CreateDate, UpdateDate, IsRemovable, IsActive,
                             EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled,
                             LockoutEnabled, AccessFailedCount)
                        VALUES
                            (N'trackify-legacy-unattributed-user', SYSUTCDATETIME(), SYSUTCDATETIME(),
                             1, 0, 0, 0, 0, 0, 0);

                    IF NOT EXISTS (SELECT 1 FROM Artists WHERE Id = N'trackify-legacy-unattributed-artist')
                        INSERT INTO Artists
                            (Id, UserId, Name, EmailSnapshot, IsActive,
                             CreateDate, UpdateDate, IsRemovable)
                        VALUES
                            (N'trackify-legacy-unattributed-artist',
                             N'trackify-legacy-unattributed-user',
                             N'Legacy artist (unattributed)', N'unattributed@trackify.invalid',
                             1, SYSUTCDATETIME(), SYSUTCDATETIME(), 1);

                    UPDATE Tracks
                    SET ArtistId = N'trackify-legacy-unattributed-artist'
                    WHERE ArtistId IS NULL;
                END
                """);

            migrationBuilder.AlterColumn<string>(
                name: "ArtistId",
                table: "Tracks",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM TrackDistributions
                    GROUP BY TrackId, DSPId HAVING COUNT(*) > 1
                )
                    THROW 51000, 'Duplicate track/DSP distributions must be resolved before this migration can add a unique constraint.', 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_ArtistId",
                table: "Tracks",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackDistributions_TrackId_DSPId",
                table: "TrackDistributions",
                columns: new[] { "TrackId", "DSPId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tracks_Artists_ArtistId",
                table: "Tracks",
                column: "ArtistId",
                principalTable: "Artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_Artists_ArtistId",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_ArtistId",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_TrackDistributions_TrackId_DSPId",
                table: "TrackDistributions");

            migrationBuilder.DropColumn(
                name: "ArtistId",
                table: "Tracks");

            migrationBuilder.CreateIndex(
                name: "IX_TrackDistributions_TrackId",
                table: "TrackDistributions",
                column: "TrackId");
        }
    }
}
