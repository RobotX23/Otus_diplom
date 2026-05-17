using LinqToDB.Mapping;

namespace Otus_diplom.Infrastructure.DataAccess.Models;

/// <summary>
/// Модель настройки бота для таблицы базы данных.
/// </summary>
[Table(Name = "bot_settings")]
public class SqlBotSettingModel
{
    /// <summary>
    /// Номер настройки.
    /// </summary>
    [PrimaryKey]
    [Identity]
    [Column(Name = "id")]
    public int Id { get; set; }

    /// <summary>
    /// Ключ настройки.
    /// </summary>
    [Column(Name = "setting_key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Значение настройки.
    /// </summary>
    [Column(Name = "setting_value")]
    public string Value { get; set; } = string.Empty;
}
