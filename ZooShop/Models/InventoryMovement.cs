namespace ZooShop.Models;

public class InventoryMovement
{
    public int MovementID { get; set; }
    public int ProductID { get; set; }
    public int WarehouseID { get; set; }
    public string MovementType { get; set; } = string.Empty; // IN, OUT, WRITE_OFF, TRANSFER
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? DocumentNumber { get; set; }
    public int? EmployeeID { get; set; }
    public DateTime MovementDate { get; set; } = DateTime.Now;
    public string? Notes { get; set; }

    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public Employee? Employee { get; set; }
}
