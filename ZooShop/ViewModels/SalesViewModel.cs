using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using ZooShop.Data;
using ZooShop.Models;

namespace ZooShop.ViewModels;

public class SalesViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    private Client? _selectedClient;
    private Employee? _selectedEmployee;
    private Warehouse? _selectedWarehouse;
    private string _paymentMethod = "Наличные";
    private decimal _cartDiscount;
    private string _receiptText = "";
    private string _statusMessage = "";

    // Поиск товаров в окне продаж
    private string _productSearchText = "";
    private int? _productFilterBrandId;
    private int? _productFilterCategoryId;

    // Форма нового клиента
    private bool _showNewClientForm;
    private string _newClientFirstName = "";
    private string _newClientLastName = "";
    private string? _newClientPhone;
    private string? _newClientEmail;
    private string? _newClientNotes;

    public ObservableCollection<SaleCartItem> CartItems { get; } = new();
    public ObservableCollection<Client> Clients { get; } = new();
    public ObservableCollection<Employee> Employees { get; } = new();
    public ObservableCollection<Warehouse> Warehouses { get; } = new();

    // Товары для добавления в чек
    public ObservableCollection<Product> AvailableProducts { get; } = new();
    public ObservableCollection<Brand> Brands { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();

    public Client? SelectedClient
    {
        get => _selectedClient;
        set { SetField(ref _selectedClient, value); RecalculateTotals(); }
    }

    public Employee? SelectedEmployee
    {
        get => _selectedEmployee;
        set => SetField(ref _selectedEmployee, value);
    }

    public Warehouse? SelectedWarehouse
    {
        get => _selectedWarehouse;
        set => SetField(ref _selectedWarehouse, value);
    }

    public string PaymentMethod
    {
        get => _paymentMethod;
        set => SetField(ref _paymentMethod, value);
    }

    public decimal CartDiscount
    {
        get => _cartDiscount;
        set { SetField(ref _cartDiscount, value); RecalculateTotals(); }
    }

    public decimal Subtotal => CartItems.Sum(i => i.LineTotal);
    public decimal DiscountTotal => CartItems.Sum(i => i.Discount) + CartDiscount;
    public decimal GrandTotal => Subtotal - DiscountTotal;

    public string ReceiptText
    {
        get => _receiptText;
        set => SetField(ref _receiptText, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    // Свойства для поиска товаров
    public string ProductSearchText
    {
        get => _productSearchText;
        set { SetField(ref _productSearchText, value); _ = LoadAvailableProducts(); }
    }

    public int? ProductFilterBrandId
    {
        get => _productFilterBrandId;
        set { SetField(ref _productFilterBrandId, value); _ = LoadAvailableProducts(); }
    }

    public int? ProductFilterCategoryId
    {
        get => _productFilterCategoryId;
        set { SetField(ref _productFilterCategoryId, value); _ = LoadAvailableProducts(); }
    }

    // Свойства для формы нового клиента
    public bool ShowNewClientForm
    {
        get => _showNewClientForm;
        set => SetField(ref _showNewClientForm, value);
    }

    public string NewClientFirstName
    {
        get => _newClientFirstName;
        set => SetField(ref _newClientFirstName, value);
    }

    public string NewClientLastName
    {
        get => _newClientLastName;
        set => SetField(ref _newClientLastName, value);
    }

    public string? NewClientPhone
    {
        get => _newClientPhone;
        set => SetField(ref _newClientPhone, value);
    }

    public string? NewClientEmail
    {
        get => _newClientEmail;
        set => SetField(ref _newClientEmail, value);
    }

    public string? NewClientNotes
    {
        get => _newClientNotes;
        set => SetField(ref _newClientNotes, value);
    }

    public ICommand IncreaseQtyCommand { get; }
    public ICommand DecreaseQtyCommand { get; }
    public ICommand RemoveItemCommand { get; }
    public ICommand CheckoutCommand { get; }
    public ICommand ClearCartCommand { get; }
    public ICommand AddProductToCartCommand { get; }
    public ICommand CreateClientCommand { get; }
    public ICommand CancelNewClientCommand { get; }
    public ICommand ShowNewClientFormCommand { get; }

    public SalesViewModel(MainViewModel main)
    {
        _main = main;
        IncreaseQtyCommand = new RelayCommand(IncreaseQty);
        DecreaseQtyCommand = new RelayCommand(DecreaseQty);
        RemoveItemCommand = new RelayCommand(RemoveItem);
        CheckoutCommand = new RelayCommand(async _ => await CheckoutAsync(), _ => CanCheckout);
        ClearCartCommand = new RelayCommand(ClearCart);
        AddProductToCartCommand = new RelayCommand(async _ => await AddSelectedProductToCart(), _ => CanAddProductToCart());
        CreateClientCommand = new RelayCommand(async _ => await CreateNewClientAsync());
        CancelNewClientCommand = new RelayCommand(_ => CancelNewClientForm());
        ShowNewClientFormCommand = new RelayCommand(_ => ShowNewClientForm = true);
        _ = LoadDataAsync();
    }

    private bool CanAddProductToCart() => _selectedAvailableProduct != null;
    private Product? _selectedAvailableProduct;
    public Product? SelectedAvailableProduct
    {
        get => _selectedAvailableProduct;
        set
        {
            if (SetField(ref _selectedAvailableProduct, value))
            {
                ((RelayCommand)AddProductToCartCommand).OnCanExecuteChanged();
            }
        }
    }

    private bool CanCheckout => CartItems.Count > 0 && SelectedEmployee != null && SelectedWarehouse != null;

    private async Task LoadDataAsync()
    {
        try
        {
            await using var db = new ZooShopDbContext();
            var clients = await db.Clients.ToListAsync();
            var employees = await db.Employees.Where(e => e.IsActive).ToListAsync();
            var warehouses = await db.Warehouses.ToListAsync();
            var brands = await db.Brands.ToListAsync();
            var categories = await db.Categories.ToListAsync();

            Clients.Clear();
            foreach (var c in clients) Clients.Add(c);
            Employees.Clear();
            foreach (var e in employees) Employees.Add(e);
            Warehouses.Clear();
            foreach (var w in warehouses) Warehouses.Add(w);
            Brands.Clear();
            foreach (var b in brands) Brands.Add(b);
            Categories.Clear();
            foreach (var c in categories) Categories.Add(c);

            if (Employees.Count > 0) SelectedEmployee = Employees[0];
            if (Warehouses.Count > 0) SelectedWarehouse = Warehouses[0];

            // Загружаем доступные товары
            await LoadAvailableProducts();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    private async Task LoadAvailableProducts()
    {
        try
        {
            await using var db = new ZooShopDbContext();
            var query = db.Products.Where(p => p.IsActive).AsQueryable();

            if (!string.IsNullOrWhiteSpace(ProductSearchText))
                query = query.Where(p => p.ProductName.Contains(ProductSearchText) || (p.SKU != null && p.SKU.Contains(ProductSearchText)));

            if (ProductFilterBrandId.HasValue)
                query = query.Where(p => p.BrandID == ProductFilterBrandId.Value);

            if (ProductFilterCategoryId.HasValue)
                query = query.Where(p => p.CategoryID == ProductFilterCategoryId.Value);

            var products = await query
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Take(100)
                .ToListAsync();

            AvailableProducts.Clear();
            foreach (var p in products) AvailableProducts.Add(p);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки товаров: {ex.Message}";
        }
    }

    public async Task AddToCartAsync(Product product)
    {
        try
        {
            var existing = CartItems.FirstOrDefault(i => i.Product.ProductID == product.ProductID);
            if (existing != null)
            {
                existing.Quantity++;
                RecalculateTotals();
            }
            else
            {
                decimal price = await GetCurrentPriceAsync(product.ProductID);
                CartItems.Add(new SaleCartItem(product, price));
            }
            RecalculateTotals();
            OnPropertyChanged(nameof(CanCheckout));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка добавления товара: {ex.Message}";
        }
    }

    public void AddToCart(Product product)
    {
        // Синхронная обёртка для вызова из синхронного кода
        _ = AddToCartAsync(product);
    }

    private async Task<decimal> GetCurrentPriceAsync(int productId)
    {
        try
        {
            await using var db = new ZooShopDbContext();
            var priceEntry = await db.PriceHistory
                .Where(ph => ph.ProductID == productId && ph.EndDate == null)
                .OrderByDescending(ph => ph.EffectiveDate)
                .FirstOrDefaultAsync();

            return priceEntry?.Price ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private void IncreaseQty(object? param)
    {
        if (param is not SaleCartItem item) return;
        item.Quantity++;
        RecalculateTotals();
    }

    private void DecreaseQty(object? param)
    {
        if (param is not SaleCartItem item || item.Quantity <= 1) return;
        item.Quantity--;
        RecalculateTotals();
    }

    private void RemoveItem(object? param)
    {
        if (param is not SaleCartItem item) return;
        CartItems.Remove(item);
        RecalculateTotals();
        OnPropertyChanged(nameof(CanCheckout));
    }

    private void RecalculateTotals()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(DiscountTotal));
        OnPropertyChanged(nameof(GrandTotal));
    }

    private async Task CheckoutAsync()
    {
        if (!CanCheckout) return;
        if (SelectedEmployee == null || SelectedWarehouse == null) return;

        try
        {
            await using var db = new ZooShopDbContext();
            await using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                // Генерация номера чека
                var receiptNumber = $"CH-{DateTime.Now:yyyyMMddHHmmss}";

                var sale = new Sale
                {
                    ReceiptNumber = receiptNumber,
                    SaleDate = DateTime.Now,
                    ClientID = SelectedClient?.ClientID,
                    EmployeeID = SelectedEmployee.EmployeeID,
                    WarehouseID = SelectedWarehouse.WarehouseID,
                    TotalAmount = GrandTotal,
                    DiscountAmount = CartDiscount,
                    PaymentMethod = PaymentMethod
                };

                db.Sales.Add(sale);

                foreach (var item in CartItems)
                {
                    var saleItem = new SaleItem
                    {
                        Sale = sale,
                        ProductID = item.Product.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Discount = item.Discount,
                        TotalPrice = item.LineTotal - item.Discount
                    };
                    db.SaleItems.Add(saleItem);

                    // Списываем остатки
                    var movement = new InventoryMovement
                    {
                        ProductID = item.Product.ProductID,
                        WarehouseID = SelectedWarehouse.WarehouseID,
                        MovementType = "OUT",
                        Quantity = item.Quantity,
                        EmployeeID = SelectedEmployee.EmployeeID,
                        MovementDate = DateTime.Now,
                        Notes = $"Продажа {receiptNumber}"
                    };
                    db.InventoryMovements.Add(movement);
                }

                // Один SaveChangesAsync для всех изменений
                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                // Генерация текста чека
                var sb = new StringBuilder();
                sb.AppendLine("═══════════════════════════");
                sb.AppendLine("       Z O O S H O P");
                sb.AppendLine("═══════════════════════════");
                sb.AppendLine($"Чек: {receiptNumber}");
                sb.AppendLine($"Дата: {sale.SaleDate:dd.MM.yyyy HH:mm}");
                sb.AppendLine($"Кассир: {SelectedEmployee.LastName} {SelectedEmployee.FirstName}");
                sb.AppendLine($"Склад: {SelectedWarehouse.WarehouseName}");
                if (SelectedClient != null)
                    sb.AppendLine($"Клиент: {SelectedClient.LastName} {SelectedClient.FirstName}");
                sb.AppendLine("───────────────────────────");

                foreach (var item in CartItems)
                {
                    sb.AppendLine($"{item.Product.ProductName}");
                    sb.AppendLine($"  {item.Quantity} x {item.UnitPrice:N2} ₽ = {item.LineTotal:N2} ₽");
                    if (item.Discount > 0)
                        sb.AppendLine($"  Скидка: -{item.Discount:N2} ₽");
                }

                sb.AppendLine("───────────────────────────");
                sb.AppendLine($"Подитог: {Subtotal:N2} ₽");
                if (CartDiscount > 0)
                    sb.AppendLine($"Скидка на чек: -{CartDiscount:N2} ₽");
                sb.AppendLine($"ИТОГО: {GrandTotal:N2} ₽");
                sb.AppendLine($"Оплата: {PaymentMethod}");
                sb.AppendLine("═══════════════════════════");
                sb.AppendLine("  Спасибо за покупку! 🐾");
                sb.AppendLine("═══════════════════════════");

                ReceiptText = sb.ToString();
                StatusMessage = $"✅ Продажа оформлена! Чек {receiptNumber}";

                // Очистка корзины
                CartItems.Clear();
                RecalculateTotals();
                OnPropertyChanged(nameof(CanCheckout));

                // Обновляем Dashboard чтобы обновилась выручка
                _main.DashboardVM.RefreshCommand.Execute(null);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
            MessageBox.Show($"Ошибка при оформлении продажи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ClearCart()
    {
        CartItems.Clear();
        CartDiscount = 0;
        ReceiptText = "";
        StatusMessage = "";
        RecalculateTotals();
        OnPropertyChanged(nameof(CanCheckout));
    }

    // === Добавление выбранного товара в чек ===
    private async Task AddSelectedProductToCart()
    {
        if (SelectedAvailableProduct == null) return;
        await AddToCartAsync(SelectedAvailableProduct);
        SelectedAvailableProduct = null;
    }

    // === Создание нового клиента ===
    private async Task CreateNewClientAsync()
    {
        if (string.IsNullOrWhiteSpace(NewClientFirstName) || string.IsNullOrWhiteSpace(NewClientLastName))
        {
            StatusMessage = "❌ Укажите имя и фамилию клиента";
            return;
        }

        try
        {
            await using var db = new ZooShopDbContext();
            var client = new Client
            {
                FirstName = NewClientFirstName ?? "",
                LastName = NewClientLastName ?? "",
                Phone = NewClientPhone ?? "",
                Email = NewClientEmail,
                Notes = NewClientNotes,
                RegistrationDate = DateTime.Now
            };
            db.Clients.Add(client);
            await db.SaveChangesAsync();

            // Перезагружаем список клиентов
            Clients.Clear();
            var clients = await db.Clients.ToListAsync();
            foreach (var c in clients) Clients.Add(c);

            // Выбираем нового клиента
            SelectedClient = client;

            // Скрываем форму
            ShowNewClientForm = false;
            ClearNewClientForm();

            StatusMessage = $"✅ Клиент {client.LastName} {client.FirstName} создан и выбран";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка создания клиента: {ex.Message}";
        }
    }

    private void CancelNewClientForm()
    {
        ShowNewClientForm = false;
        ClearNewClientForm();
    }

    private void ClearNewClientForm()
    {
        NewClientFirstName = "";
        NewClientLastName = "";
        NewClientPhone = null;
        NewClientEmail = null;
        NewClientNotes = null;
    }
}

// Обёртка для строки корзины
public class SaleCartItem : ViewModelBase
{
    public Product Product { get; }
    private int _quantity = 1;
    private decimal _discount;
    private decimal _unitPrice;

    public SaleCartItem(Product product, decimal price)
    {
        Product = product;
        _unitPrice = price;
        LineTotal = _quantity * _unitPrice;
    }

    public int Quantity
    {
        get => _quantity;
        set
        {
            SetField(ref _quantity, value);
            LineTotal = _quantity * _unitPrice;
            NotifyLineTotalChanged();
        }
    }

    public decimal UnitPrice
    {
        get => _unitPrice;
        set { SetField(ref _unitPrice, value); LineTotal = _quantity * _unitPrice; OnPropertyChanged(nameof(LineTotal)); }
    }

    public decimal LineTotal { get; private set; }

    public decimal Discount
    {
        get => _discount;
        set => SetField(ref _discount, value);
    }

    public void NotifyLineTotalChanged()
    {
        OnPropertyChanged(nameof(LineTotal));
    }
}
