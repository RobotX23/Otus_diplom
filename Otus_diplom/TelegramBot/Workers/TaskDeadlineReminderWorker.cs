using Otus_diplom.Core.DataAccess;
using Otus_diplom.Core.Entities;
using Otus_diplom.Core.Services;
using Otus_diplom.TelegramBot.Messaging;
using TaskStatus = Otus_diplom.Core.Entities.TaskStatus;

namespace Otus_diplom.TelegramBot.Workers;

/// <summary>
/// Фоновая отправка уведомлений сотрудникам о приближении дедлайна задач.
/// </summary>
public class TaskDeadlineReminderWorker
{
    private const string TaskDeadlineReminderHoursKey = "task_deadline_reminder_hours";

    private readonly ITaskService _taskService;
    private readonly IUserRepository _userRepository;
    private readonly IBotSettingsRepository _botSettingsRepository;
    private readonly IMessageSender _messageSender;
    private readonly HashSet<int> _notifiedTaskIds = new();

    /// <summary>
    /// Создает фоновый обработчик уведомлений о дедлайнах.
    /// </summary>
    public TaskDeadlineReminderWorker(
        ITaskService taskService,
        IUserRepository userRepository,
        IBotSettingsRepository botSettingsRepository,
        IMessageSender messageSender)
    {
        _taskService = taskService;
        _userRepository = userRepository;
        _botSettingsRepository = botSettingsRepository;
        _messageSender = messageSender;
    }

    /// <summary>
    /// Запускает периодическую проверку задач.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Фоновая проверка дедлайнов задач запущена.");
        await CheckDeadlinesAsync(cancellationToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await CheckDeadlinesAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Фоновая проверка дедлайнов задач остановлена.");
        }
    }

    /// <summary>
    /// Проверяет задачи и отправляет уведомления, если дедлайн близко.
    /// </summary>
    private async Task CheckDeadlinesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var reminderHoursText = _botSettingsRepository.GetValue(TaskDeadlineReminderHoursKey);
            if (!int.TryParse(reminderHoursText, out var reminderHours))
            {
                return;
            }

            var now = DateTime.Now;
            var tasks = _taskService.GetAllTasks()
                .Where(task => task.Status != TaskStatus.Closed)
                .Where(task => !_notifiedTaskIds.Contains(task.Id))
                .ToList();

            foreach (var task in tasks)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!ShouldNotify(task, reminderHours, now))
                {
                    continue;
                }

                await SendReminderAsync(task);
                _notifiedTaskIds.Add(task.Id);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Ошибка фоновой проверки дедлайнов задач: {exception}");
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Проверяет, пора ли отправить уведомление по задаче.
    /// </summary>
    private static bool ShouldNotify(EmployeeTask task, int reminderHours, DateTime now)
    {
        var deadline = task.Deadline.ToDateTime(new TimeOnly(23, 59));
        var reminderTime = deadline.AddHours(-reminderHours);
        return now >= reminderTime && now <= deadline;
    }

    /// <summary>
    /// Отправляет уведомление сотруднику.
    /// </summary>
    private async Task SendReminderAsync(EmployeeTask task)
    {
        var employee = _userRepository.GetById(task.EmployeeId);
        if (employee?.TelegramChatId is null)
        {
            Console.WriteLine($"У сотрудника для задачи {task.Id} не указан Telegram chat id.");
            return;
        }

        try
        {
            await _messageSender.SendMessageAsync(employee.TelegramChatId.Value,
                "Напоминание о дедлайне задачи:\n" +
                $"{task.Title}\n" +
                $"Срок: {task.Deadline:dd.MM.yyyy}\n" +
                "Пожалуйста, проверьте статус задачи.");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Не удалось отправить напоминание по задаче {task.Id}: {exception}");
        }
    }
}
