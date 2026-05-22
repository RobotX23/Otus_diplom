using Otus_diplom.Core.DataAccess;
using Otus_diplom.Core.Services;
using Otus_diplom.Infrastructure.DataAccess;
using Otus_diplom.Infrastructure.DataAccess.Repositories;
using Otus_diplom.TelegramBot;
using Otus_diplom.TelegramBot.Messaging;
using Otus_diplom.TelegramBot.Scenarios;
using Otus_diplom.TelegramBot.Workers;
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
    IBotSettingsRepository botSettingsRepository = new SqlBotSettingsRepository(dataContextFactory);

    var reportService = new ReportService(reportRepository, maxCompletedTaskLength);
    var taskService = new TaskService(taskRepository, maxTaskTitleLength);
    IUserService userService = new UserService(userRepository);
    IBotSettingsService botSettingsService = new BotSettingsService(botSettingsRepository);
    IMessageSender messageSender = new TelegramApiMessageSender(botClient);
    IScenarioContextRepository scenarioContextRepository = new InMemoryScenarioContextRepository();
    var scenarios = new List<IScenario>
    {
        new AddEmployeeScenario(userService, scenarioContextRepository),
        new AssignLeadScenario(userService, scenarioContextRepository),
        new DeleteEmployeeScenario(userService, scenarioContextRepository),
        new TaskDeadlineReminderScenario(botSettingsService, scenarioContextRepository),
        new DailyReportReminderScenario(botSettingsService, scenarioContextRepository),
        new AddCompletedTaskScenario(reportService, scenarioContextRepository),
        new AddBlockScenario(reportService, scenarioContextRepository),
        new SendReportScenario(reportService, scenarioContextRepository),
        new MyTasksScenario(taskService, scenarioContextRepository),
        new LastReportScenario(reportService, scenarioContextRepository),
        new LeadEmployeeTasksScenario(taskService, userRepository, scenarioContextRepository),
        new LeadEmployeeReportScenario(reportService, userRepository, scenarioContextRepository),
        new LeadAssignTaskScenario(taskService, userRepository, messageSender, scenarioContextRepository)
    };

    var updateHandler = new UpdateHandler(
        reportService,
        taskService,
        userService,
        botSettingsService,
        userRepository,
        botSettingsRepository,
        messageSender,
        scenarioContextRepository,
        scenarios);
    var taskDeadlineReminderWorker = new TaskDeadlineReminderWorker(
        taskService,
        userRepository,
        botSettingsRepository,
        messageSender);
    var dailyReportReminderWorker = new DailyReportReminderWorker(
        reportService,
        userRepository,
        botSettingsRepository,
        messageSender);
    var botRunner = new TelegramBotRunner(botClient, updateHandler);

    using var cancellationTokenSource = new CancellationTokenSource();
    Console.CancelKeyPress += (_, eventArgs) =>
    {
        eventArgs.Cancel = true;
        cancellationTokenSource.Cancel();
    };

    var taskDeadlineReminderWorkerTask = taskDeadlineReminderWorker.RunAsync(cancellationTokenSource.Token);
    var dailyReportReminderWorkerTask = dailyReportReminderWorker.RunAsync(cancellationTokenSource.Token);
    await botRunner.RunAsync(cancellationTokenSource.Token);
    await taskDeadlineReminderWorkerTask;
    await dailyReportReminderWorkerTask;
}
catch (Exception exception)
{
    Console.WriteLine($"Критическая ошибка приложения: {exception}");
}
