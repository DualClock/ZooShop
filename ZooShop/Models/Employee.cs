namespace ZooShop.Models;

public class Employee
{
    public int EmployeeID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Position { get; set; }
    public string Role { get; set; } = "Сотрудник"; // "Админ" или "Сотрудник"
    public string? PasswordHash { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<ZooTask> AssignedTasks { get; set; } = new List<ZooTask>();
    public ICollection<ZooTask> CreatedTasks { get; set; } = new List<ZooTask>();
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public override string ToString() => $"{LastName} {FirstName}";
}
