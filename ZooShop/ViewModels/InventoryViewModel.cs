using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using ZooShop.Data;
using ZooShop.Models;

namespace ZooShop.ViewModels;

public class InventoryViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    private string _statusMessage = "";
    private int _selectedWarehouseId = 1;
    private InventoryMovement? _selectedMovement;

    // Для формы движения
    private Product? _selectedProduct;
    private string _movementType = "IN";
    private int _quantity = 1;
    private decimal _unitPrice;
    private string _documentNumber = "";
    private string _notes = "";

    public ObservableCollection<CurrentStock> Stocks { get; } = new();
    public ObservableCollection<InventoryMovement> Movements { get; } = new();
    public ObservableCollection<Warehouse> Warehouses { get; } = new();
    public ObservableCollection<Product> Products { get; } = new();

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public int SelectedWarehouseId
    {
        get => _selectedWarehouseId;
        set { SetField(ref _selectedWarehouseId, value); _ = LoadStocksAsync(); }
    }

    public InventoryMovement? SelectedMovement
    {
        get => _selectedMovement;
        set => SetField(ref _selectedMovement, value);
    }

    // Форма
    public Product? SelectedProductForm
    {
        get => _selectedProduct;
        set => SetField(ref _selectedProduct, value);
    }

    public string MovementType
    {
        get => _movementType;
        set => SetField(ref _movementType, value);
    }

    public int Quantity
    {
        get => _quantity;
        set => SetField(ref _quantity, value);
    }

    public decimal UnitPrice
    {
        get => _unitPrice;
        set => SetField(ref _unitPrice, value);
    }

    public string DocumentNumber
    {
        get => _documentNumber;
        set => SetField(ref _documentNumber, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetField(ref _notes, value);
    }

    public ICommand LoadStockCommand { get; }
    public ICommand LoadMovementsCommand { get; }
    public ICommand SubmitMovementCommand { get; }

    public InventoryViewModel(MainViewModel main)
    {
        _main = main;
        LoadStockCommand = new RelayCommand(async _ => await LoadStocksAsync());
        LoadMovementsCommand = new RelayCommand(async _ => await LoadMovementsAsync());
        SubmitMovementCommand = new RelayCommand(async _ => await SubmitMovementAsync(), _ => CanSubmit);
        _ = LoadDataAsync();
    }

    private bool CanSubmit => SelectedProductForm != null && Quantity > 0;

    private async Task LoadDataAsync()
    {
        try
        {
            await using var db = new ZooShopDbContext();
            var warehouses = await db.Warehouses.ToListAsync();
            var products = await db.Products.ToListAsync();
            Warehouses.Clear();
            foreach (var w in warehouses) Warehouses.Add(w);
            Products.Clear();
            foreach (var p in products) Products.Add(p);

            await LoadStocksAsync();
            await LoadMovementsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    private async Task LoadStocksAsync()
    {
        try
        {
            await using var db = new ZooShopDbContext();
            var stocks = await db.CurrentStocks
                .Where(cs => cs.WarehouseID == SelectedWarehouseId)
                .OrderBy(cs => cs.ProductName)
                .ToListAsync();

            Stocks.Clear();
            foreach (var s in stocks) Stocks.Add(s);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки остатков: {ex.Message}";
        }
    }

    private async Task LoadMovementsAsync()
    {
        try
        {
            await using var db = new ZooShopDbContext();
            var movements = await db.InventoryMovements
                .Include(m => m.Product)
                .Include(m => m.Warehouse)
                .Include(m => m.Employee)
                .OrderByDescending(m => m.MovementDate)
                .Take(50)
                .ToListAsync();

            Movements.Clear();
            foreach (var m in movements) Movements.Add(m);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    private async Task SubmitMovementAsync()
    {
        if (SelectedProductForm == null) return;

        try
        {
            await using var db = new ZooShopDbContext();
            var movement = new InventoryMovement
            {
                ProductID = SelectedProductForm.ProductID,
                WarehouseID = SelectedWarehouseId,
                MovementType = MovementType,
                Quantity = Quantity,
                UnitPrice = UnitPrice > 0 ? UnitPrice : null,
                DocumentNumber = string.IsNullOrWhiteSpace(DocumentNumber) ? null : DocumentNumber,
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes,
                MovementDate = DateTime.Now
            };

            db.InventoryMovements.Add(movement);
            await db.SaveChangesAsync();

            StatusMessage = $"✅ Движение оформлено: {MovementType} {Quantity} шт. — {SelectedProductForm.ProductName}";
            await LoadStocksAsync();
            await LoadMovementsAsync();

            // Сброс формы
            Quantity = 1;
            UnitPrice = 0;
            DocumentNumber = "";
            Notes = "";
            SelectedProductForm = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
