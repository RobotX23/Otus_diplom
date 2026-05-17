namespace Otus_diplom.TelegramBot.Scenarios;

/// <summary>
/// Тип многошагового сценария.
/// </summary>
public enum ScenarioType
{
    /// <summary>
    /// Сценарий добавления сотрудника.
    /// </summary>
    AddEmployee,

    /// <summary>
    /// Сценарий назначения lead.
    /// </summary>
    AssignLead,

    /// <summary>
    /// Сценарий удаления сотрудника.
    /// </summary>
    DeleteEmployee,

    /// <summary>
    /// Сценарий настройки уведомления о дедлайне задачи.
    /// </summary>
    TaskDeadlineReminder,

    /// <summary>
    /// Сценарий настройки времени напоминания об отчете.
    /// </summary>
    DailyReportReminder
}
