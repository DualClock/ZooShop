namespace ZooShop.Models;

public class Product
{
    public int ProductID { get; set; }
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Composition { get; set; }
    public int? BrandID { get; set; }
    public int? CategoryID { get; set; }
    public int? AnimalTypeID { get; set; }
    public string? ImageUrl { get; set; }
    public string Unit { get; set; } = "шт";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Brand? Brand { get; set; }
    public Category? Category { get; set; }
    public AnimalType? AnimalType { get; set; }
    public ICollection<PriceHistory> PriceHistory { get; set; } = new List<PriceHistory>();
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}
