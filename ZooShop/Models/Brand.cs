namespace ZooShop.Models;

public class Brand
{
    public int BrandID { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? Website { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
