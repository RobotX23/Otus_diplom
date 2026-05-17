namespace Otus_diplom.Infrastructure.DataAccess;

/// <summary>
/// Простая фабрика подключения к базе данных.
/// </summary>
public class DataContextFactory : IDataContextFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// Создает фабрику с переданной строкой подключения.
    /// </summary>
    public DataContextFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Возвращает строку подключения к базе данных.
    /// </summary>
    public string GetConnectionString()
    {
        return _connectionString;
    }

    /// <summary>
    /// Создает новый контекст подключения к базе данных.
    /// </summary>
    public AppDataContext CreateDataContext()
    {
        return new AppDataContext(_connectionString);
    }
}
