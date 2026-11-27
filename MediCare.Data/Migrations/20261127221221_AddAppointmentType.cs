using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Appointments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "FirstVisit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Appointments");
        }
    }
}
