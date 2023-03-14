using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Db.Migrations
{
    /// <inheritdoc />
    public partial class PiecesCapturingForwardAndBackward : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PiecesMovingForwardAndBackward",
                table: "CheckersOptions",
                newName: "PiecesCapturingForwardAndBackward");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PiecesCapturingForwardAndBackward",
                table: "CheckersOptions",
                newName: "PiecesMovingForwardAndBackward");
        }
    }
}
