namespace ZooShop.Models;

public class PriceHistory
{
    public int PriceID { get; set; }
    public int ProductID { get; set; }
    public decimal Price { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }

    public Product Product { get; set; } = null!;
}
