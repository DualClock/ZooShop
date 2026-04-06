namespace ZooShop.Models;

public class Warehouse
{
    public int WarehouseID { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }

    public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public override string ToString() => WarehouseName;
}
