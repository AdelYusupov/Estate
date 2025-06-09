using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Estate.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Agent_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DealShare = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Agent_ID);
                });

            migrationBuilder.CreateTable(
                name: "ApartmentFilters",
                columns: table => new
                {
                    ApartmentFilter_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MinArea = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    MaxArea = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    MinRooms = table.Column<int>(type: "int", nullable: true),
                    MaxRooms = table.Column<int>(type: "int", nullable: true),
                    MinFloor = table.Column<int>(type: "int", nullable: true),
                    MaxFloor = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApartmentFilters", x => x.ApartmentFilter_ID);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Client_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Client_ID);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    District_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Coordinates = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.District_ID);
                });

            migrationBuilder.CreateTable(
                name: "HouseFilters",
                columns: table => new
                {
                    HouseFilter_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MinFloor = table.Column<int>(type: "int", nullable: true),
                    MaxFloor = table.Column<int>(type: "int", nullable: true),
                    MinArea = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    MaxArea = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    MinRooms = table.Column<int>(type: "int", nullable: true),
                    MaxRooms = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseFilters", x => x.HouseFilter_ID);
                });

            migrationBuilder.CreateTable(
                name: "LandFilters",
                columns: table => new
                {
                    LandFilter_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MinArea = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    MaxArea = table.Column<decimal>(type: "decimal(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandFilters", x => x.LandFilter_ID);
                });

            migrationBuilder.CreateTable(
                name: "RealEstateTypes",
                columns: table => new
                {
                    Type_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RealEstateTypes", x => x.Type_ID);
                });

            migrationBuilder.CreateTable(
                name: "Demands",
                columns: table => new
                {
                    Demand_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MinPrice = table.Column<int>(type: "int", nullable: true),
                    MaxPrice = table.Column<int>(type: "int", nullable: true),
                    Agent_ID = table.Column<int>(type: "int", nullable: false),
                    Client_ID = table.Column<int>(type: "int", nullable: false),
                    ApartmentFilter_ID = table.Column<int>(type: "int", nullable: true),
                    HouseFilter_ID = table.Column<int>(type: "int", nullable: true),
                    LandFilter_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Demands", x => x.Demand_ID);
                    table.ForeignKey(
                        name: "FK_Demands_Agents_Agent_ID",
                        column: x => x.Agent_ID,
                        principalTable: "Agents",
                        principalColumn: "Agent_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Demands_ApartmentFilters_ApartmentFilter_ID",
                        column: x => x.ApartmentFilter_ID,
                        principalTable: "ApartmentFilters",
                        principalColumn: "ApartmentFilter_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Demands_Clients_Client_ID",
                        column: x => x.Client_ID,
                        principalTable: "Clients",
                        principalColumn: "Client_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Demands_HouseFilters_HouseFilter_ID",
                        column: x => x.HouseFilter_ID,
                        principalTable: "HouseFilters",
                        principalColumn: "HouseFilter_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Demands_LandFilters_LandFilter_ID",
                        column: x => x.LandFilter_ID,
                        principalTable: "LandFilters",
                        principalColumn: "LandFilter_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RealEstates",
                columns: table => new
                {
                    RealEstate_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    District_ID = table.Column<int>(type: "int", nullable: false),
                    Type_ID = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ApartmentNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HouseNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LandNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalArea = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Rooms = table.Column<int>(type: "int", nullable: true),
                    Floor = table.Column<int>(type: "int", nullable: true),
                    TotalFloors = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RealEstates", x => x.RealEstate_ID);
                    table.ForeignKey(
                        name: "FK_RealEstates_Districts_District_ID",
                        column: x => x.District_ID,
                        principalTable: "Districts",
                        principalColumn: "District_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RealEstates_RealEstateTypes_Type_ID",
                        column: x => x.Type_ID,
                        principalTable: "RealEstateTypes",
                        principalColumn: "Type_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Supplies",
                columns: table => new
                {
                    Supply_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Price = table.Column<int>(type: "int", nullable: false),
                    RealEstate_ID = table.Column<int>(type: "int", nullable: false),
                    Agent_ID = table.Column<int>(type: "int", nullable: false),
                    Client_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplies", x => x.Supply_ID);
                    table.ForeignKey(
                        name: "FK_Supplies_Agents_Agent_ID",
                        column: x => x.Agent_ID,
                        principalTable: "Agents",
                        principalColumn: "Agent_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Supplies_Clients_Client_ID",
                        column: x => x.Client_ID,
                        principalTable: "Clients",
                        principalColumn: "Client_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Supplies_RealEstates_RealEstate_ID",
                        column: x => x.RealEstate_ID,
                        principalTable: "RealEstates",
                        principalColumn: "RealEstate_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Deals",
                columns: table => new
                {
                    Demand_ID = table.Column<int>(type: "int", nullable: false),
                    Supply_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deals", x => new { x.Demand_ID, x.Supply_ID });
                    table.ForeignKey(
                        name: "FK_Deals_Demands_Demand_ID",
                        column: x => x.Demand_ID,
                        principalTable: "Demands",
                        principalColumn: "Demand_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Deals_Supplies_Supply_ID",
                        column: x => x.Supply_ID,
                        principalTable: "Supplies",
                        principalColumn: "Supply_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Deals_Supply_ID",
                table: "Deals",
                column: "Supply_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Demands_Agent_ID",
                table: "Demands",
                column: "Agent_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Demands_ApartmentFilter_ID",
                table: "Demands",
                column: "ApartmentFilter_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Demands_Client_ID",
                table: "Demands",
                column: "Client_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Demands_HouseFilter_ID",
                table: "Demands",
                column: "HouseFilter_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Demands_LandFilter_ID",
                table: "Demands",
                column: "LandFilter_ID");

            migrationBuilder.CreateIndex(
                name: "IX_RealEstates_District_ID",
                table: "RealEstates",
                column: "District_ID");

            migrationBuilder.CreateIndex(
                name: "IX_RealEstates_Type_ID",
                table: "RealEstates",
                column: "Type_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Supplies_Agent_ID",
                table: "Supplies",
                column: "Agent_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Supplies_Client_ID",
                table: "Supplies",
                column: "Client_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Supplies_RealEstate_ID",
                table: "Supplies",
                column: "RealEstate_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Deals");

            migrationBuilder.DropTable(
                name: "Demands");

            migrationBuilder.DropTable(
                name: "Supplies");

            migrationBuilder.DropTable(
                name: "ApartmentFilters");

            migrationBuilder.DropTable(
                name: "HouseFilters");

            migrationBuilder.DropTable(
                name: "LandFilters");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "RealEstates");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "RealEstateTypes");
        }
    }
}
