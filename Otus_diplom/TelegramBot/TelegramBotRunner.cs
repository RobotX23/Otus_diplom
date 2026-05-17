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
        await RegisterBotCommandsAsync();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery]
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
    /// Регистрирует список команд, который Telegram показывает пользователю как подсказки.
    /// </summary>
    private async Task RegisterBotCommandsAsync()
    {
        var commands = new[]
        {
            new BotCommand{ Command = "start", Description = "Авторизация"},
            new BotCommand{ Command = "help", Description = "Помощь"},
            new BotCommand{ Command = "info", Description = "О программе"},
            new BotCommand{ Command = "report", Description = "Отправить ежедневный отчет"},
            new BotCommand{ Command = "task", Description = "Добавить выполненную задачу"},
            new BotCommand{ Command = "block", Description = "Добавить проблему или блокер"},
            new BotCommand{ Command = "my_last_report", Description = "Показать последний отчет"},
            new BotCommand{ Command = "my_tasks", Description = "Показать свои задачи"},
            new BotCommand{ Command = "start_task", Description = "Перевести задачу в работу"},
            new BotCommand{ Command = "close_task", Description = "Закрыть задачу"},
            new BotCommand{ Command = "reports", Description = "Показать отчеты сотрудников"},
            new BotCommand{ Command = "employees", Description = "Показать сотрудников"},
            new BotCommand{ Command = "missing_reports", Description = "Кто не отправил отчет"},
            new BotCommand{ Command = "summary", Description = "Сводка по отчетам"},
            new BotCommand{ Command = "employee_report", Description = "Отчет сотрудника"},
            new BotCommand{ Command = "assign_task", Description = "Назначить задачу"},
            new BotCommand{ Command = "employee_tasks", Description = "Задачи сотрудника"},
            new BotCommand{ Command = "team_tasks", Description = "Задачи всей группы"},
            new BotCommand{ Command = "add_employee", Description = "Добавить сотрудника"},
            new BotCommand{ Command = "set_lead", Description = "Назначить lead"},
            new BotCommand{ Command = "remove_user", Description = "Удалить пользователя"},
            new BotCommand{ Command = "users", Description = "Показать пользователей"},
            new BotCommand{ Command = "settings", Description = "Показать настройки"},
            new BotCommand{ Command = "set_task_deadline_reminder", Description = "Настроить дедлайн"}
        };

        await _botClient.SetMyCommands(commands);
    }

    /// <summary>
    /// Обрабатывает входящий update Telegram.
    /// </summary>
    private Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.Message?.Text is not null)
            {
                _updateHandler.HandleTextMessage(update.Message.Chat.Id, update.Message.Text, update.Message.From?.Username);
                return Task.CompletedTask;
            }

            if (update.CallbackQuery?.Message is not null && update.CallbackQuery.Data is not null)
            {
                _updateHandler.HandleCallbackQuery(
                    update.CallbackQuery.Message.Chat.Id,
                    update.CallbackQuery.Message.MessageId,
                    update.CallbackQuery.Id,
                    update.CallbackQuery.Data,
                    update.CallbackQuery.From.Username);
                return Task.CompletedTask;
            }

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
