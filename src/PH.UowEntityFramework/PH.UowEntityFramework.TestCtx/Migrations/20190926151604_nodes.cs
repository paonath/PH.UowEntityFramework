using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PH.UowEntityFramework.TestCtx.Migrations
{
    public partial class nodes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NodeDebug",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    DeletedTransactionId = table.Column<long>(nullable: true),
                    CreatedTransactionId = table.Column<long>(nullable: false),
                    UpdatedTransactionId = table.Column<long>(nullable: false),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: true),
                    NodeName = table.Column<string>(nullable: true),
                    ParentId = table.Column<string>(nullable: true),
                    DataId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeDebug", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NodeDebug_transaction_audit_CreatedTransactionId",
                        column: x => x.CreatedTransactionId,
                        principalTable: "transaction_audit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NodeDebug_Data_DataId",
                        column: x => x.DataId,
                        principalTable: "Data",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NodeDebug_transaction_audit_DeletedTransactionId",
                        column: x => x.DeletedTransactionId,
                        principalTable: "transaction_audit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NodeDebug_NodeDebug_ParentId",
                        column: x => x.ParentId,
                        principalTable: "NodeDebug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NodeDebug_transaction_audit_UpdatedTransactionId",
                        column: x => x.UpdatedTransactionId,
                        principalTable: "transaction_audit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NodeDebug_CreatedTransactionId",
                table: "NodeDebug",
                column: "CreatedTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeDebug_DataId",
                table: "NodeDebug",
                column: "DataId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeDebug_DeletedTransactionId",
                table: "NodeDebug",
                column: "DeletedTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeDebug_ParentId",
                table: "NodeDebug",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeDebug_UpdatedTransactionId",
                table: "NodeDebug",
                column: "UpdatedTransactionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NodeDebug");
        }
    }
}
