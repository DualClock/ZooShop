namespace ZooShop.Models;

public class ZooTask
{
    public int TaskID { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AssignedTo { get; set; }
    public int? CreatedBy { get; set; }
    public string Priority { get; set; } = "Средний";
    public string Status { get; set; } = "Новая";
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Employee AssignedEmployee { get; set; } = null!;
    public Employee? Creator { get; set; }
}
