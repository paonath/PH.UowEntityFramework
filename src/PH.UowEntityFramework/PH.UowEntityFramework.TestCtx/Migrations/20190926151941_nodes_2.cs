using Microsoft.EntityFrameworkCore.Migrations;

namespace PH.UowEntityFramework.TestCtx.Migrations
{
    public partial class nodes_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NodeDebug_transaction_audit_CreatedTransactionId",
                table: "NodeDebug");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeDebug_Data_DataId",
                table: "NodeDebug");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeDebug_transaction_audit_DeletedTransactionId",
                table: "NodeDebug");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeDebug_NodeDebug_ParentId",
                table: "NodeDebug");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeDebug_transaction_audit_UpdatedTransactionId",
                table: "NodeDebug");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NodeDebug",
                table: "NodeDebug");

            migrationBuilder.RenameTable(
                name: "NodeDebug",
                newName: "nodes_test");

            migrationBuilder.RenameIndex(
                name: "IX_NodeDebug_UpdatedTransactionId",
                table: "nodes_test",
                newName: "IX_nodes_test_UpdatedTransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_NodeDebug_ParentId",
                table: "nodes_test",
                newName: "IX_nodes_test_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_NodeDebug_DeletedTransactionId",
                table: "nodes_test",
                newName: "IX_nodes_test_DeletedTransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_NodeDebug_DataId",
                table: "nodes_test",
                newName: "IX_nodes_test_DataId");

            migrationBuilder.RenameIndex(
                name: "IX_NodeDebug_CreatedTransactionId",
                table: "nodes_test",
                newName: "IX_nodes_test_CreatedTransactionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_nodes_test",
                table: "nodes_test",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_nodes_test_Id_Deleted_CreatedTransactionId_UpdatedTransactionId_DeletedTransactionId",
                table: "nodes_test",
                columns: new[] { "Id", "Deleted", "CreatedTransactionId", "UpdatedTransactionId", "DeletedTransactionId" });

            migrationBuilder.AddForeignKey(
                name: "FK_nodes_test_transaction_audit_CreatedTransactionId",
                table: "nodes_test",
                column: "CreatedTransactionId",
                principalTable: "transaction_audit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_nodes_test_Data_DataId",
                table: "nodes_test",
                column: "DataId",
                principalTable: "Data",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_nodes_test_transaction_audit_DeletedTransactionId",
                table: "nodes_test",
                column: "DeletedTransactionId",
                principalTable: "transaction_audit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_nodes_test_nodes_test_ParentId",
                table: "nodes_test",
                column: "ParentId",
                principalTable: "nodes_test",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_nodes_test_transaction_audit_UpdatedTransactionId",
                table: "nodes_test",
                column: "UpdatedTransactionId",
                principalTable: "transaction_audit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_nodes_test_transaction_audit_CreatedTransactionId",
                table: "nodes_test");

            migrationBuilder.DropForeignKey(
                name: "FK_nodes_test_Data_DataId",
                table: "nodes_test");

            migrationBuilder.DropForeignKey(
                name: "FK_nodes_test_transaction_audit_DeletedTransactionId",
                table: "nodes_test");

            migrationBuilder.DropForeignKey(
                name: "FK_nodes_test_nodes_test_ParentId",
                table: "nodes_test");

            migrationBuilder.DropForeignKey(
                name: "FK_nodes_test_transaction_audit_UpdatedTransactionId",
                table: "nodes_test");

            migrationBuilder.DropPrimaryKey(
                name: "PK_nodes_test",
                table: "nodes_test");

            migrationBuilder.DropIndex(
                name: "IX_nodes_test_Id_Deleted_CreatedTransactionId_UpdatedTransactionId_DeletedTransactionId",
                table: "nodes_test");

            migrationBuilder.RenameTable(
                name: "nodes_test",
                newName: "NodeDebug");

            migrationBuilder.RenameIndex(
                name: "IX_nodes_test_UpdatedTransactionId",
                table: "NodeDebug",
                newName: "IX_NodeDebug_UpdatedTransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_nodes_test_ParentId",
                table: "NodeDebug",
                newName: "IX_NodeDebug_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_nodes_test_DeletedTransactionId",
                table: "NodeDebug",
                newName: "IX_NodeDebug_DeletedTransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_nodes_test_DataId",
                table: "NodeDebug",
                newName: "IX_NodeDebug_DataId");

            migrationBuilder.RenameIndex(
                name: "IX_nodes_test_CreatedTransactionId",
                table: "NodeDebug",
                newName: "IX_NodeDebug_CreatedTransactionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NodeDebug",
                table: "NodeDebug",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeDebug_transaction_audit_CreatedTransactionId",
                table: "NodeDebug",
                column: "CreatedTransactionId",
                principalTable: "transaction_audit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NodeDebug_Data_DataId",
                table: "NodeDebug",
                column: "DataId",
                principalTable: "Data",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NodeDebug_transaction_audit_DeletedTransactionId",
                table: "NodeDebug",
                column: "DeletedTransactionId",
                principalTable: "transaction_audit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NodeDebug_NodeDebug_ParentId",
                table: "NodeDebug",
                column: "ParentId",
                principalTable: "NodeDebug",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NodeDebug_transaction_audit_UpdatedTransactionId",
                table: "NodeDebug",
                column: "UpdatedTransactionId",
                principalTable: "transaction_audit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
