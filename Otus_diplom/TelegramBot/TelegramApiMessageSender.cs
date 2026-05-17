using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_diplom.TelegramBot;

/// <summary>
/// Отправка сообщений через Telegram.Bot.
/// </summary>
public class TelegramApiMessageSender : IMessageSender
{
    private readonly ITelegramBotClient _botClient;

    /// <summary>
    /// Создает отправитель сообщений Telegram.
    /// </summary>
    public TelegramApiMessageSender(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    /// <summary>
    /// Отправляет текстовое сообщение в Telegram-чат.
    /// </summary>
    public void SendMessage(long chatId, string text, ReplyMarkup? keyboard = null)
    {
        _botClient.SendMessage(chatId, text, replyMarkup: keyboard).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Изменяет ранее отправленное сообщение с inline-кнопками.
    /// </summary>
    public void EditMessage(long chatId, int messageId, string text, InlineKeyboardMarkup? keyboard = null)
    {
        _botClient.EditMessageText(chatId, messageId, text, replyMarkup: keyboard).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Отвечает на callback от inline-кнопки.
    /// </summary>
    public void AnswerCallback(string callbackQueryId)
    {
        _botClient.AnswerCallbackQuery(callbackQueryId).GetAwaiter().GetResult();
    }
}
