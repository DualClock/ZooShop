using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using ZooShop.Data;
using ZooShop.Models;

namespace ZooShop.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private TodaySales? _todaySales;
    private string _notifications = "";
    private decimal _lowStockCount;
    private readonly MainViewModel _main;

    public TodaySales? TodaySales
    {
        get => _todaySales;
        set => SetField(ref _todaySales, value);
    }

    public string Notifications
    {
        get => _notifications;
        set => SetField(ref _notifications, value);
    }

    public decimal LowStockCount
    {
        get => _lowStockCount;
        set => SetField(ref _lowStockCount, value);
    }

    public ObservableCollection<Product> TopProducts { get; } = new();

    public ICommand NavigateCommand { get; }
    public ICommand RefreshCommand { get; }

    public DashboardViewModel(MainViewModel main)
    {
        _main = main;
        NavigateCommand = new RelayCommand(param => Navigate(param as string));
        RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            await using var db = new ZooShopDbContext();

            var sales = await db.TodaySales.FirstOrDefaultAsync();
            TodaySales = sales ?? new TodaySales { TotalSales = 0, TotalRevenue = 0, NetRevenue = 0, AvgCheck = 0 };

            var topProducts = await db.SaleItems
                .Include(si => si.Product)
                    .ThenInclude(p => p.Category)
                .Include(si => si.Product)
                    .ThenInclude(p => p.Brand)
                .Include(si => si.Product)
                    .ThenInclude(p => p.AnimalType)
                .OrderByDescending(si => si.Quantity)
                .Take(5)
                .Select(si => si.Product!)
                .ToListAsync();

            TopProducts.Clear();
            foreach (var p in topProducts)
                TopProducts.Add(p);

            if (TopProducts.Count == 0)
            {
                var products = await db.Products.Take(5).ToListAsync();
                foreach (var p in products)
                    TopProducts.Add(p);
            }

            var lowStock = await db.CurrentStocks
                .Where(cs => cs.CurrentQuantity > 0 && cs.CurrentQuantity <= 10)
                .ToListAsync();

            LowStockCount = lowStock.Count;

            var notifText = "";
            if (lowStock.Count > 0)
            {
                notifText += $"⚠️ {lowStock.Count} товар(ов) с остатком ≤ 10:\n";
                foreach (var item in lowStock.Take(3))
                    notifText += $"  • {item.ProductName} — {item.CurrentQuantity} шт. ({item.WarehouseName})\n";
            }

            var todaySchedules = await db.Schedules
                .Include(s => s.Employee)
                .Where(s => s.WorkDate == DateTime.Today)
                .ToListAsync();

            if (todaySchedules.Any())
            {
                notifText += $"\n👥 Сегодня в смене: {todaySchedules.Count} сотр.\n";
                foreach (var sch in todaySchedules)
                {
                    var empName = sch.Employee != null
                        ? $"{sch.Employee.LastName} {sch.Employee.FirstName}"
                        : "Сотрудник";
                    notifText += $"  • {empName} ({sch.ShiftType})\n";
                }
            }

            var pendingTasks = await db.Tasks
                .CountAsync(t => t.Status == "Новая" || t.Status == "В работе");

            if (pendingTasks > 0)
                notifText += $"\n📋 Ожидает задач: {pendingTasks}";

            Notifications = notifText;
        }
        catch (Exception ex)
        {
            Notifications = $"Ошибка загрузки: {ex.Message}";
            TodaySales = new TodaySales { TotalSales = 0, TotalRevenue = 0, NetRevenue = 0, AvgCheck = 0 };
        }
    }

    private void Navigate(string? screen)
    {
        if (!string.IsNullOrEmpty(screen))
            _main.NavigateCommand.Execute(screen);
    }
}
