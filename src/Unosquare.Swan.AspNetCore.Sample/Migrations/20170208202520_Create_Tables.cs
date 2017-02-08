using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Unosquare.Swan.AspNetCore.Sample.Migrations
{
    public partial class Create_Tables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LogEntries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Browser = table.Column<string>(maxLength: 200, nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    Exception = table.Column<string>(maxLength: 2000, nullable: true),
                    HostAddress = table.Column<string>(maxLength: 20, nullable: true),
                    Level = table.Column<string>(maxLength: 50, nullable: false),
                    Logger = table.Column<string>(maxLength: 255, nullable: false),
                    Message = table.Column<string>(maxLength: 4000, nullable: false),
                    Thread = table.Column<string>(maxLength: 255, nullable: false),
                    Url = table.Column<string>(maxLength: 100, nullable: true),
                    Username = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditTrailEntries",
                columns: table => new
                {
                    AuditId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Action = table.Column<int>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    JsonBody = table.Column<string>(nullable: true),
                    TableName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTrailEntries", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogEntries");

            migrationBuilder.DropTable(
                name: "AuditTrailEntries");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
