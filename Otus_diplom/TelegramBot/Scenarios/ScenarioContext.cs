namespace Otus_diplom.TelegramBot.Scenarios;

/// <summary>
/// Контекст текущего многошагового сценария пользователя.
/// </summary>
public class ScenarioContext
{
    /// <summary>
    /// Chat id пользователя Telegram.
    /// </summary>
    public long ChatId { get; set; }

    /// <summary>
    /// Номер пользователя в системе.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Тип запущенного сценария.
    /// </summary>
    public ScenarioType ScenarioType { get; set; }

    /// <summary>
    /// Текущий шаг сценария.
    /// </summary>
    public string Step { get; set; } = string.Empty;

    /// <summary>
    /// Временные данные сценария.
    /// </summary>
    public Dictionary<string, string> Data { get; set; } = new();

    /// <summary>
    /// Время создания сценария.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
