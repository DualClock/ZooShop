namespace ZooShop.Models;

public class Sale
{
    public int SaleID { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; } = DateTime.Now;
    public int? ClientID { get; set; }
    public int EmployeeID { get; set; }
    public int WarehouseID { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public string? PaymentMethod { get; set; }
    public bool IsPrinted { get; set; }
    public bool EmailSent { get; set; }

    public Client? Client { get; set; }
    public Employee Employee { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}
