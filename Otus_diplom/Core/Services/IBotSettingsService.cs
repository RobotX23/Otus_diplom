using Otus_diplom.Core.Entities;

namespace Otus_diplom.Core.Services;

/// <summary>
/// Сервис настройки параметров бота.
/// </summary>
public interface IBotSettingsService
{
    /// <summary>
    /// Сохраняет время уведомления о дедлайне задачи в часах.
    /// </summary>
    void SetTaskDeadlineReminderHours(User admin, string hoursText);

    /// <summary>
    /// Сохраняет время напоминания об отчете.
    /// </summary>
    void SetDailyReportReminderTime(User admin, string timeText);
}
