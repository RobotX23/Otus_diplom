namespace Otus_diplom.Core.Entities;

/// <summary>
/// Пользователь Telegram-бота.
/// </summary>
public class User
{
    /// <summary>
    /// Уникальный номер пользователя.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор чата Telegram.
    /// </summary>
    public long? TelegramChatId { get; set; }

    /// <summary>
    /// Username пользователя в Telegram без символа @.
    /// </summary>
    public string TelegramUsername { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя, которое вводится в командах.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Роль пользователя: сотрудник или lead.
    /// </summary>
    public UserRole Role { get; set; }
}
