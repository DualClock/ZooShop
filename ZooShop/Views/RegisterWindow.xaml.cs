using System.Windows;
using Microsoft.EntityFrameworkCore;
using ZooShop.Data;
using ZooShop.Models;

namespace ZooShop.Views;

public partial class RegisterWindow : Window
{
    public RegisterWindow()
    {
        InitializeComponent();
    }

    private async void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        var lastName = TxtLastName.Text.Trim();
        var firstName = TxtFirstName.Text.Trim();
        var middleName = TxtMiddleName.Text.Trim();
        var phone = TxtPhone.Text.Trim();
        var email = TxtEmail.Text.Trim();
        var position = TxtPosition.Text.Trim();
        var password = TxtPassword.Password;
        var passwordConfirm = TxtPasswordConfirm.Password;

        if (string.IsNullOrEmpty(lastName))
        {
            ShowError("Введите фамилию");
            return;
        }

        if (string.IsNullOrEmpty(firstName))
        {
            ShowError("Введите имя");
            return;
        }

        if (string.IsNullOrEmpty(email))
        {
            ShowError("Введите email — это будет ваш логин");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowError("Введите пароль");
            return;
        }

        if (password != passwordConfirm)
        {
            ShowError("Пароли не совпадают");
            return;
        }

        try
        {
            await using var db = new ZooShopDbContext();

            var existsLogin = await db.Users.AnyAsync(u => u.Login == email);
            if (existsLogin)
            {
                ShowError("Пользователь с таким email уже существует");
                return;
            }

            var employeeRole = await db.UserRoles.FirstOrDefaultAsync(r => r.RoleName == "Сотрудник");
            if (employeeRole == null)
            {
                ShowError("Ошибка: роль 'Сотрудник' не найдена в БД");
                return;
            }

            var newEmployee = new Employee
            {
                FirstName = firstName,
                LastName = lastName,
                MiddleName = string.IsNullOrEmpty(middleName) ? null : middleName,
                Phone = string.IsNullOrEmpty(phone) ? null : phone,
                Email = email,
                Position = string.IsNullOrEmpty(position) ? null : position,
                Role = "Сотрудник",
                IsActive = true
            };

            db.Employees.Add(newEmployee);
            await db.SaveChangesAsync();

            var newUser = new User
            {
                Login = email,
                Password = password,
                RoleID = employeeRole.RoleID,
                EmployeeID = newEmployee.EmployeeID,
                IsActive = true
            };

            db.Users.Add(newUser);
            await db.SaveChangesAsync();

            MessageBox.Show(
                $"Аккаунт создан!\n\n{lastName} {firstName}\nЛогин: {email}\nРоль: Сотрудник\n\nТеперь войдите в систему.",
                "Регистрация успешна",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            RegisteredEmail = email;
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            ShowError($"Ошибка: {ex.Message}");
        }
    }

    private void ShowError(string message)
    {
        TxtError.Text = message;
        TxtError.Visibility = Visibility.Visible;
    }

    private void HyperlinkLogin_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    public string RegisteredEmail { get; private set; } = "";
}
