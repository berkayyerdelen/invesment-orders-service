using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestmentOrders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAccountConcurrencyToken : Migration
    {
        // `xmin` is Postgres' built-in system column, not a column EF ever actually added (the scaffolded
        // CreateTable step never emitted real DDL for it either). `ALTER TABLE ... DROP COLUMN "xmin"` is not
        // valid Postgres SQL — system columns can't be dropped — so this migration is a deliberate no-op; it
        // exists only so the EF model snapshot matches the removal of the `IsRowVersion()` mapping in code.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
