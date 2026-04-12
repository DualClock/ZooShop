namespace ZooShop.Models;

public class UserRole
{
    public int RoleID { get; set; }
    public string RoleName { get; set; } = string.Empty; // "Админ", "Сотрудник"
    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}
