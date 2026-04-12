namespace ZooShop;

/// <summary>
/// Конфигурация подключения к БД.
/// Переключите UseLocalDb на false, чтобы работать с MSSQLSERVER.
/// </summary>
public static class DbConfig
{
    /// <summary>
    /// true — LocalDB, false — MSSQLSERVER на localhost
    /// </summary>
    public const bool UseLocalDb = true;

    public static string ServerName => UseLocalDb
        ? @"(localdb)\MSSQLLocalDB"
        : @".\MSSQLSERVER";

    public static string ConnectionString =>
        $"Data Source={ServerName};Initial Catalog=ZooShop;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Application Name=ZooShop;Command Timeout=30";

    public static string MasterConnectionString =>
        $"Data Source={ServerName};Initial Catalog=master;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";
}
