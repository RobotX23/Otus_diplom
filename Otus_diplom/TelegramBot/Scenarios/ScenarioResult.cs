using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_diplom.TelegramBot.Scenarios;

/// <summary>
/// Результат обработки шага сценария.
/// </summary>
public class ScenarioResult
{
    /// <summary>
    /// Текст сообщения для пользователя.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Клавиатура, которую нужно показать пользователю.
    /// </summary>
    public ReplyMarkup? Keyboard { get; set; }

    /// <summary>
    /// Показывает, что сообщение нужно изменить, а не отправлять новое.
    /// </summary>
    public bool EditCurrentMessage { get; set; }

    /// <summary>
    /// Показывает, что после callback нужно отправить новое сообщение.
    /// </summary>
    public bool SendNewMessage { get; set; }
}
