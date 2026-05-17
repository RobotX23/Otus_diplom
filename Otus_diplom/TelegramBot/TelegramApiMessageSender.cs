using Telegram.Bot;

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
    public void SendMessage(long chatId, string text)
    {
        _botClient.SendMessage(chatId, text).GetAwaiter().GetResult();
    }
}
