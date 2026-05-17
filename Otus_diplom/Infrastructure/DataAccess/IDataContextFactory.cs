namespace Otus_diplom.Infrastructure.DataAccess;

/// <summary>
/// Контракт фабрики подключения к базе данных.
/// </summary>
public interface IDataContextFactory
{
    /// <summary>
    /// Возвращает строку подключения к базе данных.
    /// </summary>
    string GetConnectionString();

    /// <summary>
    /// Создает контекст подключения к базе данных.
    /// </summary>
    AppDataContext CreateDataContext();
}
