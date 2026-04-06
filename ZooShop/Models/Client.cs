namespace ZooShop.Models;

public class Client
{
    public int ClientID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? DiscountCardNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime RegistrationDate { get; set; } = DateTime.Now;

    public ICollection<Pet> Pets { get; set; } = new List<Pet>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public override string ToString() => $"{LastName} {FirstName} ({Phone})";
}
