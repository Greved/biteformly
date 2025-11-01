using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiteForm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "forms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "form_fields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_fields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_form_fields_forms_FormId",
                        column: x => x.FormId,
                        principalTable: "forms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "form_submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmittedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_form_submissions_forms_FormId",
                        column: x => x.FormId,
                        principalTable: "forms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "form_responses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_responses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_form_responses_form_fields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "form_fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_form_responses_form_submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "form_submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_form_fields_FormId_Key",
                table: "form_fields",
                columns: new[] { "FormId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_form_responses_FieldId",
                table: "form_responses",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_form_responses_SubmissionId",
                table: "form_responses",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_form_submissions_FormId",
                table: "form_submissions",
                column: "FormId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "form_responses");

            migrationBuilder.DropTable(
                name: "form_fields");

            migrationBuilder.DropTable(
                name: "form_submissions");

            migrationBuilder.DropTable(
                name: "forms");
        }
    }
}
