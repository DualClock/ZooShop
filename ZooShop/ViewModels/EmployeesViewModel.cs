using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using ZooShop.Data;
using ZooShop.Models;

namespace ZooShop.ViewModels;

public class EmployeesViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    private Employee? _selectedEmployee;
    private string _statusMessage = "";
    private bool _showInactive = false;

    // Форма создания/редактирования
    private string _formFirstName = "";
    private string _formLastName = "";
    private string? _formMiddleName;
    private string? _formPhone;
    private string? _formEmail;
    private string? _formPosition;
    private string _formRole = "Сотрудник";
    private bool _formIsActive = true;
    private bool _isEditingMode;

    public ObservableCollection<Employee> Employees { get; } = new();

    public Employee? SelectedEmployee
    {
        get => _selectedEmployee;
        set
        {
            if (SetField(ref _selectedEmployee, value))
            {
                if (value != null) PopulateFormForEdit(value);
                else ClearForm();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public bool ShowInactive
    {
        get => _showInactive;
        set
        {
            if (SetField(ref _showInactive, value))
            {
                _ = LoadEmployeesAsync();
            }
        }
    }

    // Свойства формы
    public string FormFirstName
    {
        get => _formFirstName;
        set => SetField(ref _formFirstName, value);
    }

    public string FormLastName
    {
        get => _formLastName;
        set => SetField(ref _formLastName, value);
    }

    public string? FormMiddleName
    {
        get => _formMiddleName;
        set => SetField(ref _formMiddleName, value);
    }

    public string? FormPhone
    {
        get => _formPhone;
        set => SetField(ref _formPhone, value);
    }

    public string? FormEmail
    {
        get => _formEmail;
        set => SetField(ref _formEmail, value);
    }

    public string? FormPosition
    {
        get => _formPosition;
        set => SetField(ref _formPosition, value);
    }

    public string FormRole
    {
        get => _formRole;
        set => SetField(ref _formRole, value);
    }

    public bool FormIsActive
    {
        get => _formIsActive;
        set => SetField(ref _formIsActive, value);
    }

    public bool IsEditingMode
    {
        get => _isEditingMode;
        set => SetField(ref _isEditingMode, value);
    }

    public ICommand LoadEmployeesCommand { get; }
    public ICommand SaveEmployeeCommand { get; }
    public ICommand DeleteEmployeeCommand { get; }
    public ICommand ClearFormCommand { get; }

    public EmployeesViewModel(MainViewModel main)
    {
        _main = main;
        LoadEmployeesCommand = new RelayCommand(async _ => await LoadEmployeesAsync());
        SaveEmployeeCommand = new RelayCommand(async _ => await SaveEmployeeAsync(), _ => CanSaveEmployee());
        DeleteEmployeeCommand = new RelayCommand(async _ => await DeleteEmployeeAsync(), _ => SelectedEmployee != null);
        ClearFormCommand = new RelayCommand(_ => ClearForm());
        _ = LoadEmployeesAsync();
    }

    private bool CanSaveEmployee()
    {
        return !string.IsNullOrWhiteSpace(FormFirstName) &&
               !string.IsNullOrWhiteSpace(FormLastName);
    }

    private async Task LoadEmployeesAsync()
    {
        try
        {
            await using var db = new ZooShopDbContext();
            
            // По умолчанию показываем только активных сотрудников
            var query = db.Employees.AsQueryable();
            if (!ShowInactive)
            {
                query = query.Where(e => e.IsActive);
            }
            
            var employees = await query.OrderBy(e => e.LastName).ToListAsync();
            Employees.Clear();
            foreach (var e in employees) Employees.Add(e);
            
            var statusText = $"Загружено сотрудников: {employees.Count}";
            if (!ShowInactive)
            {
                var inactiveCount = await db.Employees.CountAsync(e => !e.IsActive);
                if (inactiveCount > 0)
                    statusText += $" (скрыто неактивных: {inactiveCount})";
            }
            StatusMessage = statusText;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
        }
    }

    private void PopulateFormForEdit(Employee emp)
    {
        IsEditingMode = true;
        FormFirstName = emp.FirstName;
        FormLastName = emp.LastName;
        FormMiddleName = emp.MiddleName;
        FormPhone = emp.Phone;
        FormEmail = emp.Email;
        FormPosition = emp.Position;
        FormRole = emp.Role;
        FormIsActive = emp.IsActive;
    }

    private void ClearForm()
    {
        IsEditingMode = false;
        SelectedEmployee = null;
        FormFirstName = "";
        FormLastName = "";
        FormMiddleName = null;
        FormPhone = null;
        FormEmail = null;
        FormPosition = null;
        FormRole = "Сотрудник";
        FormIsActive = true;
    }

    private async Task SaveEmployeeAsync()
    {
        if (!CanSaveEmployee()) return;

        try
        {
            await using var db = new ZooShopDbContext();

            if (IsEditingMode && SelectedEmployee != null)
            {
                // Обновление существующего сотрудника
                var emp = await db.Employees.FindAsync(SelectedEmployee.EmployeeID);
                if (emp == null)
                {
                    StatusMessage = "❌ Сотрудник не найден";
                    return;
                }
                emp.FirstName = FormFirstName;
                emp.LastName = FormLastName;
                emp.MiddleName = FormMiddleName;
                emp.Phone = FormPhone;
                emp.Email = FormEmail;
                emp.Position = FormPosition;
                emp.Role = FormRole;
                emp.IsActive = FormIsActive;

                // Если email изменился — обновляем логин в Users
                if (!string.IsNullOrWhiteSpace(FormEmail))
                {
                    var user = await db.Users.FirstOrDefaultAsync(u => u.EmployeeID == emp.EmployeeID);
                    if (user != null)
                        user.Login = FormEmail;
                }

                StatusMessage = $"✅ {FormLastName} {FormFirstName} обновлён";
            }
            else
            {
                // Создание нового сотрудника + аккаунта
                var emp = new Employee
                {
                    FirstName = FormFirstName,
                    LastName = FormLastName,
                    MiddleName = FormMiddleName,
                    Phone = FormPhone,
                    Email = FormEmail,
                    Position = FormPosition,
                    Role = FormRole,
                    IsActive = FormIsActive
                };
                db.Employees.Add(emp);
                await db.SaveChangesAsync(); // Получаем EmployeeID

                // Определяем роль
                var roleName = FormRole == "Админ" ? "Админ" : "Сотрудник";
                var userRole = await db.UserRoles.FirstOrDefaultAsync(r => r.RoleName == roleName);
                if (userRole == null)
                {
                    StatusMessage = "❌ Роль не найдена";
                    return;
                }

                // Создаём аккаунт
                var user = new User
                {
                    Login = string.IsNullOrWhiteSpace(FormEmail) ? $"user{emp.EmployeeID}" : FormEmail,
                    Password = "123", // Пароль по умолчанию
                    RoleID = userRole.RoleID,
                    EmployeeID = emp.EmployeeID,
                    IsActive = FormIsActive
                };
                db.Users.Add(user);

                StatusMessage = $"✅ {FormLastName} {FormFirstName} добавлен (пароль: 123)";
            }

            await db.SaveChangesAsync();
            await LoadEmployeesAsync();
            ClearForm();
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка сохранения: {ex.Message}";
        }
    }

    private async Task DeleteEmployeeAsync()
    {
        if (SelectedEmployee == null)
        {
            StatusMessage = "❌ Выберите сотрудника из списка";
            return;
        }

        var empName = $"{SelectedEmployee.LastName} {SelectedEmployee.FirstName}";
        var result = MessageBox.Show(
            $"Деактивировать сотрудника {empName}?\n\n" +
            $"Сотрудник не сможет войти в систему, но останется в базе для отчётности.",
            "Деактивация сотрудника", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await using var db = new ZooShopDbContext();
            var emp = await db.Employees.FindAsync(SelectedEmployee.EmployeeID);
            if (emp == null)
            {
                StatusMessage = "❌ Сотрудник не найден в БД";
                return;
            }

            var oldStatus = emp.IsActive ? "Активен" : "Неактивен";
            
            // Деактивируем сотрудника
            emp.IsActive = false;

            // Деактивируем аккаунт
            var user = await db.Users.FirstOrDefaultAsync(u => u.EmployeeID == emp.EmployeeID);
            if (user != null)
            {
                user.IsActive = false;
            }

            await db.SaveChangesAsync();
            
            StatusMessage = $"✅ {empName} деактивирован ({oldStatus} → Неактивен)";
            
            // Перезагружаем список — деактивированный сотрудник исчезнет
            await LoadEmployeesAsync();
            ClearForm();
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка деактивации: {ex.Message}";
        }
    }
}
