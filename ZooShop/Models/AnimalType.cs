namespace ZooShop.Models;

public class AnimalType
{
    public int AnimalTypeID { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Pet> Pets { get; set; } = new List<Pet>();
}
