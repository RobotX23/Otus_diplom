using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Otus_diplom.TelegramBot;

/// <summary>
/// Запускает получение сообщений от Telegram через polling.
/// </summary>
public class TelegramBotRunner
{
    private readonly ITelegramBotClient _botClient;
    private readonly UpdateHandler _updateHandler;

    /// <summary>
    /// Создает runner для Telegram-бота.
    /// </summary>
    public TelegramBotRunner(ITelegramBotClient botClient, UpdateHandler updateHandler)
    {
        _botClient = botClient;
        _updateHandler = updateHandler;
    }

    /// <summary>
    /// Запускает polling и ожидает остановки приложения.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message]
        };

        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken);

        var botInfo = await _botClient.GetMe(cancellationToken);
        Console.WriteLine($"Telegram-бот @{botInfo.Username} запущен.");
        Console.WriteLine("Нажмите Ctrl+C для остановки.");

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Telegram-бот остановлен.");
        }
    }

    /// <summary>
    /// Обрабатывает входящий update Telegram.
    /// </summary>
    private Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            var message = update.Message;
            if (message?.Text is null)
            {
                return Task.CompletedTask;
            }

            _updateHandler.HandleTextMessage(message.Chat.Id, message.Text);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Ошибка обработки Telegram update: {exception}");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Обрабатывает ошибку polling.
    /// </summary>
    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка Telegram polling: {exception}");
        return Task.CompletedTask;
    }
}
