using Otus_diplom.Core.DataAccess;
using Otus_diplom.Core.Entities;
using Otus_diplom.Core.Exceptions;

namespace Otus_diplom.Core.Services;

/// <summary>
/// Настройки параметров бота.
/// </summary>
public class BotSettingsService : IBotSettingsService
{
    private const string TaskDeadlineReminderHoursKey = "task_deadline_reminder_hours";
    private const string DailyReportReminderTimeKey = "daily_report_reminder_time";

    private readonly IBotSettingsRepository _botSettingsRepository;

    /// <summary>
    /// Создает сервис настроек бота.
    /// </summary>
    public BotSettingsService(IBotSettingsRepository botSettingsRepository)
    {
        _botSettingsRepository = botSettingsRepository;
    }

    /// <summary>
    /// Сохраняет время уведомления о дедлайне задачи в часах.
    /// </summary>
    public void SetTaskDeadlineReminderHours(User admin, string hoursText)
    {
        if (admin.Role != UserRole.Administrator)
        {
            throw new DomainException("Команда доступна только администратору.");
        }

        if (!int.TryParse(hoursText.Trim(), out var hours) || hours < 0 || hours > 23)
        {
            throw new DomainException("Введите настройку уведомления в часах от 0 до 23.");
        }

        _botSettingsRepository.SetValue(TaskDeadlineReminderHoursKey, hours.ToString());
    }

    /// <summary>
    /// Сохраняет время напоминания об отчете.
    /// </summary>
    public void SetDailyReportReminderTime(User admin, string timeText)
    {
        if (admin.Role != UserRole.Administrator)
        {
            throw new DomainException("Команда доступна только администратору.");
        }

        if (!TimeOnly.TryParseExact(timeText.Trim(), "HH:mm", out var time))
        {
            throw new DomainException("Введите время напоминания об отчете в формате 00:00 до 23:59.");
        }

        _botSettingsRepository.SetValue(DailyReportReminderTimeKey, time.ToString("HH:mm"));
    }
}
