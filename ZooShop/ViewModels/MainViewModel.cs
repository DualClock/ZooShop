using System.Collections.ObjectModel;
using System.Windows.Input;
using ZooShop.ViewModels;
using ZooShop.Views;

namespace ZooShop.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase? _currentView;
    private string _currentTitle = "ZooShop — Панель управления";

    public ViewModelBase? CurrentView
    {
        get => _currentView;
        set => SetField(ref _currentView, value);
    }

    public string CurrentTitle
    {
        get => _currentTitle;
        set => SetField(ref _currentTitle, value);
    }

    public ObservableCollection<string> NavigationItems { get; } = new()
    {
        "Панель управления",
        "Каталог",
        "Продажи",
        "Склад",
        "Сотрудники",
        "Клиенты и питомцы",
        "Задачи и расписание",
        "Отчёты",
        "Поддержка"
    };

    public ICommand NavigateCommand { get; }

    // Общий словарь для обмена данными между ViewModel
    public Dictionary<string, object> SharedData { get; } = new();

    // Ссылка на DashboardViewModel для навигации из вложенных VM
    public DashboardViewModel DashboardVM { get; }
    public CatalogViewModel CatalogVM { get; }
    public SalesViewModel SalesVM { get; }
    public InventoryViewModel InventoryVM { get; }
    public EmployeesViewModel EmployeesVM { get; }
    public ClientsPetsViewModel ClientsPetsVM { get; }
    public TasksScheduleViewModel TasksScheduleVM { get; }
    public ReportsViewModel ReportsVM { get; }
    public SupportViewModel SupportVM { get; }

    public MainViewModel()
    {
        DashboardVM = new DashboardViewModel(this);
        CatalogVM = new CatalogViewModel(this);
        SalesVM = new SalesViewModel(this);
        InventoryVM = new InventoryViewModel(this);
        EmployeesVM = new EmployeesViewModel(this);
        ClientsPetsVM = new ClientsPetsViewModel(this);
        TasksScheduleVM = new TasksScheduleViewModel(this);
        ReportsVM = new ReportsViewModel(this);
        SupportVM = new SupportViewModel();

        NavigateCommand = new RelayCommand(param => Navigate(param as string ?? ""));
        Navigate("Панель управления");
    }

    private void Navigate(string screenName)
    {
        try
        {
            CurrentView = (ViewModelBase)(screenName switch
            {
                "Панель управления" => DashboardVM,
                "Каталог" => CatalogVM,
                "Продажи" => SalesVM,
                "Склад" => InventoryVM,
                "Сотрудники" => EmployeesVM,
                "Клиенты и питомцы" => ClientsPetsVM,
                "Задачи и расписание" => TasksScheduleVM,
                "Отчёты" => ReportsVM,
                "Поддержка" => SupportVM,
                _ => DashboardVM
            });

            CurrentTitle = screenName == "Панель управления"
                ? "ZooShop — Панель управления"
                : $"ZooShop — {screenName}";
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Ошибка навигации: {ex.GetType().Name}: {ex.Message}", "Ошибка",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}
