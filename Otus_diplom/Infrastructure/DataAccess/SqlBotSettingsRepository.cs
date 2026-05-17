using LinqToDB;
using Otus_diplom.Core.DataAccess;
using Otus_diplom.Infrastructure.DataAccess.Models;

namespace Otus_diplom.Infrastructure.DataAccess;

/// <summary>
/// SQL-репозиторий настроек бота.
/// </summary>
public class SqlBotSettingsRepository : IBotSettingsRepository
{
    private readonly IDataContextFactory _dataContextFactory;

    /// <summary>
    /// Создает SQL-репозиторий настроек.
    /// </summary>
    public SqlBotSettingsRepository(IDataContextFactory dataContextFactory)
    {
        _dataContextFactory = dataContextFactory;
    }

    /// <summary>
    /// Возвращает все настройки бота.
    /// </summary>
    public Dictionary<string, string> GetAll()
    {
        using var db = _dataContextFactory.CreateDataContext();
        return db.BotSettings.ToDictionary(setting => setting.Key, setting => setting.Value);
    }

    /// <summary>
    /// Возвращает значение настройки по ключу.
    /// </summary>
    public string? GetValue(string key)
    {
        using var db = _dataContextFactory.CreateDataContext();
        return db.BotSettings.FirstOrDefault(setting => setting.Key == key)?.Value;
    }

    /// <summary>
    /// Сохраняет значение настройки.
    /// </summary>
    public void SetValue(string key, string value)
    {
        using var db = _dataContextFactory.CreateDataContext();
        var existingSetting = db.BotSettings.FirstOrDefault(setting => setting.Key == key);

        if (existingSetting is null)
        {
            db.Insert(new SqlBotSettingModel
            {
                Key = key,
                Value = value
            });
            return;
        }

        db.BotSettings
            .Where(setting => setting.Key == key)
            .Set(setting => setting.Value, value)
            .Update();
    }
}
