namespace Otus_diplom.TelegramBot.Messaging;

using Telegram.Bot.Types.ReplyMarkups;

/// <summary>
/// Контракт отправки сообщений пользователю.
/// </summary>
public interface IMessageSender
{
    /// <summary>
    /// Отправляет сообщение пользователю.
    /// </summary>
    Task SendMessageAsync(long chatId, string text, ReplyMarkup? keyboard = null);

    /// <summary>
    /// Изменяет ранее отправленное сообщение с inline-кнопками.
    /// </summary>
    Task EditMessageAsync(long chatId, int messageId, string text, InlineKeyboardMarkup? keyboard = null);

    /// <summary>
    /// Отвечает на callback от inline-кнопки.
    /// </summary>
    Task AnswerCallbackAsync(string callbackQueryId);
}
