using Microsoft.EntityFrameworkCore;
using ZooShop.Models;

namespace ZooShop.Data;

public class ZooShopDbContext : DbContext
{
    public static string ConnectionString => DbConfig.ConnectionString;

    public DbSet<AnimalType> AnimalTypes => Set<AnimalType>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<PriceHistory> PriceHistory => Set<PriceHistory>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<ZooTask> Tasks => Set<ZooTask>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<User> Users => Set<User>();

    // Представления
    public DbSet<CurrentStock> CurrentStocks => Set<CurrentStock>();
    public DbSet<TodaySales> TodaySales => Set<TodaySales>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Представления - только для чтения
        modelBuilder.Entity<CurrentStock>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_CurrentStock");
        });

        modelBuilder.Entity<TodaySales>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_TodaySales");
        });

        // Настройка отношений Employee ↔ Task
        modelBuilder.Entity<ZooTask>()
            .HasOne(t => t.AssignedEmployee)
            .WithMany(e => e.AssignedTasks)
            .HasForeignKey(t => t.AssignedTo)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ZooTask>()
            .HasOne(t => t.Creator)
            .WithMany(e => e.CreatedTasks)
            .HasForeignKey(t => t.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Явные PK для составных сущностей
        modelBuilder.Entity<InventoryMovement>().HasKey(e => e.MovementID);
        modelBuilder.Entity<SaleItem>().HasKey(e => e.SaleItemID);
        modelBuilder.Entity<PriceHistory>().HasKey(e => e.PriceID);
        modelBuilder.Entity<Schedule>().HasKey(e => e.ScheduleID);
        modelBuilder.Entity<ZooTask>().HasKey(e => e.TaskID);

        // Связи User ↔ UserRole, User ↔ Employee
        modelBuilder.Entity<UserRole>().HasKey(r => r.RoleID);
        modelBuilder.Entity<User>().HasKey(u => u.UserID);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Employee)
            .WithMany()
            .HasForeignKey(u => u.EmployeeID)
            .OnDelete(DeleteBehavior.SetNull);

        // Seed-данные
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // === Животные ===
        modelBuilder.Entity<AnimalType>().HasData(
            new AnimalType { AnimalTypeID = 1, TypeName = "Собаки", Description = "Товары для собак" },
            new AnimalType { AnimalTypeID = 2, TypeName = "Кошки", Description = "Товары для кошек" },
            new AnimalType { AnimalTypeID = 3, TypeName = "Птицы", Description = "Товары для птиц" },
            new AnimalType { AnimalTypeID = 4, TypeName = "Рыбки", Description = "Товары для рыбок" },
            new AnimalType { AnimalTypeID = 5, TypeName = "Грызуны", Description = "Товары для грызунов" }
        );

        // === Категории ===
        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryID = 1, CategoryName = "Корма", ParentCategoryID = null },
            new Category { CategoryID = 2, CategoryName = "Игрушки", ParentCategoryID = null },
            new Category { CategoryID = 3, CategoryName = "Аксессуары", ParentCategoryID = null },
            new Category { CategoryID = 4, CategoryName = "Лакомства", ParentCategoryID = null },
            new Category { CategoryID = 5, CategoryName = "Сухие корма", ParentCategoryID = 1 },
            new Category { CategoryID = 6, CategoryName = "Влажные корма", ParentCategoryID = 1 },
            new Category { CategoryID = 7, CategoryName = "Мячики", ParentCategoryID = 2 },
            new Category { CategoryID = 8, CategoryName = "Лежанки", ParentCategoryID = 3 }
        );

        // === Бренды ===
        modelBuilder.Entity<Brand>().HasData(
            new Brand { BrandID = 1, BrandName = "Royal Canin", Country = "Франция", Website = "www.royalcanin.com" },
            new Brand { BrandID = 2, BrandName = "Pro Plan", Country = "США", Website = "www.proplan.ru" },
            new Brand { BrandID = 3, BrandName = "Pedigree", Country = "США", Website = "www.pedigree.ru" },
            new Brand { BrandID = 4, BrandName = "Whiskas", Country = "США", Website = "www.whiskas.ru" },
            new Brand { BrandID = 5, BrandName = "Trixie", Country = "Германия", Website = "www.trixie.de" }
        );

        // === Склады ===
        modelBuilder.Entity<Warehouse>().HasData(
            new Warehouse { WarehouseID = 1, WarehouseName = "Основной склад", Address = "ул. Складская, 10", Phone = "+7-495-111-22-33" },
            new Warehouse { WarehouseID = 2, WarehouseName = "Магазин №1", Address = "ул. Зоологическая, 5", Phone = "+7-495-444-55-66" }
        );

        // === Роли пользователей ===
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole { RoleID = 1, RoleName = "Админ", Description = "Администратор системы" },
            new UserRole { RoleID = 2, RoleName = "Сотрудник", Description = "Рядовой сотрудник" }
        );

        // === Пользователи (аккаунты для входа) ===
        modelBuilder.Entity<User>().HasData(
            new User { UserID = 1, Login = "anna@zooshop.ru", Password = "123", RoleID = 2, EmployeeID = 1, IsActive = true },
            new User { UserID = 2, Login = "ivan@zooshop.ru", Password = "123", RoleID = 2, EmployeeID = 2, IsActive = true },
            new User { UserID = 3, Login = "maria@zooshop.ru", Password = "admin", RoleID = 1, EmployeeID = 3, IsActive = true }
        );

        // === Сотрудники ===
        modelBuilder.Entity<Employee>().HasData(
            new Employee { EmployeeID = 1, FirstName = "Анна", LastName = "Петрова", MiddleName = "Ивановна", Phone = "+7-916-111-22-33", Email = "anna@zooshop.ru", Position = "Продавец", Role = "Сотрудник", IsActive = true },
            new Employee { EmployeeID = 2, FirstName = "Иван", LastName = "Сидоров", MiddleName = "Петрович", Phone = "+7-916-222-33-44", Email = "ivan@zooshop.ru", Position = "Продавец", Role = "Сотрудник", IsActive = true },
            new Employee { EmployeeID = 3, FirstName = "Мария", LastName = "Козлова", MiddleName = "Сергеевна", Phone = "+7-916-333-44-55", Email = "maria@zooshop.ru", Position = "Администратор", Role = "Админ", IsActive = true }
        );

        // === Товары (5 товаров) ===
        modelBuilder.Entity<Product>().HasData(
            new Product { ProductID = 1, SKU = "RC-DOG-001", ProductName = "Royal Canin для собак средних пород, 10кг", Description = "Сухой корм для взрослых собак средних пород (9-25 кг)", Composition = "Курица, рис, кукуруза, витамины A/D3/E", BrandID = 1, CategoryID = 5, AnimalTypeID = 1, Unit = "шт", CreatedAt = new DateTime(2026, 3, 1) },
            new Product { ProductID = 2, SKU = "PP-CAT-002", ProductName = "Pro Plan для кошек стерилизованных, 1.5кг", Description = "Влажный корм для стерилизованных кошек", Composition = "Лосось, подсолнечное масло, минералы", BrandID = 2, CategoryID = 6, AnimalTypeID = 2, Unit = "шт", CreatedAt = new DateTime(2026, 3, 5) },
            new Product { ProductID = 3, SKU = "TR-BALL-003", ProductName = "Мяч Trixie для собак с верёвкой", Description = "Прочный резиновый мяч с верёвкой для активных игр", Composition = "Резина, хлопок", BrandID = 5, CategoryID = 7, AnimalTypeID = 1, Unit = "шт", CreatedAt = new DateTime(2026, 3, 10) },
            new Product { ProductID = 4, SKU = "WH-CAT-004", ProductName = "Whiskas паучи с курицей, набор 12шт", Description = "Влажный корм для взрослых кошек с курицей в соусе", Composition = "Курица, бульон, загустители, витамины", BrandID = 4, CategoryID = 6, AnimalTypeID = 2, Unit = "уп", CreatedAt = new DateTime(2026, 3, 15) },
            new Product { ProductID = 5, SKU = "RC-BED-005", ProductName = "Лежанка Royal Canin мягкая, 60x40см", Description = "Мягкая лежанка для собак и кошек с антистрессовым покрытием", Composition = "Полиэстер, поролон", BrandID = 1, CategoryID = 8, AnimalTypeID = 1, Unit = "шт", CreatedAt = new DateTime(2026, 3, 20) }
        );

        // === История цен ===
        modelBuilder.Entity<PriceHistory>().HasData(
            new PriceHistory { PriceID = 1, ProductID = 1, Price = 4500, EffectiveDate = new DateTime(2026, 3, 1) },
            new PriceHistory { PriceID = 2, ProductID = 2, Price = 1200, EffectiveDate = new DateTime(2026, 3, 5) },
            new PriceHistory { PriceID = 3, ProductID = 3, Price = 650, EffectiveDate = new DateTime(2026, 3, 10) },
            new PriceHistory { PriceID = 4, ProductID = 4, Price = 890, EffectiveDate = new DateTime(2026, 3, 15) },
            new PriceHistory { PriceID = 5, ProductID = 5, Price = 3200, EffectiveDate = new DateTime(2026, 3, 20) }
        );

        // === Клиенты (2 клиента) ===
        modelBuilder.Entity<Client>().HasData(
            new Client { ClientID = 1, FirstName = "Елена", LastName = "Смирнова", Phone = "+7-926-100-20-30", Email = "elena@mail.ru", BirthDate = new DateTime(1985, 6, 15), DiscountCardNumber = "ZS-00123", Notes = "Постоянный клиент", RegistrationDate = new DateTime(2026, 1, 10) },
            new Client { ClientID = 2, FirstName = "Дмитрий", LastName = "Волков", Phone = "+7-926-200-30-40", Email = "dmitry@mail.ru", BirthDate = new DateTime(1990, 3, 22), DiscountCardNumber = "ZS-00124", Notes = "Предпочитает натуральные корма", RegistrationDate = new DateTime(2026, 2, 5) }
        );

        // === Питомцы ===
        modelBuilder.Entity<Pet>().HasData(
            new Pet { PetID = 1, ClientID = 1, PetName = "Барсик", AnimalTypeID = 2, Breed = "Британская короткошёрстная", BirthDate = new DateTime(2023, 4, 10), Gender = "Мальчик", Weight = 5.2m, Allergies = "Курица", SpecialNeeds = "Диета для чувствительного пищеварения" },
            new Pet { PetID = 2, ClientID = 1, PetName = "Рекс", AnimalTypeID = 1, Breed = "Немецкая овчарка", BirthDate = new DateTime(2022, 8, 5), Gender = "Мальчик", Weight = 32.0m, Allergies = "", SpecialNeeds = "" },
            new Pet { PetID = 3, ClientID = 2, PetName = "Муся", AnimalTypeID = 2, Breed = "Персидская", BirthDate = new DateTime(2024, 1, 20), Gender = "Девочка", Weight = 3.8m, Allergies = "Рыба, курица", SpecialNeeds = "Ежедневный уход за шерстью" }
        );

        // === Движения товаров (начальные остатки) ===
        modelBuilder.Entity<InventoryMovement>().HasData(
            new InventoryMovement { MovementID = 1, ProductID = 1, WarehouseID = 1, MovementType = "IN", Quantity = 50, UnitPrice = 3000, DocumentNumber = "НАКЛ-001", EmployeeID = 3, MovementDate = new DateTime(2026, 3, 1), Notes = "Первичная приёмка" },
            new InventoryMovement { MovementID = 2, ProductID = 1, WarehouseID = 2, MovementType = "TRANSFER", Quantity = 15, DocumentNumber = "ПЕР-001", EmployeeID = 3, MovementDate = new DateTime(2026, 3, 2), Notes = "Перемещение в магазин" },
            new InventoryMovement { MovementID = 3, ProductID = 2, WarehouseID = 1, MovementType = "IN", Quantity = 100, UnitPrice = 800, DocumentNumber = "НАКЛ-002", EmployeeID = 3, MovementDate = new DateTime(2026, 3, 5), Notes = "Первичная приёмка" },
            new InventoryMovement { MovementID = 4, ProductID = 2, WarehouseID = 2, MovementType = "TRANSFER", Quantity = 30, DocumentNumber = "ПЕР-002", EmployeeID = 3, MovementDate = new DateTime(2026, 3, 6), Notes = "Перемещение в магазин" },
            new InventoryMovement { MovementID = 5, ProductID = 3, WarehouseID = 2, MovementType = "IN", Quantity = 40, UnitPrice = 400, DocumentNumber = "НАКЛ-003", EmployeeID = 3, MovementDate = new DateTime(2026, 3, 10), Notes = "Приёмка игрушек" },
            new InventoryMovement { MovementID = 6, ProductID = 4, WarehouseID = 1, MovementType = "IN", Quantity = 80, UnitPrice = 600, DocumentNumber = "НАКЛ-004", EmployeeID = 3, MovementDate = new DateTime(2026, 3, 15), Notes = "Приёмка влажных кормов" },
            new InventoryMovement { MovementID = 7, ProductID = 4, WarehouseID = 2, MovementType = "TRANSFER", Quantity = 25, DocumentNumber = "ПЕР-003", EmployeeID = 3, MovementDate = new DateTime(2026, 3, 16), Notes = "Перемещение в магазин" },
            new InventoryMovement { MovementID = 8, ProductID = 5, WarehouseID = 2, MovementType = "IN", Quantity = 10, UnitPrice = 2200, DocumentNumber = "НАКЛ-005", EmployeeID = 3, MovementDate = new DateTime(2026, 3, 20), Notes = "Приёмка аксессуаров" }
        );

        // === Задачи ===
        modelBuilder.Entity<ZooTask>().HasData(
            new ZooTask { TaskID = 1, TaskTitle = "Приёмка новой партии кормов", Description = "Принять и разместить на складе 200 единиц корма Royal Canin", AssignedTo = 1, CreatedBy = 3, Priority = "Высокий", Status = "Новая", DueDate = new DateTime(2026, 4, 10), CreatedAt = new DateTime(2026, 4, 1) },
            new ZooTask { TaskID = 2, TaskTitle = "Выкладка товаров в зале", Description = "Обновить экспозицию кормов и аксессуаров", AssignedTo = 2, CreatedBy = 3, Priority = "Средний", Status = "В работе", DueDate = new DateTime(2026, 4, 7), CreatedAt = new DateTime(2026, 4, 2) },
            new ZooTask { TaskID = 3, TaskTitle = "Инвентаризация склада", Description = "Провести полную инвентаризацию основного склада", AssignedTo = 3, CreatedBy = 3, Priority = "Высокий", Status = "Новая", DueDate = new DateTime(2026, 4, 15), CreatedAt = new DateTime(2026, 4, 3) },
            new ZooTask { TaskID = 4, TaskTitle = "Уборка в зале", Description = "Ежедневная уборка торгового зала", AssignedTo = 1, CreatedBy = 3, Priority = "Низкий", Status = "Выполнена", DueDate = new DateTime(2026, 4, 5), CompletedDate = new DateTime(2026, 4, 5, 18, 0, 0), CreatedAt = new DateTime(2026, 4, 5) }
        );

        // === Расписание смен ===
        var seedDate = new DateTime(2026, 4, 6); // Фиксированная дата для seed
        modelBuilder.Entity<Schedule>().HasData(
            new Schedule { ScheduleID = 1, EmployeeID = 1, WorkDate = seedDate, ShiftStart = new TimeSpan(9, 0, 0), ShiftEnd = new TimeSpan(18, 0, 0), ShiftType = "Утренняя", IsWorked = true },
            new Schedule { ScheduleID = 2, EmployeeID = 2, WorkDate = seedDate, ShiftStart = new TimeSpan(12, 0, 0), ShiftEnd = new TimeSpan(21, 0, 0), ShiftType = "Вечерняя", IsWorked = true },
            new Schedule { ScheduleID = 3, EmployeeID = 3, WorkDate = seedDate, ShiftStart = new TimeSpan(9, 0, 0), ShiftEnd = new TimeSpan(18, 0, 0), ShiftType = "Полный день", IsWorked = true },
            new Schedule { ScheduleID = 4, EmployeeID = 1, WorkDate = seedDate.AddDays(1), ShiftStart = new TimeSpan(9, 0, 0), ShiftEnd = new TimeSpan(18, 0, 0), ShiftType = "Утренняя", IsWorked = false },
            new Schedule { ScheduleID = 5, EmployeeID = 2, WorkDate = seedDate.AddDays(1), ShiftStart = new TimeSpan(9, 0, 0), ShiftEnd = new TimeSpan(18, 0, 0), ShiftType = "Полный день", IsWorked = false }
        );
    }

    public async Task<int> SaveChangesAsync()
    {
        return await SaveChangesAsync(CancellationToken.None);
    }
}
