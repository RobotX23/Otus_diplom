using LinqToDB.Mapping;

namespace Otus_diplom.Infrastructure.DataAccess.Models;

/// <summary>
/// Модель пользователя для будущей таблицы базы данных.
/// </summary>
[Table(Name = "users")]
public class SqlUserModel
{
    /// <summary>
    /// Номер пользователя в базе данных.
    /// </summary>
    [PrimaryKey]
    [Identity]
    [Column(Name = "id")]
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор чата Telegram.
    /// </summary>
    [Column(Name = "telegram_chat_id")]
    public long TelegramChatId { get; set; }

    /// <summary>
    /// Имя пользователя.
    /// </summary>
    [Column(Name = "full_name")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Роль пользователя.
    /// </summary>
    [Column(Name = "role")]
    public string Role { get; set; } = string.Empty;
}
