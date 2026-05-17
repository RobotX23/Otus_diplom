namespace Otus_diplom.TelegramBot;

using Telegram.Bot.Types.ReplyMarkups;

/// <summary>
/// Контракт отправки сообщений пользователю.
/// </summary>
public interface IMessageSender
{
    /// <summary>
    /// Отправляет сообщение пользователю.
    /// </summary>
    void SendMessage(long chatId, string text, ReplyMarkup? keyboard = null);
}
