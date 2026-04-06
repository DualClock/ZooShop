using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using ZooShop.Data;
using ZooShop.Models;

namespace ZooShop.ViewModels;

public class TasksScheduleViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    private string _statusMessage = "";
    private DateTime _selectedDate = DateTime.Today;

    // Форма задачи
    private string _taskTitle = "";
    private string _taskDescription = "";
    private int? _assignedToId;
    private string _taskPriority = "Средний";

    public ObservableCollection<ZooTask> Tasks { get; } = new();
    public ObservableCollection<Employee> Employees { get; } = new();
    public ObservableCollection<Schedule> Schedules { get; } = new();

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set { SetField(ref _selectedDate, value); _ = LoadSchedulesAsync(); }
    }

    public string TaskTitle
    {
        get => _taskTitle;
        set => SetField(ref _taskTitle, value);
    }

    public string TaskDescription
    {
        get => _taskDescription;
        set => SetField(ref _taskDescription, value);
    }

    public int? AssignedToId
    {
        get => _assignedToId;
        set => SetField(ref _assignedToId, value);
    }

    public string TaskPriority
    {
        get => _taskPriority;
        set => SetField(ref _taskPriority, value);
    }

    public ICommand LoadTasksCommand { get; }
    public ICommand LoadSchedulesCommand { get; }
    public ICommand CreateTaskCommand { get; }
    public ICommand CompleteTaskCommand { get; }
    public ICommand DeleteTaskCommand { get; }

    private ZooTask? _selectedTask;
    public ZooTask? SelectedTask
    {
        get => _selectedTask;
        set => SetField(ref _selectedTask, value);
    }

    public TasksScheduleViewModel(MainViewModel main)
    {
        _main = main;
        LoadTasksCommand = new RelayCommand(async _ => await LoadTasksAsync());
        LoadSchedulesCommand = new RelayCommand(async _ => await LoadSchedulesAsync());
        CreateTaskCommand = new RelayCommand(async _ => await CreateTaskAsync(), _ => !string.IsNullOrWhiteSpace(TaskTitle) && AssignedToId.HasValue);
        CompleteTaskCommand = new RelayCommand(async _ => await CompleteTaskAsync(), _ => SelectedTask != null && SelectedTask.Status != "Выполнена");
        DeleteTaskCommand = new RelayCommand(async _ => await DeleteTaskAsync(), _ => SelectedTask != null);
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            await using var db = new ZooShopDbContext();
            var employees = await db.Employees.Where(e => e.IsActive).ToListAsync();
            Employees.Clear();
            foreach (var e in employees) Employees.Add(e);

            await LoadTasksAsync();
            await LoadSchedulesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    private async Task LoadTasksAsync()
    {
        try
        {
            await using var db = new ZooShopDbContext();
            var tasks = await db.Tasks
                .Include(t => t.AssignedEmployee)
                .OrderByDescending(t => t.CreatedAt)
                .Take(50)
                .ToListAsync();

            Tasks.Clear();
            foreach (var t in tasks) Tasks.Add(t);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    private async Task LoadSchedulesAsync()
    {
        try
        {
            await using var db = new ZooShopDbContext();
            var schedules = await db.Schedules
                .Include(s => s.Employee)
                .Where(s => s.WorkDate == SelectedDate.Date)
                .ToListAsync();

            Schedules.Clear();
            foreach (var s in schedules) Schedules.Add(s);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    private async Task CreateTaskAsync()
    {
        try
        {
            await using var db = new ZooShopDbContext();
            var task = new ZooTask
            {
                TaskTitle = TaskTitle,
                Description = TaskDescription,
                AssignedTo = AssignedToId ?? 0,
                Priority = TaskPriority,
                Status = "Новая",
                CreatedAt = DateTime.Now
            };

            db.Tasks.Add(task);
            await db.SaveChangesAsync();

            StatusMessage = $"✅ Задача '{TaskTitle}' создана";
            TaskTitle = ""; TaskDescription = ""; AssignedToId = null; TaskPriority = "Средний";
            await LoadTasksAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
    }

    private async Task CompleteTaskAsync()
    {
        if (SelectedTask == null) return;
        try
        {
            await using var db = new ZooShopDbContext();
            var task = await db.Tasks.FindAsync(SelectedTask.TaskID);
            if (task != null)
            {
                task.Status = "Выполнена";
                task.CompletedDate = DateTime.Now;
                await db.SaveChangesAsync();
                StatusMessage = $"✅ Задача '{task.TaskTitle}' выполнена!";
            }
            await LoadTasksAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
    }

    private async Task DeleteTaskAsync()
    {
        if (SelectedTask == null) return;
        if (SelectedTask.Status == "Выполнена")
        {
            StatusMessage = "Нельзя удалить выполненную задачу";
            return;
        }
        try
        {
            await using var db = new ZooShopDbContext();
            db.Tasks.Remove(SelectedTask);
            await db.SaveChangesAsync();
            StatusMessage = "✅ Задача удалена";
            SelectedTask = null;
            await LoadTasksAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
    }
}
