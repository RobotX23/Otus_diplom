namespace Otus_diplom.Core.DataAccess;

/// <summary>
/// Контракт репозитория настроек бота.
/// </summary>
public interface IBotSettingsRepository
{
    /// <summary>
    /// Возвращает все настройки бота.
    /// </summary>
    Dictionary<string, string> GetAll();

    /// <summary>
    /// Возвращает значение настройки.
    /// </summary>
    string? GetValue(string key);

    /// <summary>
    /// Сохраняет значение настройки.
    /// </summary>
    void SetValue(string key, string value);
}
