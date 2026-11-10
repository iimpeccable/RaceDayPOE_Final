using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RacingDays.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateResultSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Results_Events_EventId",
                table: "Results");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_Users_UserId",
                table: "Results");

            migrationBuilder.DropIndex(
                name: "IX_Results_EventId",
                table: "Results");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Results");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Results",
                newName: "RegistrationId");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "Results",
                newName: "FinishTime");

            migrationBuilder.RenameIndex(
                name: "IX_Results_UserId",
                table: "Results",
                newName: "IX_Results_RegistrationId");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 2,
                column: "Name",
                value: "Elite Marathon");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 3,
                columns: new[] { "Name", "Type" },
                values: new object[] { "Junior Cycling", "Age/Distance" });

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Registrations_RegistrationId",
                table: "Results",
                column: "RegistrationId",
                principalTable: "Registrations",
                principalColumn: "RegistrationId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Results_Registrations_RegistrationId",
                table: "Results");

            migrationBuilder.RenameColumn(
                name: "RegistrationId",
                table: "Results",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "FinishTime",
                table: "Results",
                newName: "Time");

            migrationBuilder.RenameIndex(
                name: "IX_Results_RegistrationId",
                table: "Results",
                newName: "IX_Results_UserId");

            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "Results",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 2,
                column: "Name",
                value: "Half Marathon 21km");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 3,
                columns: new[] { "Name", "Type" },
                values: new object[] { "Senior (18-39)", "Age" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Age", "Contact", "Email", "Name", "PasswordHash", "RoleId" },
                values: new object[,]
                {
                    { 1, 30, "0123456789", "dummyadmin@racingdays.co.za", "dummyadmin", "$2a$11$RkFGzzXEyfyXQObbdjgMY.zQzsFwCqnprJX01K.5wVOY9do/1U.tG", 1 },
                    { 2, 25, "0987654321", "dummyparticipant@racingdays.co.za", "dummyparticipant", "$2a$11$RkFGzzXEyfyXQObbdjgMY.zQzsFwCqnprJX01K.5wVOY9do/1U.tG", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Results_EventId",
                table: "Results",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Events_EventId",
                table: "Results",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Users_UserId",
                table: "Results",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
