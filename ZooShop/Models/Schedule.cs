namespace ZooShop.Models;

public class Schedule
{
    public int ScheduleID { get; set; }
    public int EmployeeID { get; set; }
    public DateTime WorkDate { get; set; }
    public TimeSpan? ShiftStart { get; set; }
    public TimeSpan? ShiftEnd { get; set; }
    public string? ShiftType { get; set; }
    public bool IsWorked { get; set; }
    public string? Notes { get; set; }

    public Employee Employee { get; set; } = null!;
}
