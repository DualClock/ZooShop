namespace ZooShop.Models;

public class User
{
    public int UserID { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int RoleID { get; set; }
    public int? EmployeeID { get; set; }
    public bool IsActive { get; set; } = true;

    // Навигация
    public UserRole? Role { get; set; }
    public Employee? Employee { get; set; }
}
