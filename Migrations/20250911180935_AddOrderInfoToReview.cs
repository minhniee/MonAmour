using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonAmour.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderInfoToReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__PaymentDe__booki__2A164134",
                table: "PaymentDetail");

            migrationBuilder.DropTable(
                name: "Booking");

            migrationBuilder.DropIndex(
                name: "IX_PaymentDetail_booking_id",
                table: "PaymentDetail");

            migrationBuilder.DropColumn(
                name: "booking_id",
                table: "PaymentDetail");

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "Review",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderItemId",
                table: "Review",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Review");

            migrationBuilder.DropColumn(
                name: "OrderItemId",
                table: "Review");

            migrationBuilder.AddColumn<int>(
                name: "booking_id",
                table: "PaymentDetail",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Booking",
                columns: table => new
                {
                    booking_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    concept_id = table.Column<int>(type: "int", nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    booking_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    booking_time = table.Column<TimeSpan>(type: "time", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    confirmed_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    payment_status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "pending"),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    total_price = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Booking__5DE3A5B18F1D4649", x => x.booking_id);
                    table.ForeignKey(
                        name: "FK__Booking__concept__7A672E12",
                        column: x => x.concept_id,
                        principalTable: "Concept",
                        principalColumn: "concept_id");
                    table.ForeignKey(
                        name: "FK__Booking__user_id__797309D9",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetail_booking_id",
                table: "PaymentDetail",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_concept_id",
                table: "Booking",
                column: "concept_id");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_user_id",
                table: "Booking",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK__PaymentDe__booki__2A164134",
                table: "PaymentDetail",
                column: "booking_id",
                principalTable: "Booking",
                principalColumn: "booking_id");
        }
    }
}
