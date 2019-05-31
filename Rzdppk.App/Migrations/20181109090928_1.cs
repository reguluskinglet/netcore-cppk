using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Rzdppk.App.Migrations
{
    public partial class _1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActCategories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "auth_roles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Name = table.Column<string>(nullable: true),
                    Permissions = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auth_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Brigades",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    BrigadeType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brigades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceFaults",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceFaults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Name = table.Column<string>(nullable: true),
                    Serial = table.Column<string>(nullable: true),
                    CellNumber = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Directions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Directions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentCategoryes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentCategoryes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Faults",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    FaultType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Models",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    ModelType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Models", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Controller = table.Column<string>(nullable: true),
                    Action = table.Column<string>(nullable: true),
                    PermissionBits = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stantions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    ShortName = table.Column<string>(nullable: true),
                    StantionType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stantions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TemplateLabels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Name = table.Column<string>(nullable: true),
                    Template = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateLabels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    TripType = table.Column<int>(nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TvBoxes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TvBoxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "auth_users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Login = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    PasswordHash = table.Column<string>(nullable: true),
                    IsBlocked = table.Column<bool>(nullable: false),
                    RoleId = table.Column<int>(nullable: false),
                    BrigadeId = table.Column<int>(nullable: true),
                    PersonPosition = table.Column<string>(nullable: true),
                    PersonNumber = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auth_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_auth_users_Brigades_BrigadeId",
                        column: x => x.BrigadeId,
                        principalTable: "Brigades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_auth_users_auth_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "auth_roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Turnovers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Name = table.Column<string>(nullable: true),
                    DirectionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turnovers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Turnovers_Directions_DirectionId",
                        column: x => x.DirectionId,
                        principalTable: "Directions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Equipments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    CategoryId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipments_EquipmentCategoryes_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "EquipmentCategoryes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Parkings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    StantionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parkings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parkings_Stantions_StantionId",
                        column: x => x.StantionId,
                        principalTable: "Stantions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trains",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    StantionId = table.Column<int>(nullable: true),
                    DirectionId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trains_Directions_DirectionId",
                        column: x => x.DirectionId,
                        principalTable: "Directions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trains_Stantions_StantionId",
                        column: x => x.StantionId,
                        principalTable: "Stantions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DayOfTrips",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    TripId = table.Column<int>(nullable: false),
                    Day = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayOfTrips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DayOfTrips_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StantionOnTrips",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    TripId = table.Column<int>(nullable: false),
                    StantionId = table.Column<int>(nullable: false),
                    InTime = table.Column<DateTime>(nullable: false),
                    OutTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StantionOnTrips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StantionOnTrips_Stantions_StantionId",
                        column: x => x.StantionId,
                        principalTable: "Stantions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StantionOnTrips_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TvPanels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    TVBoxId = table.Column<int>(nullable: false),
                    Number = table.Column<int>(nullable: false),
                    ScreenType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TvPanels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TvPanels_TvBoxes_TVBoxId",
                        column: x => x.TVBoxId,
                        principalTable: "TvBoxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceTasks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    RefId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    DeviceId = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    DeviceFaultId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceTasks_DeviceFaults_DeviceFaultId",
                        column: x => x.DeviceFaultId,
                        principalTable: "DeviceFaults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceTasks_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceTasks_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskPrints",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Name = table.Column<string>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<int>(nullable: true),
                    LabelType = table.Column<int>(nullable: false),
                    TemplateLabelId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskPrints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskPrints_TemplateLabels_TemplateLabelId",
                        column: x => x.TemplateLabelId,
                        principalTable: "TemplateLabels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskPrints_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DayOfRoutes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Day = table.Column<int>(nullable: false),
                    TurnoverId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayOfRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DayOfRoutes_Turnovers_TurnoverId",
                        column: x => x.TurnoverId,
                        principalTable: "Turnovers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    TurnoverId = table.Column<int>(nullable: true),
                    Mileage = table.Column<double>(nullable: false, defaultValue: 0.0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Routes_Turnovers_TurnoverId",
                        column: x => x.TurnoverId,
                        principalTable: "Turnovers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentActs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    EquipmentId = table.Column<int>(nullable: false),
                    ActCategoryId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentActs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentActs_ActCategories_ActCategoryId",
                        column: x => x.ActCategoryId,
                        principalTable: "ActCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentActs_Equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentModels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    ModelId = table.Column<int>(nullable: false),
                    IsMark = table.Column<bool>(nullable: true),
                    EquipmentId = table.Column<int>(nullable: false),
                    ParentId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentModels_Equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentModels_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentModels_EquipmentModels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "EquipmentModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FaultEquipments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    EquipmentId = table.Column<int>(nullable: false),
                    FaultId = table.Column<int>(nullable: false),
                    TaskLevel = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaultEquipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaultEquipments_Equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FaultEquipments_Faults_FaultId",
                        column: x => x.FaultId,
                        principalTable: "Faults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Carriages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Serial = table.Column<string>(nullable: true),
                    Number = table.Column<int>(nullable: false),
                    TrainId = table.Column<int>(nullable: true),
                    ModelId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carriages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carriages_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Carriages_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspectionTerminals",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    SendStatus = table.Column<int>(nullable: false),
                    CheckListType = table.Column<int>(nullable: false),
                    DateStart = table.Column<DateTime>(nullable: false),
                    DateEnd = table.Column<DateTime>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    TrainId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionTerminals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionTerminals_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InspectionTerminals_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TechPasses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Number = table.Column<string>(nullable: false),
                    PlaceDrawUp = table.Column<string>(nullable: false),
                    DateDrawUp = table.Column<DateTime>(nullable: false),
                    TrainId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechPasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechPasses_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceTaskComments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    RefId = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Text = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: false),
                    DeviceTaskId = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTaskComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceTaskComments_DeviceTasks_DeviceTaskId",
                        column: x => x.DeviceTaskId,
                        principalTable: "DeviceTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceTaskComments_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepoEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    ParkingId = table.Column<int>(nullable: false),
                    InspectionId = table.Column<int>(nullable: true),
                    InspectionTxt = table.Column<string>(nullable: true),
                    TrainId = table.Column<int>(nullable: false),
                    RouteId = table.Column<int>(nullable: true),
                    InTime = table.Column<DateTime>(nullable: false),
                    ParkingTime = table.Column<DateTime>(nullable: true),
                    RepairStopTime = table.Column<DateTime>(nullable: true),
                    TestStartTime = table.Column<DateTime>(nullable: true),
                    TestStopTime = table.Column<DateTime>(nullable: true),
                    UserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepoEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepoEvents_Parkings_ParkingId",
                        column: x => x.ParkingId,
                        principalTable: "Parkings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepoEvents_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepoEvents_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepoEvents_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspectionRoutes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    RouteId = table.Column<int>(nullable: false),
                    Start = table.Column<DateTime>(nullable: false),
                    End = table.Column<DateTime>(nullable: false),
                    CheckListType = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionRoutes_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanedRouteTrains",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    RouteId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    TrainId = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanedRouteTrains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanedRouteTrains_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanedRouteTrains_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanedRouteTrains_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TerminalBrigadeTrains",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    PlanedRouteTrainId = table.Column<int>(nullable: false),
                    StantionEndId = table.Column<int>(nullable: false),
                    StantionStartId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    BrigadeType = table.Column<int>(nullable: false),
                    TrainId = table.Column<int>(nullable: false),
                    RouteId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerminalBrigadeTrains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TerminalBrigadeTrains_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerminalBrigadeTrains_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerminalBrigadeTrains_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TerminalInspectionRouteTrains",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    CheckListType = table.Column<int>(nullable: false),
                    Start = table.Column<DateTime>(nullable: false),
                    End = table.Column<DateTime>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    RouteId = table.Column<int>(nullable: false),
                    TrainId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerminalInspectionRouteTrains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TerminalInspectionRouteTrains_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerminalInspectionRouteTrains_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TripOnRoutes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    RouteId = table.Column<int>(nullable: false),
                    TripId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripOnRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripOnRoutes_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TripOnRoutes_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CheckListEquipments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    CheckListType = table.Column<int>(nullable: false),
                    ValueType = table.Column<int>(nullable: false),
                    Value = table.Column<int>(nullable: false),
                    FaultType = table.Column<int>(nullable: false),
                    NameTask = table.Column<string>(nullable: true),
                    EquipmentModelId = table.Column<int>(nullable: false),
                    TaskLevel = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckListEquipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckListEquipments_EquipmentModels_EquipmentModelId",
                        column: x => x.EquipmentModelId,
                        principalTable: "EquipmentModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarriageMigrations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    CarriageId = table.Column<int>(nullable: false),
                    TrainId = table.Column<int>(nullable: false),
                    StantionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarriageMigrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarriageMigrations_Carriages_CarriageId",
                        column: x => x.CarriageId,
                        principalTable: "Carriages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarriageMigrations_Stantions_StantionId",
                        column: x => x.StantionId,
                        principalTable: "Stantions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarriageMigrations_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Labels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    Rfid = table.Column<string>(nullable: true),
                    LabelType = table.Column<int>(nullable: false),
                    CarriageId = table.Column<int>(nullable: false),
                    EquipmentModelId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Labels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Labels_Carriages_CarriageId",
                        column: x => x.CarriageId,
                        principalTable: "Carriages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Labels_EquipmentModels_EquipmentModelId",
                        column: x => x.EquipmentModelId,
                        principalTable: "EquipmentModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrainTaskTerminals",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    SendStatus = table.Column<int>(nullable: false),
                    TaskNumber = table.Column<int>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    TaskType = table.Column<int>(nullable: false),
                    CarriageId = table.Column<int>(nullable: false),
                    EquipmentModelId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainTaskTerminals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainTaskTerminals_Carriages_CarriageId",
                        column: x => x.CarriageId,
                        principalTable: "Carriages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainTaskTerminals_EquipmentModels_EquipmentModelId",
                        column: x => x.EquipmentModelId,
                        principalTable: "EquipmentModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainTaskTerminals_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionDataTerminals",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    SendStatus = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Value = table.Column<int>(nullable: false),
                    InspectionTerminalId = table.Column<Guid>(nullable: false),
                    CarriageId = table.Column<int>(nullable: true),
                    Text = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionDataTerminals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionDataTerminals_Carriages_CarriageId",
                        column: x => x.CarriageId,
                        principalTable: "Carriages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionDataTerminals_InspectionTerminals_InspectionTerminalId",
                        column: x => x.InspectionTerminalId,
                        principalTable: "InspectionTerminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SignatureTerminals",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    SendStatus = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    InspectionId = table.Column<Guid>(nullable: false),
                    CaptionImage = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignatureTerminals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignatureTerminals_InspectionTerminals_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "InspectionTerminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SignatureTerminals_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanedInspectionRoutes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    PlanedRouteTrainId = table.Column<int>(nullable: false),
                    Start = table.Column<DateTime>(nullable: false),
                    End = table.Column<DateTime>(nullable: false),
                    CheckListType = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanedInspectionRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanedInspectionRoutes_PlanedRouteTrains_PlanedRouteTrainId",
                        column: x => x.PlanedRouteTrainId,
                        principalTable: "PlanedRouteTrains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaneStantionOnTrips",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    StantionId = table.Column<int>(nullable: false),
                    TripId = table.Column<int>(nullable: false),
                    InTime = table.Column<DateTime>(nullable: false),
                    OutTime = table.Column<DateTime>(nullable: false),
                    PlanedRouteTrainId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaneStantionOnTrips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaneStantionOnTrips_PlanedRouteTrains_PlanedRouteTrainId",
                        column: x => x.PlanedRouteTrainId,
                        principalTable: "PlanedRouteTrains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaneStantionOnTrips_Stantions_StantionId",
                        column: x => x.StantionId,
                        principalTable: "Stantions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaneStantionOnTrips_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeterageTerminals",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    SendStatus = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Value = table.Column<int>(nullable: true),
                    InspectionId = table.Column<Guid>(nullable: true),
                    LabelId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeterageTerminals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeterageTerminals_InspectionTerminals_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "InspectionTerminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MeterageTerminals_Labels_LabelId",
                        column: x => x.LabelId,
                        principalTable: "Labels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskPrintItems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    LabelId = table.Column<int>(nullable: true),
                    TaskPrintId = table.Column<int>(nullable: true),
                    TimePrinted = table.Column<long>(nullable: false),
                    CanPrinted = table.Column<bool>(nullable: false),
                    UserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskPrintItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskPrintItems_Labels_LabelId",
                        column: x => x.LabelId,
                        principalTable: "Labels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskPrintItems_TaskPrints_TaskPrintId",
                        column: x => x.TaskPrintId,
                        principalTable: "TaskPrints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskPrintItems_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrainTaskAttributeTerminals",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    SendStatus = table.Column<int>(nullable: false),
                    TrainTaskId = table.Column<Guid>(nullable: false),
                    InspectionId = table.Column<Guid>(nullable: true),
                    CheckListEquipmentId = table.Column<int>(nullable: true),
                    FaultId = table.Column<int>(nullable: true),
                    UserId = table.Column<int>(nullable: false),
                    Value = table.Column<int>(nullable: true),
                    TaskLevel = table.Column<int>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainTaskAttributeTerminals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainTaskAttributeTerminals_CheckListEquipments_CheckListEquipmentId",
                        column: x => x.CheckListEquipmentId,
                        principalTable: "CheckListEquipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainTaskAttributeTerminals_Faults_FaultId",
                        column: x => x.FaultId,
                        principalTable: "Faults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainTaskAttributeTerminals_InspectionTerminals_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "InspectionTerminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainTaskAttributeTerminals_TrainTaskTerminals_TrainTaskId",
                        column: x => x.TrainTaskId,
                        principalTable: "TrainTaskTerminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainTaskAttributeTerminals_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainTaskCommentTerminals",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    SendStatus = table.Column<int>(nullable: false),
                    TrainTaskId = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    Text = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainTaskCommentTerminals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainTaskCommentTerminals_TrainTaskTerminals_TrainTaskId",
                        column: x => x.TrainTaskId,
                        principalTable: "TrainTaskTerminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainTaskCommentTerminals_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainTaskExecutorTerminals",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    SendStatus = table.Column<int>(nullable: false),
                    TrainTaskId = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    BrigadeType = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainTaskExecutorTerminals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainTaskExecutorTerminals_TrainTaskTerminals_TrainTaskId",
                        column: x => x.TrainTaskId,
                        principalTable: "TrainTaskTerminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainTaskExecutorTerminals_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainTaskStatusTerminals",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    SendStatus = table.Column<int>(nullable: false),
                    TrainTaskId = table.Column<Guid>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    TrainTaskTerminalId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainTaskStatusTerminals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainTaskStatusTerminals_TrainTaskTerminals_TrainTaskTerminalId",
                        column: x => x.TrainTaskTerminalId,
                        principalTable: "TrainTaskTerminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainTaskStatusTerminals_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaneBrigadeTrains",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "datetime('now')"),
                    StantionStartId = table.Column<int>(nullable: false),
                    StantionEndId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    PlanedRouteTrainId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaneBrigadeTrains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaneBrigadeTrains_PlanedRouteTrains_PlanedRouteTrainId",
                        column: x => x.PlanedRouteTrainId,
                        principalTable: "PlanedRouteTrains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaneBrigadeTrains_PlaneStantionOnTrips_StantionEndId",
                        column: x => x.StantionEndId,
                        principalTable: "PlaneStantionOnTrips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlaneBrigadeTrains_PlaneStantionOnTrips_StantionStartId",
                        column: x => x.StantionStartId,
                        principalTable: "PlaneStantionOnTrips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlaneBrigadeTrains_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTerminals",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    SendStatus = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    DocumentType = table.Column<int>(nullable: false, defaultValue: 1),
                    TrainTaskCommentTerminalId = table.Column<Guid>(nullable: true),
                    Data = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTerminals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentTerminals_TrainTaskCommentTerminals_TrainTaskCommentTerminalId",
                        column: x => x.TrainTaskCommentTerminalId,
                        principalTable: "TrainTaskCommentTerminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspectionTagScaneds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    SendStatus = table.Column<int>(nullable: false),
                    LabelId = table.Column<int>(nullable: false),
                    InspectionId = table.Column<Guid>(nullable: true),
                    IsRfidScaned = table.Column<bool>(nullable: false),
                    TaskStatusId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionTagScaneds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionTagScaneds_InspectionTerminals_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "InspectionTerminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionTagScaneds_Labels_LabelId",
                        column: x => x.LabelId,
                        principalTable: "Labels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InspectionTagScaneds_TrainTaskStatusTerminals_TaskStatusId",
                        column: x => x.TaskStatusId,
                        principalTable: "TrainTaskStatusTerminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_auth_users_BrigadeId",
                table: "auth_users",
                column: "BrigadeId");

            migrationBuilder.CreateIndex(
                name: "IX_auth_users_Login",
                table: "auth_users",
                column: "Login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_auth_users_RoleId",
                table: "auth_users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CarriageMigrations_CarriageId",
                table: "CarriageMigrations",
                column: "CarriageId");

            migrationBuilder.CreateIndex(
                name: "IX_CarriageMigrations_StantionId",
                table: "CarriageMigrations",
                column: "StantionId");

            migrationBuilder.CreateIndex(
                name: "IX_CarriageMigrations_TrainId",
                table: "CarriageMigrations",
                column: "TrainId");

            migrationBuilder.CreateIndex(
                name: "IX_Carriages_ModelId",
                table: "Carriages",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Carriages_TrainId",
                table: "Carriages",
                column: "TrainId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckListEquipments_EquipmentModelId",
                table: "CheckListEquipments",
                column: "EquipmentModelId");

            migrationBuilder.CreateIndex(
                name: "IX_DayOfRoutes_TurnoverId",
                table: "DayOfRoutes",
                column: "TurnoverId");

            migrationBuilder.CreateIndex(
                name: "IX_DayOfTrips_TripId",
                table: "DayOfTrips",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_DepoEvents_ParkingId",
                table: "DepoEvents",
                column: "ParkingId");

            migrationBuilder.CreateIndex(
                name: "IX_DepoEvents_RouteId",
                table: "DepoEvents",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_DepoEvents_TrainId",
                table: "DepoEvents",
                column: "TrainId");

            migrationBuilder.CreateIndex(
                name: "IX_DepoEvents_UserId",
                table: "DepoEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTaskComments_DeviceTaskId",
                table: "DeviceTaskComments",
                column: "DeviceTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTaskComments_UserId",
                table: "DeviceTaskComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTasks_DeviceFaultId",
                table: "DeviceTasks",
                column: "DeviceFaultId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTasks_DeviceId",
                table: "DeviceTasks",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTasks_UserId",
                table: "DeviceTasks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTerminals_TrainTaskCommentTerminalId",
                table: "DocumentTerminals",
                column: "TrainTaskCommentTerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentActs_ActCategoryId",
                table: "EquipmentActs",
                column: "ActCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentActs_EquipmentId",
                table: "EquipmentActs",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentModels_EquipmentId",
                table: "EquipmentModels",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentModels_ModelId",
                table: "EquipmentModels",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentModels_ParentId",
                table: "EquipmentModels",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_CategoryId",
                table: "Equipments",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultEquipments_EquipmentId",
                table: "FaultEquipments",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultEquipments_FaultId",
                table: "FaultEquipments",
                column: "FaultId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionDataTerminals_CarriageId",
                table: "InspectionDataTerminals",
                column: "CarriageId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionDataTerminals_InspectionTerminalId",
                table: "InspectionDataTerminals",
                column: "InspectionTerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionRoutes_RouteId",
                table: "InspectionRoutes",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTagScaneds_InspectionId",
                table: "InspectionTagScaneds",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTagScaneds_LabelId",
                table: "InspectionTagScaneds",
                column: "LabelId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTagScaneds_TaskStatusId",
                table: "InspectionTagScaneds",
                column: "TaskStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTerminals_TrainId",
                table: "InspectionTerminals",
                column: "TrainId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTerminals_UserId",
                table: "InspectionTerminals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Labels_CarriageId",
                table: "Labels",
                column: "CarriageId");

            migrationBuilder.CreateIndex(
                name: "IX_Labels_EquipmentModelId",
                table: "Labels",
                column: "EquipmentModelId");

            migrationBuilder.CreateIndex(
                name: "IX_MeterageTerminals_InspectionId",
                table: "MeterageTerminals",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MeterageTerminals_LabelId",
                table: "MeterageTerminals",
                column: "LabelId");

            migrationBuilder.CreateIndex(
                name: "IX_Parkings_StantionId",
                table: "Parkings",
                column: "StantionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaneBrigadeTrains_PlanedRouteTrainId",
                table: "PlaneBrigadeTrains",
                column: "PlanedRouteTrainId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaneBrigadeTrains_StantionEndId",
                table: "PlaneBrigadeTrains",
                column: "StantionEndId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaneBrigadeTrains_StantionStartId",
                table: "PlaneBrigadeTrains",
                column: "StantionStartId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaneBrigadeTrains_UserId",
                table: "PlaneBrigadeTrains",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanedInspectionRoutes_PlanedRouteTrainId",
                table: "PlanedInspectionRoutes",
                column: "PlanedRouteTrainId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanedRouteTrains_RouteId",
                table: "PlanedRouteTrains",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanedRouteTrains_TrainId",
                table: "PlanedRouteTrains",
                column: "TrainId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanedRouteTrains_UserId",
                table: "PlanedRouteTrains",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaneStantionOnTrips_PlanedRouteTrainId",
                table: "PlaneStantionOnTrips",
                column: "PlanedRouteTrainId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaneStantionOnTrips_StantionId",
                table: "PlaneStantionOnTrips",
                column: "StantionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaneStantionOnTrips_TripId",
                table: "PlaneStantionOnTrips",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_TurnoverId",
                table: "Routes",
                column: "TurnoverId");

            migrationBuilder.CreateIndex(
                name: "IX_SignatureTerminals_InspectionId",
                table: "SignatureTerminals",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_SignatureTerminals_UserId",
                table: "SignatureTerminals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StantionOnTrips_StantionId",
                table: "StantionOnTrips",
                column: "StantionId");

            migrationBuilder.CreateIndex(
                name: "IX_StantionOnTrips_TripId",
                table: "StantionOnTrips",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskPrintItems_LabelId",
                table: "TaskPrintItems",
                column: "LabelId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskPrintItems_TaskPrintId",
                table: "TaskPrintItems",
                column: "TaskPrintId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskPrintItems_UserId",
                table: "TaskPrintItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskPrints_TemplateLabelId",
                table: "TaskPrints",
                column: "TemplateLabelId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskPrints_UserId",
                table: "TaskPrints",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TechPasses_TrainId",
                table: "TechPasses",
                column: "TrainId");

            migrationBuilder.CreateIndex(
                name: "IX_TerminalBrigadeTrains_RouteId",
                table: "TerminalBrigadeTrains",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_TerminalBrigadeTrains_TrainId",
                table: "TerminalBrigadeTrains",
                column: "TrainId");

            migrationBuilder.CreateIndex(
                name: "IX_TerminalBrigadeTrains_UserId",
                table: "TerminalBrigadeTrains",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TerminalInspectionRouteTrains_RouteId",
                table: "TerminalInspectionRouteTrains",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_TerminalInspectionRouteTrains_TrainId",
                table: "TerminalInspectionRouteTrains",
                column: "TrainId");

            migrationBuilder.CreateIndex(
                name: "IX_Trains_DirectionId",
                table: "Trains",
                column: "DirectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Trains_StantionId",
                table: "Trains",
                column: "StantionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainTaskAttributeTerminals_CheckListEquipmentId",
                table: "TrainTaskAttributeTerminals",
                column: "CheckListEquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainTaskAttributeTerminals_FaultId",
                table: "TrainTaskAttributeTerminals",
                column: "FaultId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainTaskAttributeTerminals_InspectionId",
                table: "TrainTaskAttributeTerminals",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainTaskAttributeTerminals_TrainTaskId",
                table: "TrainTaskAttributeTerminals",
                column: "TrainTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainTaskAttributeTerminals_UserId",
                table: "TrainTaskAttributeTerminals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainTaskCommentTerminals_TrainTaskId",
                table: "TrainTaskCommentTerminals",
                column: "TrainTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainTaskCommentTerminals_UserId",
                table: "TrainTaskCommentTerminals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainTaskExecutorTerminals_TrainTaskId",
                table: "TrainTaskExecutorTerminals",
                column: "TrainTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainTaskExecutorTerminals_UserId",
                table: "TrainTaskExecutorTerminals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainTaskStatusTerminals_TrainTaskTerminalId",
                table: "TrainTaskStatusTerminals",
                column: "TrainTaskTerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainTaskStatusTerminals_UserId",
                table: "TrainTaskStatusTerminals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainTaskTerminals_CarriageId",
                table: "TrainTaskTerminals",
                column: "CarriageId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainTaskTerminals_EquipmentModelId",
                table: "TrainTaskTerminals",
                column: "EquipmentModelId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainTaskTerminals_UserId",
                table: "TrainTaskTerminals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TripOnRoutes_RouteId",
                table: "TripOnRoutes",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_TripOnRoutes_TripId",
                table: "TripOnRoutes",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_Turnovers_DirectionId",
                table: "Turnovers",
                column: "DirectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TvPanels_TVBoxId",
                table: "TvPanels",
                column: "TVBoxId");

            migrationBuilder.AddColumn<DateTime>(
                "OutTime",
                "DepoEvents",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarriageMigrations");

            migrationBuilder.DropTable(
                name: "DayOfRoutes");

            migrationBuilder.DropTable(
                name: "DayOfTrips");

            migrationBuilder.DropTable(
                name: "DepoEvents");

            migrationBuilder.DropTable(
                name: "DeviceTaskComments");

            migrationBuilder.DropTable(
                name: "DocumentTerminals");

            migrationBuilder.DropTable(
                name: "EquipmentActs");

            migrationBuilder.DropTable(
                name: "FaultEquipments");

            migrationBuilder.DropTable(
                name: "InspectionDataTerminals");

            migrationBuilder.DropTable(
                name: "InspectionRoutes");

            migrationBuilder.DropTable(
                name: "InspectionTagScaneds");

            migrationBuilder.DropTable(
                name: "MeterageTerminals");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "PlaneBrigadeTrains");

            migrationBuilder.DropTable(
                name: "PlanedInspectionRoutes");

            migrationBuilder.DropTable(
                name: "SignatureTerminals");

            migrationBuilder.DropTable(
                name: "StantionOnTrips");

            migrationBuilder.DropTable(
                name: "TaskPrintItems");

            migrationBuilder.DropTable(
                name: "TechPasses");

            migrationBuilder.DropTable(
                name: "TerminalBrigadeTrains");

            migrationBuilder.DropTable(
                name: "TerminalInspectionRouteTrains");

            migrationBuilder.DropTable(
                name: "TrainTaskAttributeTerminals");

            migrationBuilder.DropTable(
                name: "TrainTaskExecutorTerminals");

            migrationBuilder.DropTable(
                name: "TripOnRoutes");

            migrationBuilder.DropTable(
                name: "TvPanels");

            migrationBuilder.DropTable(
                name: "Parkings");

            migrationBuilder.DropTable(
                name: "DeviceTasks");

            migrationBuilder.DropTable(
                name: "TrainTaskCommentTerminals");

            migrationBuilder.DropTable(
                name: "ActCategories");

            migrationBuilder.DropTable(
                name: "TrainTaskStatusTerminals");

            migrationBuilder.DropTable(
                name: "PlaneStantionOnTrips");

            migrationBuilder.DropTable(
                name: "Labels");

            migrationBuilder.DropTable(
                name: "TaskPrints");

            migrationBuilder.DropTable(
                name: "CheckListEquipments");

            migrationBuilder.DropTable(
                name: "Faults");

            migrationBuilder.DropTable(
                name: "InspectionTerminals");

            migrationBuilder.DropTable(
                name: "TvBoxes");

            migrationBuilder.DropTable(
                name: "DeviceFaults");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "TrainTaskTerminals");

            migrationBuilder.DropTable(
                name: "PlanedRouteTrains");

            migrationBuilder.DropTable(
                name: "Trips");

            migrationBuilder.DropTable(
                name: "TemplateLabels");

            migrationBuilder.DropTable(
                name: "Carriages");

            migrationBuilder.DropTable(
                name: "EquipmentModels");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "auth_users");

            migrationBuilder.DropTable(
                name: "Trains");

            migrationBuilder.DropTable(
                name: "Equipments");

            migrationBuilder.DropTable(
                name: "Models");

            migrationBuilder.DropTable(
                name: "Turnovers");

            migrationBuilder.DropTable(
                name: "Brigades");

            migrationBuilder.DropTable(
                name: "auth_roles");

            migrationBuilder.DropTable(
                name: "Stantions");

            migrationBuilder.DropTable(
                name: "EquipmentCategoryes");

            migrationBuilder.DropTable(
                name: "Directions");

            migrationBuilder.DropColumn(
                name: "OutTime",
                table: "DepoEvents");
        }
    }
}
