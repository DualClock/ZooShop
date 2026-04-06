namespace ZooShop.Models;

public class CurrentStock
{
    public int ProductID { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int WarehouseID { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int? CurrentQuantity { get; set; }
}
