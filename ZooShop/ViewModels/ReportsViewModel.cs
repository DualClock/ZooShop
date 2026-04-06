using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using ZooShop.Data;

namespace ZooShop.ViewModels;

public class ReportsViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    private string _statusMessage = "";
    private DateTime _startDate = DateTime.Today.AddDays(-30);
    private DateTime _endDate = DateTime.Today;

    public ObservableCollection<ReportSaleItem> SalesReport { get; } = new();
    public ObservableCollection<ReportProductItem> ProductsReport { get; } = new();
    public ObservableCollection<ReportEmployeeItem> EmployeesReport { get; } = new();

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public DateTime StartDate
    {
        get => _startDate;
        set => SetField(ref _startDate, value);
    }

    public DateTime EndDate
    {
        get => _endDate;
        set => SetField(ref _endDate, value);
    }

    public ICommand GenerateSalesReportCommand { get; }
    public ICommand GenerateProductsReportCommand { get; }
    public ICommand GenerateEmployeesReportCommand { get; }
    public ICommand GenerateReportsCommand { get; }
    public ICommand ExportCsvCommand { get; }

    private string _activeReportType = "";

    public ReportsViewModel(MainViewModel main)
    {
        _main = main;
        GenerateSalesReportCommand = new RelayCommand(async _ => await GenerateSalesReportAsync());
        GenerateProductsReportCommand = new RelayCommand(async _ => await GenerateProductsReportAsync());
        GenerateEmployeesReportCommand = new RelayCommand(async _ => await GenerateEmployeesReportAsync());
        GenerateReportsCommand = new RelayCommand(async _ => await GenerateAllReportsAsync());
        ExportCsvCommand = new RelayCommand(async _ => await ExportCsvAsync());
    }

    private async Task GenerateAllReportsAsync()
    {
        await GenerateSalesReportAsync();
        await GenerateProductsReportAsync();
        await GenerateEmployeesReportAsync();
    }

    private async Task GenerateSalesReportAsync()
    {
        try
        {
            await using var db = new ZooShopDbContext();
            var sales = await db.Sales
                .Include(s => s.Client)
                .Include(s => s.Employee)
                .Include(s => s.Warehouse)
                .Where(s => s.SaleDate >= StartDate && s.SaleDate <= EndDate)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            SalesReport.Clear();
            foreach (var s in sales)
            {
                SalesReport.Add(new ReportSaleItem
                {
                    ReceiptNumber = s.ReceiptNumber,
                    SaleDate = s.SaleDate,
                    Client = s.Client != null ? $"{s.Client.LastName} {s.Client.FirstName}" : "Без клиента",
                    Employee = s.Employee != null ? $"{s.Employee.LastName} {s.Employee.FirstName}" : "Неизвестно",
                    Warehouse = s.Warehouse != null ? s.Warehouse.WarehouseName : "Неизвестно",
                    TotalAmount = s.TotalAmount,
                    DiscountAmount = s.DiscountAmount,
                    PaymentMethod = s.PaymentMethod ?? ""
                });
            }

            _activeReportType = "sales";
            StatusMessage = $"✅ Найдено продаж: {SalesReport.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
    }

    private async Task GenerateProductsReportAsync()
    {
        try
        {
            await using var db = new ZooShopDbContext();
            var products = await db.SaleItems
                .Include(si => si.Product)
                .Where(si => si.Sale.SaleDate >= StartDate && si.Sale.SaleDate <= EndDate)
                .GroupBy(si => new { si.ProductID, si.Product!.ProductName, si.Product.SKU })
                .Select(g => new
                {
                    ProductID = g.Key.ProductID,
                    ProductName = g.Key.ProductName,
                    SKU = g.Key.SKU ?? "",
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToListAsync();

            ProductsReport.Clear();
            foreach (var p in products)
            {
                ProductsReport.Add(new ReportProductItem
                {
                    ProductName = p.ProductName,
                    SKU = p.SKU,
                    TotalQuantity = p.TotalQuantity,
                    TotalRevenue = p.TotalRevenue
                });
            }

            _activeReportType = "products";
            StatusMessage = $"✅ Товаров продано: {ProductsReport.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
    }

    private async Task GenerateEmployeesReportAsync()
    {
        try
        {
            await using var db = new ZooShopDbContext();
            var employees = await db.Sales
                .Include(s => s.Employee)
                .Where(s => s.SaleDate >= StartDate && s.SaleDate <= EndDate)
                .GroupBy(s => new { s.EmployeeID, s.Employee!.FirstName, s.Employee.LastName })
                .Select(g => new
                {
                    EmployeeID = g.Key.EmployeeID,
                    FirstName = g.Key.FirstName,
                    LastName = g.Key.LastName,
                    TotalSales = g.Count(),
                    TotalRevenue = g.Sum(s => s.TotalAmount)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToListAsync();

            EmployeesReport.Clear();
            foreach (var e in employees)
            {
                EmployeesReport.Add(new ReportEmployeeItem
                {
                    EmployeeName = !string.IsNullOrEmpty(e.LastName)
                        ? $"{e.LastName} {e.FirstName}"
                        : $"Сотрудник #{e.EmployeeID}",
                    TotalSales = e.TotalSales,
                    TotalRevenue = e.TotalRevenue
                });
            }

            _activeReportType = "employees";
            StatusMessage = $"✅ Сотрудников: {EmployeesReport.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
    }

    private async Task ExportCsvAsync()
    {
        var dialog = new SaveFileDialog { Filter = "CSV файлы|*.csv", DefaultExt = "csv" };
        if (dialog.ShowDialog() != true) return;

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("\uFEFF"); // BOM для Excel

            if (_activeReportType == "sales")
            {
                sb.AppendLine("Чек;Дата;Клиент;Сотрудник;Склад;Сумма;Скидка;Оплата");
                foreach (var item in SalesReport)
                    sb.AppendLine($"{item.ReceiptNumber};{item.SaleDate:dd.MM.yyyy HH:mm};{item.Client};{item.Employee};{item.Warehouse};{item.TotalAmount};{item.DiscountAmount};{item.PaymentMethod}");
            }
            else if (_activeReportType == "products")
            {
                sb.AppendLine("Товар;Кол-во;Выручка");
                foreach (var item in ProductsReport)
                    sb.AppendLine($"{item.ProductName};{item.TotalQuantity};{item.TotalRevenue}");
            }
            else if (_activeReportType == "employees")
            {
                sb.AppendLine("Сотрудник;Кол-во продаж;Выручка");
                foreach (var item in EmployeesReport)
                    sb.AppendLine($"{item.EmployeeName};{item.TotalSales};{item.TotalRevenue}");
            }

            await File.WriteAllTextAsync(dialog.FileName, sb.ToString(), Encoding.UTF8);
            StatusMessage = $"✅ Экспорт завершён: {dialog.FileName}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка экспорта: {ex.Message}";
        }
    }
}

public class ReportSaleItem : ViewModelBase
{
    public string ReceiptNumber { get; set; } = "";
    public DateTime SaleDate { get; set; }
    public string Client { get; set; } = "";
    public string Employee { get; set; } = "";
    public string Warehouse { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string PaymentMethod { get; set; } = "";
}

public class ReportProductItem : ViewModelBase
{
    public string ProductName { get; set; } = "";
    public string SKU { get; set; } = "";
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class ReportEmployeeItem : ViewModelBase
{
    public string EmployeeName { get; set; } = "";
    public int TotalSales { get; set; }
    public decimal TotalRevenue { get; set; }
}
