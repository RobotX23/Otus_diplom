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
    DailyReportReminder,

    /// <summary>
    /// Сценарий добавления выполненной задачи в отчет.
    /// </summary>
    AddCompletedTask,

    /// <summary>
    /// Сценарий просмотра и изменения задач сотрудника.
    /// </summary>
    MyTasks,

    /// <summary>
    /// Сценарий просмотра последнего отчета сотрудника.
    /// </summary>
    LastReport,

    /// <summary>
    /// Сценарий добавления проблемы в отчет.
    /// </summary>
    AddBlock,

    /// <summary>
    /// Сценарий подтверждения отправки отчета.
    /// </summary>
    SendReport,

    /// <summary>
    /// Сценарий просмотра задач сотрудников lead.
    /// </summary>
    LeadEmployeeTasks,

    /// <summary>
    /// Сценарий просмотра отчета сотрудника lead.
    /// </summary>
    LeadEmployeeReport
}
