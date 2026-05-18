using Otus_diplom.Core.DataAccess;
using Otus_diplom.Core.Services;

namespace Otus_diplom.TelegramBot;

/// <summary>
/// Фоновая отправка напоминаний сотрудникам о ежедневном отчете.
/// </summary>
public class DailyReportReminderWorker
{
    private const string DailyReportReminderTimeKey = "daily_report_reminder_time";

    private readonly IReportService _reportService;
    private readonly IUserRepository _userRepository;
    private readonly IBotSettingsRepository _botSettingsRepository;
    private readonly IMessageSender _messageSender;
    private readonly HashSet<string> _notifiedEmployeeDates = new();

    /// <summary>
    /// Создает фоновый обработчик напоминаний об отчете.
    /// </summary>
    public DailyReportReminderWorker(
        IReportService reportService,
        IUserRepository userRepository,
        IBotSettingsRepository botSettingsRepository,
        IMessageSender messageSender)
    {
        _reportService = reportService;
        _userRepository = userRepository;
        _botSettingsRepository = botSettingsRepository;
        _messageSender = messageSender;
    }

    /// <summary>
    /// Запускает периодическую проверку отправки отчетов.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Фоновая проверка ежедневных отчетов запущена.");
        await CheckReportsAsync(cancellationToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await CheckReportsAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Фоновая проверка ежедневных отчетов остановлена.");
        }
    }

    /// <summary>
    /// Проверяет сотрудников и отправляет напоминания тем, кто не отправил отчет.
    /// </summary>
    private Task CheckReportsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var reminderTimeText = _botSettingsRepository.GetValue(DailyReportReminderTimeKey);
            if (!TimeOnly.TryParseExact(reminderTimeText, "HH:mm", out var reminderTime))
            {
                return Task.CompletedTask;
            }

            var now = DateTime.Now;
            if (TimeOnly.FromDateTime(now) < reminderTime)
            {
                return Task.CompletedTask;
            }

            var today = DateOnly.FromDateTime(now);
            var employees = _userRepository.GetEmployees();
            foreach (var employee in employees)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var notificationKey = $"{today:yyyy-MM-dd}:{employee.Id}";
                if (_notifiedEmployeeDates.Contains(notificationKey))
                {
                    continue;
                }

                var report = _reportService.GetTodayReport(employee);
                if (report?.IsSent == true)
                {
                    continue;
                }

                SendReminder(employee.TelegramChatId, employee.FullName);
                _notifiedEmployeeDates.Add(notificationKey);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Ошибка фоновой проверки ежедневных отчетов: {exception}");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Отправляет напоминание сотруднику.
    /// </summary>
    private void SendReminder(long? telegramChatId, string employeeName)
    {
        if (telegramChatId is null)
        {
            Console.WriteLine($"У сотрудника {employeeName} не указан Telegram chat id.");
            return;
        }

        try
        {
            _messageSender.SendMessage(telegramChatId.Value,
                "Напоминание об отчете:\n" +
                "Пожалуйста, отправьте ежедневный отчет за сегодня.");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Не удалось отправить напоминание об отчете сотруднику {employeeName}: {exception}");
        }
    }
}
