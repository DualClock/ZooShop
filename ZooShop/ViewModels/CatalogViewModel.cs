using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using ZooShop.Data;
using ZooShop.Models;

namespace ZooShop.ViewModels;

public class CatalogViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    private string _searchText = "";
    private int? _selectedBrandId;
    private int? _selectedCategoryId;
    private int? _selectedAnimalTypeId;
    private Product? _selectedProduct;
    private string _statusMessage = "";

    // Форма создания/редактирования товара
    private string _formSku = "";
    private string _formProductName = "";
    private string? _formDescription;
    private string? _formComposition;
    private int? _formBrandId;
    private int? _formCategoryId;
    private int? _formAnimalTypeId;
    private string _formUnit = "шт";
    private decimal _formPrice;
    private bool _isEditingMode;

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Brand> Brands { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<AnimalType> AnimalTypes { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set { SetField(ref _searchText, value); _ = LoadProductsAsync(); }
    }

    public int? SelectedBrandId
    {
        get => _selectedBrandId;
        set { SetField(ref _selectedBrandId, value); _ = LoadProductsAsync(); }
    }

    public int? SelectedCategoryId
    {
        get => _selectedCategoryId;
        set { SetField(ref _selectedCategoryId, value); _ = LoadProductsAsync(); }
    }

    public int? SelectedAnimalTypeId
    {
        get => _selectedAnimalTypeId;
        set { SetField(ref _selectedAnimalTypeId, value); _ = LoadProductsAsync(); }
    }

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (SetField(ref _selectedProduct, value))
            {
                if (value != null)
                {
                    PopulateFormForEdit(value);
                }
                else
                {
                    ClearProductForm();
                }
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public ProductStockInfo? SelectedProductStock { get; private set; }

    // Свойства формы товара
    public string FormSku
    {
        get => _formSku;
        set => SetField(ref _formSku, value);
    }

    public string FormProductName
    {
        get => _formProductName;
        set => SetField(ref _formProductName, value);
    }

    public string? FormDescription
    {
        get => _formDescription;
        set => SetField(ref _formDescription, value);
    }

    public string? FormComposition
    {
        get => _formComposition;
        set => SetField(ref _formComposition, value);
    }

    public int? FormBrandId
    {
        get => _formBrandId;
        set => SetField(ref _formBrandId, value);
    }

    public int? FormCategoryId
    {
        get => _formCategoryId;
        set => SetField(ref _formCategoryId, value);
    }

    public int? FormAnimalTypeId
    {
        get => _formAnimalTypeId;
        set => SetField(ref _formAnimalTypeId, value);
    }

    public string FormUnit
    {
        get => _formUnit;
        set => SetField(ref _formUnit, value);
    }

    public decimal FormPrice
    {
        get => _formPrice;
        set => SetField(ref _formPrice, value);
    }

    public bool IsEditingMode
    {
        get => _isEditingMode;
        set => SetField(ref _isEditingMode, value);
    }

    // Создание нового бренда
    private bool _showNewBrandForm;
    private string _newBrandName = "";
    private string? _newBrandCountry;
    public bool ShowNewBrandForm { get => _showNewBrandForm; set => SetField(ref _showNewBrandForm, value); }
    public string NewBrandName { get => _newBrandName; set => SetField(ref _newBrandName, value); }
    public string? NewBrandCountry { get => _newBrandCountry; set => SetField(ref _newBrandCountry, value); }

    // Создание нового вида животного
    private bool _showNewAnimalTypeForm;
    private string _newAnimalTypeName = "";
    private string? _newAnimalTypeDesc;
    public bool ShowNewAnimalTypeForm { get => _showNewAnimalTypeForm; set => SetField(ref _showNewAnimalTypeForm, value); }
    public string NewAnimalTypeName { get => _newAnimalTypeName; set => SetField(ref _newAnimalTypeName, value); }
    public string? NewAnimalTypeDesc { get => _newAnimalTypeDesc; set => SetField(ref _newAnimalTypeDesc, value); }

    // Создание новой категории
    private bool _showNewCategoryForm;
    private string _newCategoryName = "";
    public bool ShowNewCategoryForm { get => _showNewCategoryForm; set => SetField(ref _showNewCategoryForm, value); }
    public string NewCategoryName { get => _newCategoryName; set => SetField(ref _newCategoryName, value); }

    public ICommand SearchCommand { get; }
    public ICommand AddToCartCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand SaveProductCommand { get; }
    public ICommand DeleteProductCommand { get; }
    public ICommand ClearFormCommand { get; }
    public ICommand ShowNewBrandCommand { get; }
    public ICommand SaveNewBrandCommand { get; }
    public ICommand CancelNewBrandCommand { get; }
    public ICommand ShowNewAnimalTypeCommand { get; }
    public ICommand SaveNewAnimalTypeCommand { get; }
    public ICommand CancelNewAnimalTypeCommand { get; }
    public ICommand ShowNewCategoryCommand { get; }
    public ICommand SaveNewCategoryCommand { get; }
    public ICommand CancelNewCategoryCommand { get; }

    public CatalogViewModel(MainViewModel main)
    {
        _main = main;
        SearchCommand = new RelayCommand(async _ => await LoadProductsAsync());
        AddToCartCommand = new RelayCommand(_ => AddToCart(), _ => SelectedProduct != null);
        ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
        SaveProductCommand = new RelayCommand(async _ => await SaveProductAsync(), _ => CanSaveProduct());
        DeleteProductCommand = new RelayCommand(async _ => await DeleteProductAsync(), _ => SelectedProduct != null);
        ClearFormCommand = new RelayCommand(_ => ClearProductForm());
        ShowNewBrandCommand = new RelayCommand(_ => ShowNewBrandForm = true);
        SaveNewBrandCommand = new RelayCommand(async _ => await CreateNewBrandAsync());
        CancelNewBrandCommand = new RelayCommand(_ => { ShowNewBrandForm = false; NewBrandName = ""; NewBrandCountry = null; });
        ShowNewAnimalTypeCommand = new RelayCommand(_ => ShowNewAnimalTypeForm = true);
        SaveNewAnimalTypeCommand = new RelayCommand(async _ => await CreateNewAnimalTypeAsync());
        CancelNewAnimalTypeCommand = new RelayCommand(_ => { ShowNewAnimalTypeForm = false; NewAnimalTypeName = ""; NewAnimalTypeDesc = null; });
        ShowNewCategoryCommand = new RelayCommand(_ => ShowNewCategoryForm = true);
        SaveNewCategoryCommand = new RelayCommand(async _ => await CreateNewCategoryAsync());
        CancelNewCategoryCommand = new RelayCommand(_ => { ShowNewCategoryForm = false; NewCategoryName = ""; });
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            await using var db = new ZooShopDbContext();
            var brands = await db.Brands.ToListAsync();
            var categories = await db.Categories.ToListAsync();
            var animalTypes = await db.AnimalTypes.ToListAsync();

            Brands.Clear();
            foreach (var b in brands) Brands.Add(b);
            Categories.Clear();
            foreach (var c in categories) Categories.Add(c);
            AnimalTypes.Clear();
            foreach (var a in animalTypes) AnimalTypes.Add(a);

            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            await using var db = new ZooShopDbContext();
            var query = db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(p => p.ProductName.Contains(SearchText) || (p.SKU != null && p.SKU.Contains(SearchText)));

            if (SelectedBrandId.HasValue)
                query = query.Where(p => p.BrandID == SelectedBrandId.Value);

            if (SelectedCategoryId.HasValue)
                query = query.Where(p => p.CategoryID == SelectedCategoryId.Value);

            if (SelectedAnimalTypeId.HasValue)
                query = query.Where(p => p.AnimalTypeID == SelectedAnimalTypeId.Value);

            var products = await query
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.AnimalType)
                .ToListAsync();

            Products.Clear();
            foreach (var p in products) Products.Add(p);

            SelectedProductStock = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки товаров: {ex.Message}";
        }
    }

    public async void LoadProductStock()
    {
        if (SelectedProduct == null) return;
        try
        {
            await using var db = new ZooShopDbContext();
            var stocks = await db.CurrentStocks
                .Where(cs => cs.ProductID == SelectedProduct.ProductID)
                .ToListAsync();

            SelectedProductStock = new ProductStockInfo
            {
                ProductName = SelectedProduct.ProductName,
                Stocks = stocks
            };
            OnPropertyChanged(nameof(SelectedProductStock));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки остатков: {ex.Message}";
        }
    }

    public async void AddToCart()
    {
        if (SelectedProduct == null) return;
        try
        {
            await _main.SalesVM.AddToCartAsync(SelectedProduct);
            _main.NavigateCommand.Execute("Продажи");
            StatusMessage = $"✅ {SelectedProduct.ProductName} добавлен в чек";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка добавления: {ex.Message}";
        }
    }

    private void ClearFilters()
    {
        SearchText = "";
        SelectedBrandId = null;
        SelectedCategoryId = null;
        SelectedAnimalTypeId = null;
    }

    // === CRUD для товаров ===

    private bool CanSaveProduct()
    {
        return !string.IsNullOrWhiteSpace(FormProductName);
    }

    private void PopulateFormForEdit(Product product)
    {
        IsEditingMode = true;
        FormSku = product.SKU ?? "";
        FormProductName = product.ProductName;
        FormDescription = product.Description;
        FormComposition = product.Composition;
        FormBrandId = product.BrandID;
        FormCategoryId = product.CategoryID;
        FormAnimalTypeId = product.AnimalTypeID;
        FormUnit = product.Unit;
        // Загружаем текущую цену
        _ = LoadProductPrice(product.ProductID);
    }

    private async Task LoadProductPrice(int productId)
    {
        try
        {
            await using var db = new ZooShopDbContext();
            var priceEntry = await db.PriceHistory
                .Where(ph => ph.ProductID == productId && ph.EndDate == null)
                .OrderByDescending(ph => ph.EffectiveDate)
                .FirstOrDefaultAsync();
            FormPrice = priceEntry?.Price ?? 0;
        }
        catch
        {
            FormPrice = 0;
        }
    }

    private void ClearProductForm()
    {
        IsEditingMode = false;
        SelectedProduct = null;
        FormSku = "";
        FormProductName = "";
        FormDescription = null;
        FormComposition = null;
        FormBrandId = null;
        FormCategoryId = null;
        FormAnimalTypeId = null;
        FormUnit = "шт";
        FormPrice = 0;
    }

    private async Task SaveProductAsync()
    {
        if (!CanSaveProduct()) return;

        try
        {
            await using var db = new ZooShopDbContext();
            await using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                if (IsEditingMode && SelectedProduct != null)
                {
                    // Обновление существующего товара
                    var product = await db.Products.FindAsync(SelectedProduct.ProductID);
                    if (product == null)
                    {
                        StatusMessage = "❌ Товар не найден";
                        return;
                    }
                    product.SKU = FormSku;
                    product.ProductName = FormProductName;
                    product.Description = FormDescription;
                    product.Composition = FormComposition;
                    product.BrandID = FormBrandId;
                    product.CategoryID = FormCategoryId;
                    product.AnimalTypeID = FormAnimalTypeId;
                    product.Unit = FormUnit;

                    // Обновляем цену если она изменилась
                    if (FormPrice > 0)
                    {
                        var oldPrice = await db.PriceHistory
                            .Where(ph => ph.ProductID == product.ProductID && ph.EndDate == null)
                            .FirstOrDefaultAsync();
                        if (oldPrice != null && oldPrice.Price != FormPrice)
                        {
                            oldPrice.EndDate = DateTime.Now;
                            db.PriceHistory.Add(new PriceHistory
                            {
                                ProductID = product.ProductID,
                                Price = FormPrice,
                                EffectiveDate = DateTime.Now
                            });
                        }
                        else if (oldPrice == null)
                        {
                            db.PriceHistory.Add(new PriceHistory
                            {
                                ProductID = product.ProductID,
                                Price = FormPrice,
                                EffectiveDate = DateTime.Now
                            });
                        }
                    }

                    StatusMessage = $"✅ {FormProductName} обновлён";
                }
                else
                {
                    // Создание нового товара
                    var product = new Product
                    {
                        SKU = FormSku,
                        ProductName = FormProductName,
                        Description = FormDescription,
                        Composition = FormComposition,
                        BrandID = FormBrandId,
                        CategoryID = FormCategoryId,
                        AnimalTypeID = FormAnimalTypeId,
                        Unit = FormUnit,
                        CreatedAt = DateTime.Now
                    };
                    db.Products.Add(product);

                    // Сохраняем чтобы получить ProductID
                    await db.SaveChangesAsync();

                    // Создаём запись цены
                    if (FormPrice > 0)
                    {
                        db.PriceHistory.Add(new PriceHistory
                        {
                            ProductID = product.ProductID,
                            Price = FormPrice,
                            EffectiveDate = DateTime.Now
                        });
                    }

                    StatusMessage = $"✅ {FormProductName} добавлен";
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                await LoadProductsAsync();
                ClearProductForm();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка сохранения: {ex.Message}";
        }
    }

    private async Task DeleteProductAsync()
    {
        if (SelectedProduct == null) return;

        var result = System.Windows.MessageBox.Show(
            $"Удалить товар \"{SelectedProduct.ProductName}\"?",
            "Подтверждение", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

        if (result != System.Windows.MessageBoxResult.Yes) return;

        try
        {
            await using var db = new ZooShopDbContext();
            var product = await db.Products.FindAsync(SelectedProduct.ProductID);
            if (product == null)
            {
                StatusMessage = "❌ Товар не найден";
                return;
            }

            // Деактивируем вместо удаления
            product.IsActive = false;
            await db.SaveChangesAsync();
            StatusMessage = $"✅ {product.ProductName} деактивирован";
            await LoadProductsAsync();
            ClearProductForm();
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка удаления: {ex.Message}";
        }
    }

    // === Создание нового бренда ===
    private async Task CreateNewBrandAsync()
    {
        if (string.IsNullOrWhiteSpace(NewBrandName))
        {
            StatusMessage = "❌ Укажите название бренда";
            return;
        }

        try
        {
            await using var db = new ZooShopDbContext();
            var brand = new Brand
            {
                BrandName = NewBrandName,
                Country = NewBrandCountry,
                Website = ""
            };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();

            // Перезагружаем список
            Brands.Add(brand);
            FormBrandId = brand.BrandID;

            // Скрываем форму
            ShowNewBrandForm = false;
            NewBrandName = "";
            NewBrandCountry = null;

            StatusMessage = $"✅ Бренд \"{brand.BrandName}\" создан";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
    }

    // === Создание нового вида животного ===
    private async Task CreateNewAnimalTypeAsync()
    {
        if (string.IsNullOrWhiteSpace(NewAnimalTypeName))
        {
            StatusMessage = "❌ Укажите название вида";
            return;
        }

        try
        {
            await using var db = new ZooShopDbContext();
            var animalType = new AnimalType
            {
                TypeName = NewAnimalTypeName,
                Description = NewAnimalTypeDesc ?? ""
            };
            db.AnimalTypes.Add(animalType);
            await db.SaveChangesAsync();

            // Перезагружаем список
            AnimalTypes.Add(animalType);
            FormAnimalTypeId = animalType.AnimalTypeID;

            // Скрываем форму
            ShowNewAnimalTypeForm = false;
            NewAnimalTypeName = "";
            NewAnimalTypeDesc = null;

            StatusMessage = $"✅ Вид \"{animalType.TypeName}\" создан";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
    }

    // === Создание новой категории ===
    private async Task CreateNewCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName))
        {
            StatusMessage = "❌ Укажите название категории";
            return;
        }

        try
        {
            await using var db = new ZooShopDbContext();
            var category = new Category
            {
                CategoryName = NewCategoryName,
                ParentCategoryID = null
            };
            db.Categories.Add(category);
            await db.SaveChangesAsync();

            // Перезагружаем список
            Categories.Add(category);
            FormCategoryId = category.CategoryID;

            // Скрываем форму
            ShowNewCategoryForm = false;
            NewCategoryName = "";

            StatusMessage = $"✅ Категория \"{category.CategoryName}\" создана";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
    }
}

public class ProductStockInfo
{
    public string ProductName { get; set; } = "";
    public List<CurrentStock> Stocks { get; set; } = new();
}
