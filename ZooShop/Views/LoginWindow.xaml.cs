using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using ZooShop.Data;
using ZooShop.Models;

namespace ZooShop.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private void TxtLogin_TextChanged(object sender, TextChangedEventArgs e)
    {
        HideError();
    }

    private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        HideError();
    }

    private void HideError()
    {
        TxtError.Visibility = Visibility.Collapsed;
    }

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        var login = TxtLogin.Text.Trim();
        var password = TxtPassword.Password;

        System.Diagnostics.Debug.WriteLine($"[LOGIN] Попытка входа: login='{login}', password='{password}'");

        if (string.IsNullOrWhiteSpace(login))
        {
            ShowError("Введите логин (email)");
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ShowError("Введите пароль");
            return;
        }

        try
        {
            await using var db = new ZooShopDbContext();

            // Покажем ВСЕ логины в БД для отладки
            var allUsers = await db.Users.ToListAsync();
            System.Diagnostics.Debug.WriteLine($"[LOGIN] Все пользователи в БД ({allUsers.Count}):");
            foreach (var u in allUsers)
            {
                System.Diagnostics.Debug.WriteLine($"  - Login='{u.Login}', Password='{u.Password}', IsActive={u.IsActive}, RoleID={u.RoleID}");
            }

            var user = await db.Users
                .Include(u => u.Role)
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u =>
                    u.IsActive &&
                    u.Login == login);

            System.Diagnostics.Debug.WriteLine($"[LOGIN] Найден пользователь: {user != null}");
            if (user != null)
            {
                System.Diagnostics.Debug.WriteLine($"  Login={user.Login}, IsActive={user.IsActive}, RoleID={user.RoleID}");
                System.Diagnostics.Debug.WriteLine($"  Role.RoleName={user.Role?.RoleName ?? "NULL"}");
                System.Diagnostics.Debug.WriteLine($"  Employee={user.Employee?.FirstName} {user.Employee?.LastName}");
            }

            if (user == null)
            {
                // Проверим есть ли вообще такой логин (даже неактивный)
                var anyUser = await db.Users.FirstOrDefaultAsync(u => u.Login == login);
                if (anyUser != null)
                {
                    ShowError($"Пользователь найден, но НЕАКТИВЕН (IsActive={anyUser.IsActive})");
                }
                else
                {
                    ShowError("Пользователь не найден");
                }
                return;
            }

            if (user.Password != password)
            {
                ShowError($"Неверный пароль (в БД: '{user.Password}', введено: '{password}')");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[LOGIN] Вход успешен! Role={user.Role?.RoleName}");
            SessionService.CurrentUser = user;
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LOGIN] ОШИБКА: {ex.Message}");
            ShowError($"Ошибка: {ex.Message}");
        }
    }

    private void ShowError(string message)
    {
        TxtError.Text = message;
        TxtError.Visibility = Visibility.Visible;
    }

    private void HyperlinkRegister_Click(object sender, RoutedEventArgs e)
    {
        Hide();
        var registerWindow = new RegisterWindow();
        var result = registerWindow.ShowDialog();
        
        // Если регистрация успешна, подставляем email в поле логина
        if (result == true && !string.IsNullOrEmpty(registerWindow.RegisteredEmail))
        {
            TxtLogin.Text = registerWindow.RegisteredEmail;
            TxtPassword.Focus();
        }
        
        Show();
        TxtPassword.Password = "";
    }
}
