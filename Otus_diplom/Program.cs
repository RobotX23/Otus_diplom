using Otus_diplom.Core.DataAccess;
using Otus_diplom.Core.Services;
using Otus_diplom.Infrastructure.DataAccess;
using Otus_diplom.TelegramBot;
using Telegram.Bot;

try
{
    var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? string.Empty;
    var telegramToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ?? string.Empty;
    var maxTaskTitleLength = 200;
    var maxCompletedTaskLength = 200;

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        Console.WriteLine("Не задана переменная окружения DB_CONNECTION_STRING.");
        return;
    }

    if (string.IsNullOrWhiteSpace(telegramToken))
    {
        Console.WriteLine("Не задана переменная окружения TELEGRAM_BOT_TOKEN.");
        return;
    }

    var dataContextFactory = new DataContextFactory(connectionString);
    var botClient = new TelegramBotClient(telegramToken);

    IUserRepository userRepository = new SqlUserRepository(dataContextFactory);
    IReportRepository reportRepository = new SqlReportRepository(dataContextFactory);
    ITaskRepository taskRepository = new SqlTaskRepository(dataContextFactory);

    var reportService = new ReportService(reportRepository, maxCompletedTaskLength);
    var taskService = new TaskService(taskRepository, maxTaskTitleLength);
    IMessageSender messageSender = new TelegramApiMessageSender(botClient);

    var updateHandler = new UpdateHandler(reportService, taskService, userRepository, messageSender);
    var botRunner = new TelegramBotRunner(botClient, updateHandler);

    using var cancellationTokenSource = new CancellationTokenSource();
    Console.CancelKeyPress += (_, eventArgs) =>
    {
        eventArgs.Cancel = true;
        cancellationTokenSource.Cancel();
    };

    await botRunner.RunAsync(cancellationTokenSource.Token);
}
catch (Exception exception)
{
    Console.WriteLine($"Критическая ошибка приложения: {exception}");
}
