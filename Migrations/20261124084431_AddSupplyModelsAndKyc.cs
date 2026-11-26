using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Rent_Lehenga_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplyModelsAndKyc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "aadhar_doc_url",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "aadhar_number",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "gst_number",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_verified",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "kyc_status",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "NOT_SUBMITTED");

            migrationBuilder.AddColumn<string>(
                name: "selfie_video_url",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "shop_name",
                table: "users",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "admin_notes",
                table: "listings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "bust_size",
                table: "listings",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "listings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "BRIDAL");

            migrationBuilder.AddColumn<string>(
                name: "fabric",
                table: "listings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "original_price",
                table: "listings",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<string>(
                name: "rejection_reason",
                table: "listings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "security_deposit",
                table: "listings",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<string>(
                name: "size",
                table: "listings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "skirt_length",
                table: "listings",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "supply_type",
                table: "listings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "ADMIN_OWN");

            migrationBuilder.AddColumn<DateTime>(
                name: "verified_at",
                table: "listings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "verified_by_user_id",
                table: "listings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "waist_size",
                table: "listings",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "listing_photos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    listing_id = table.Column<long>(type: "bigint", nullable: false),
                    photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_cover = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    photo_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "FRONT"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_listing_photos", x => x.id);
                    table.ForeignKey(
                        name: "FK_listing_photos_listings_listing_id",
                        column: x => x.listing_id,
                        principalTable: "listings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_listing_photos_listing_id",
                table: "listing_photos",
                column: "listing_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "listing_photos");

            migrationBuilder.DropColumn(
                name: "aadhar_doc_url",
                table: "users");

            migrationBuilder.DropColumn(
                name: "aadhar_number",
                table: "users");

            migrationBuilder.DropColumn(
                name: "address",
                table: "users");

            migrationBuilder.DropColumn(
                name: "city",
                table: "users");

            migrationBuilder.DropColumn(
                name: "gst_number",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_verified",
                table: "users");

            migrationBuilder.DropColumn(
                name: "kyc_status",
                table: "users");

            migrationBuilder.DropColumn(
                name: "selfie_video_url",
                table: "users");

            migrationBuilder.DropColumn(
                name: "shop_name",
                table: "users");

            migrationBuilder.DropColumn(
                name: "admin_notes",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "bust_size",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "category",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "fabric",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "original_price",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "rejection_reason",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "security_deposit",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "size",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "skirt_length",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "supply_type",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "verified_at",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "verified_by_user_id",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "waist_size",
                table: "listings");
        }
    }
}
