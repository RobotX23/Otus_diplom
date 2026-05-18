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
    public async Task SendMessageAsync(long chatId, string text, ReplyMarkup? keyboard = null)
    {
        await _botClient.SendMessage(chatId, text, replyMarkup: keyboard);
    }

    /// <summary>
    /// Изменяет ранее отправленное сообщение с inline-кнопками.
    /// </summary>
    public async Task EditMessageAsync(long chatId, int messageId, string text, InlineKeyboardMarkup? keyboard = null)
    {
        await _botClient.EditMessageText(chatId, messageId, text, replyMarkup: keyboard);
    }

    /// <summary>
    /// Отвечает на callback от inline-кнопки.
    /// </summary>
    public async Task AnswerCallbackAsync(string callbackQueryId)
    {
        await _botClient.AnswerCallbackQuery(callbackQueryId);
    }
}
