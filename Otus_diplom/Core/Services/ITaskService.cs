using Otus_diplom.Core.Entities;

namespace Otus_diplom.Core.Services;

/// <summary>
/// Контракт сервиса задач.
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Назначает задачу сотруднику.
    /// </summary>
    EmployeeTask AssignTask(User lead, User employee, string title, DateOnly deadline);

    /// <summary>
    /// Возвращает задачи сотрудника.
    /// </summary>
    List<EmployeeTask> GetEmployeeTasks(User employee);

    /// <summary>
    /// Возвращает все задачи.
    /// </summary>
    List<EmployeeTask> GetAllTasks();

    /// <summary>
    /// Переводит задачу в работу.
    /// </summary>
    bool StartTask(User employee, int taskId);

    /// <summary>
    /// Закрывает задачу с комментарием.
    /// </summary>
    bool CloseTask(User employee, int taskId, string comment);
}
