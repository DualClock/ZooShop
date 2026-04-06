using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ZooShop.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnimalTypes",
                columns: table => new
                {
                    AnimalTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalTypes", x => x.AnimalTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    BrandID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.BrandID);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentCategoryID = table.Column<int>(type: "int", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryID);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryID",
                        column: x => x.ParentCategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID");
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    ClientID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiscountCardNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.ClientID);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeID);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    WarehouseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.WarehouseID);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SKU = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Barcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Composition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BrandID = table.Column<int>(type: "int", nullable: true),
                    CategoryID = table.Column<int>(type: "int", nullable: true),
                    AnimalTypeID = table.Column<int>(type: "int", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductID);
                    table.ForeignKey(
                        name: "FK_Products_AnimalTypes_AnimalTypeID",
                        column: x => x.AnimalTypeID,
                        principalTable: "AnimalTypes",
                        principalColumn: "AnimalTypeID");
                    table.ForeignKey(
                        name: "FK_Products_Brands_BrandID",
                        column: x => x.BrandID,
                        principalTable: "Brands",
                        principalColumn: "BrandID");
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID");
                });

            migrationBuilder.CreateTable(
                name: "Pets",
                columns: table => new
                {
                    PetID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientID = table.Column<int>(type: "int", nullable: false),
                    PetName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnimalTypeID = table.Column<int>(type: "int", nullable: false),
                    Breed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Allergies = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecialNeeds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pets", x => x.PetID);
                    table.ForeignKey(
                        name: "FK_Pets_AnimalTypes_AnimalTypeID",
                        column: x => x.AnimalTypeID,
                        principalTable: "AnimalTypes",
                        principalColumn: "AnimalTypeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pets_Clients_ClientID",
                        column: x => x.ClientID,
                        principalTable: "Clients",
                        principalColumn: "ClientID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    ScheduleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShiftStart = table.Column<TimeSpan>(type: "time", nullable: true),
                    ShiftEnd = table.Column<TimeSpan>(type: "time", nullable: true),
                    ShiftType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsWorked = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.ScheduleID);
                    table.ForeignKey(
                        name: "FK_Schedules_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    TaskID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedTo = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.TaskID);
                    table.ForeignKey(
                        name: "FK_Tasks_Employees_AssignedTo",
                        column: x => x.AssignedTo,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Employees_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    SaleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SaleDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClientID = table.Column<int>(type: "int", nullable: true),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    WarehouseID = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPrinted = table.Column<bool>(type: "bit", nullable: false),
                    EmailSent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales", x => x.SaleID);
                    table.ForeignKey(
                        name: "FK_Sales_Clients_ClientID",
                        column: x => x.ClientID,
                        principalTable: "Clients",
                        principalColumn: "ClientID");
                    table.ForeignKey(
                        name: "FK_Sales_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sales_Warehouses_WarehouseID",
                        column: x => x.WarehouseID,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryMovements",
                columns: table => new
                {
                    MovementID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    WarehouseID = table.Column<int>(type: "int", nullable: false),
                    MovementType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DocumentNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    MovementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryMovements", x => x.MovementID);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Warehouses_WarehouseID",
                        column: x => x.WarehouseID,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriceHistory",
                columns: table => new
                {
                    PriceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceHistory", x => x.PriceID);
                    table.ForeignKey(
                        name: "FK_PriceHistory_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaleItems",
                columns: table => new
                {
                    SaleItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SaleID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleItems", x => x.SaleItemID);
                    table.ForeignKey(
                        name: "FK_SaleItems_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaleItems_Sales_SaleID",
                        column: x => x.SaleID,
                        principalTable: "Sales",
                        principalColumn: "SaleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AnimalTypes",
                columns: new[] { "AnimalTypeID", "Description", "TypeName" },
                values: new object[,]
                {
                    { 1, "Товары для собак", "Собаки" },
                    { 2, "Товары для кошек", "Кошки" },
                    { 3, "Товары для птиц", "Птицы" },
                    { 4, "Товары для рыбок", "Рыбки" },
                    { 5, "Товары для грызунов", "Грызуны" }
                });

            migrationBuilder.InsertData(
                table: "Brands",
                columns: new[] { "BrandID", "BrandName", "Country", "Website" },
                values: new object[,]
                {
                    { 1, "Royal Canin", "Франция", "www.royalcanin.com" },
                    { 2, "Pro Plan", "США", "www.proplan.ru" },
                    { 3, "Pedigree", "США", "www.pedigree.ru" },
                    { 4, "Whiskas", "США", "www.whiskas.ru" },
                    { 5, "Trixie", "Германия", "www.trixie.de" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryID", "CategoryName", "Icon", "ParentCategoryID" },
                values: new object[,]
                {
                    { 1, "Корма", null, null },
                    { 2, "Игрушки", null, null },
                    { 3, "Аксессуары", null, null },
                    { 4, "Лакомства", null, null }
                });

            migrationBuilder.InsertData(
                table: "Clients",
                columns: new[] { "ClientID", "BirthDate", "DiscountCardNumber", "Email", "FirstName", "LastName", "Notes", "Phone", "RegistrationDate" },
                values: new object[,]
                {
                    { 1, new DateTime(1985, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "ZS-00123", "elena@mail.ru", "Елена", "Смирнова", "Постоянный клиент", "+7-926-100-20-30", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(1990, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "ZS-00124", "dmitry@mail.ru", "Дмитрий", "Волков", "Предпочитает натуральные корма", "+7-926-200-30-40", new DateTime(2026, 2, 5, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "EmployeeID", "Email", "FirstName", "IsActive", "LastName", "MiddleName", "PasswordHash", "Phone", "Position" },
                values: new object[,]
                {
                    { 1, "anna@zooshop.ru", "Анна", true, "Петрова", "Ивановна", "hash1", "+7-916-111-22-33", "Продавец" },
                    { 2, "ivan@zooshop.ru", "Иван", true, "Сидоров", "Петрович", "hash2", "+7-916-222-33-44", "Продавец" },
                    { 3, "maria@zooshop.ru", "Мария", true, "Козлова", "Сергеевна", "hash3", "+7-916-333-44-55", "Администратор" }
                });

            migrationBuilder.InsertData(
                table: "Warehouses",
                columns: new[] { "WarehouseID", "Address", "Phone", "WarehouseName" },
                values: new object[,]
                {
                    { 1, "ул. Складская, 10", "+7-495-111-22-33", "Основной склад" },
                    { 2, "ул. Зоологическая, 5", "+7-495-444-55-66", "Магазин №1" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryID", "CategoryName", "Icon", "ParentCategoryID" },
                values: new object[,]
                {
                    { 5, "Сухие корма", null, 1 },
                    { 6, "Влажные корма", null, 1 },
                    { 7, "Мячики", null, 2 },
                    { 8, "Лежанки", null, 3 }
                });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "PetID", "Allergies", "AnimalTypeID", "BirthDate", "Breed", "ClientID", "Gender", "PetName", "PhotoUrl", "SpecialNeeds", "Weight" },
                values: new object[,]
                {
                    { 1, "Курица", 2, new DateTime(2023, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Британская короткошёрстная", 1, "Мальчик", "Барсик", null, "Диета для чувствительного пищеварения", 5.2m },
                    { 2, "", 1, new DateTime(2022, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Немецкая овчарка", 1, "Мальчик", "Рекс", null, "", 32.0m },
                    { 3, "Рыба, курица", 2, new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Персидская", 2, "Девочка", "Муся", null, "Ежедневный уход за шерстью", 3.8m }
                });

            migrationBuilder.InsertData(
                table: "Schedules",
                columns: new[] { "ScheduleID", "EmployeeID", "IsWorked", "Notes", "ShiftEnd", "ShiftStart", "ShiftType", "WorkDate" },
                values: new object[,]
                {
                    { 1, 1, true, null, new TimeSpan(0, 18, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0), "Утренняя", new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 2, true, null, new TimeSpan(0, 21, 0, 0, 0), new TimeSpan(0, 12, 0, 0, 0), "Вечерняя", new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, 3, true, null, new TimeSpan(0, 18, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0), "Полный день", new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, 1, false, null, new TimeSpan(0, 18, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0), "Утренняя", new DateTime(2026, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, 2, false, null, new TimeSpan(0, 18, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0), "Полный день", new DateTime(2026, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "TaskID", "AssignedTo", "CompletedDate", "CreatedAt", "CreatedBy", "Description", "DueDate", "Priority", "Status", "TaskTitle" },
                values: new object[,]
                {
                    { 1, 1, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "Принять и разместить на складе 200 единиц корма Royal Canin", new DateTime(2026, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Высокий", "Новая", "Приёмка новой партии кормов" },
                    { 2, 2, null, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "Обновить экспозицию кормов и аксессуаров", new DateTime(2026, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Средний", "В работе", "Выкладка товаров в зале" },
                    { 3, 3, null, new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "Провести полную инвентаризацию основного склада", new DateTime(2026, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Высокий", "Новая", "Инвентаризация склада" },
                    { 4, 1, new DateTime(2026, 4, 5, 18, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "Ежедневная уборка торгового зала", new DateTime(2026, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Низкий", "Выполнена", "Уборка в зале" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductID", "AnimalTypeID", "Barcode", "BrandID", "CategoryID", "Composition", "CreatedAt", "Description", "ImageUrl", "IsActive", "ProductName", "SKU", "Unit" },
                values: new object[,]
                {
                    { 1, 1, null, 1, 5, "Курица, рис, кукуруза, витамины A/D3/E", new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Сухой корм для взрослых собак средних пород (9-25 кг)", null, true, "Royal Canin для собак средних пород, 10кг", "RC-DOG-001", "шт" },
                    { 2, 2, null, 2, 6, "Лосось, подсолнечное масло, минералы", new DateTime(2026, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Влажный корм для стерилизованных кошек", null, true, "Pro Plan для кошек стерилизованных, 1.5кг", "PP-CAT-002", "шт" },
                    { 3, 1, null, 5, 7, "Резина, хлопок", new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Прочный резиновый мяч с верёвкой для активных игр", null, true, "Мяч Trixie для собак с верёвкой", "TR-BALL-003", "шт" },
                    { 4, 2, null, 4, 6, "Курица, бульон, загустители, витамины", new DateTime(2026, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Влажный корм для взрослых кошек с курицей в соусе", null, true, "Whiskas паучи с курицей, набор 12шт", "WH-CAT-004", "уп" },
                    { 5, 1, null, 1, 8, "Полиэстер, поролон", new DateTime(2026, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Мягкая лежанка для собак и кошек с антистрессовым покрытием", null, true, "Лежанка Royal Canin мягкая, 60x40см", "RC-BED-005", "шт" }
                });

            migrationBuilder.InsertData(
                table: "InventoryMovements",
                columns: new[] { "MovementID", "DocumentNumber", "EmployeeID", "MovementDate", "MovementType", "Notes", "ProductID", "Quantity", "UnitPrice", "WarehouseID" },
                values: new object[,]
                {
                    { 1, "НАКЛ-001", 3, new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "IN", "Первичная приёмка", 1, 50, 3000m, 1 },
                    { 2, "ПЕР-001", 3, new DateTime(2026, 3, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "TRANSFER", "Перемещение в магазин", 1, 15, null, 2 },
                    { 3, "НАКЛ-002", 3, new DateTime(2026, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "IN", "Первичная приёмка", 2, 100, 800m, 1 },
                    { 4, "ПЕР-002", 3, new DateTime(2026, 3, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "TRANSFER", "Перемещение в магазин", 2, 30, null, 2 },
                    { 5, "НАКЛ-003", 3, new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "IN", "Приёмка игрушек", 3, 40, 400m, 2 },
                    { 6, "НАКЛ-004", 3, new DateTime(2026, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "IN", "Приёмка влажных кормов", 4, 80, 600m, 1 },
                    { 7, "ПЕР-003", 3, new DateTime(2026, 3, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "TRANSFER", "Перемещение в магазин", 4, 25, null, 2 },
                    { 8, "НАКЛ-005", 3, new DateTime(2026, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "IN", "Приёмка аксессуаров", 5, 10, 2200m, 2 }
                });

            migrationBuilder.InsertData(
                table: "PriceHistory",
                columns: new[] { "PriceID", "EffectiveDate", "EndDate", "Price", "ProductID" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 4500m, 1 },
                    { 2, new DateTime(2026, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1200m, 2 },
                    { 3, new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 650m, 3 },
                    { 4, new DateTime(2026, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 890m, 4 },
                    { 5, new DateTime(2026, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 3200m, 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryID",
                table: "Categories",
                column: "ParentCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_EmployeeID",
                table: "InventoryMovements",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_ProductID",
                table: "InventoryMovements",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_WarehouseID",
                table: "InventoryMovements",
                column: "WarehouseID");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_AnimalTypeID",
                table: "Pets",
                column: "AnimalTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_ClientID",
                table: "Pets",
                column: "ClientID");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistory_ProductID",
                table: "PriceHistory",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_AnimalTypeID",
                table: "Products",
                column: "AnimalTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandID",
                table: "Products",
                column: "BrandID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryID",
                table: "Products",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_ProductID",
                table: "SaleItems",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_SaleID",
                table: "SaleItems",
                column: "SaleID");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_ClientID",
                table: "Sales",
                column: "ClientID");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_EmployeeID",
                table: "Sales",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_WarehouseID",
                table: "Sales",
                column: "WarehouseID");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_EmployeeID",
                table: "Schedules",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedTo",
                table: "Tasks",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CreatedBy",
                table: "Tasks",
                column: "CreatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryMovements");

            migrationBuilder.DropTable(
                name: "Pets");

            migrationBuilder.DropTable(
                name: "PriceHistory");

            migrationBuilder.DropTable(
                name: "SaleItems");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Sales");

            migrationBuilder.DropTable(
                name: "AnimalTypes");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Warehouses");
        }
    }
}
