using ZooShop.Models;

namespace ZooShop;

/// <summary>
/// Статический сервис сессии — хранит текущего вошедшего пользователя.
/// </summary>
public static class SessionService
{
    public static User? CurrentUser { get; set; }

    public static bool IsAuthenticated => CurrentUser != null;

    public static bool IsAdmin => CurrentUser?.Role?.RoleName == "Админ";

    public static string DisplayName => CurrentUser?.Employee != null
        ? $"{CurrentUser.Employee.LastName} {CurrentUser.Employee.FirstName}"
        : CurrentUser?.Login ?? "";

    public static string RoleName => CurrentUser?.Role?.RoleName ?? "";

    public static void Logout()
    {
        CurrentUser = null;
    }
}
