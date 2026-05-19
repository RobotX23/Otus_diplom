using Otus_diplom.Core.Entities;

namespace Otus_diplom.Core.DataAccess;

/// <summary>
/// Контракт репозитория задач сотрудников.
/// </summary>
public interface ITaskRepository
{
    /// <summary>
    /// Добавляет новую задачу.
    /// </summary>
    void Add(EmployeeTask task);

    /// <summary>
    /// Сохраняет изменения задачи.
    /// </summary>
    void Save(EmployeeTask task);

    /// <summary>
    /// Возвращает задачи сотрудника.
    /// </summary>
    List<EmployeeTask> GetByEmployeeId(int employeeId);

    /// <summary>
    /// Возвращает все задачи.
    /// </summary>
    List<EmployeeTask> GetAll();

    /// <summary>
    /// Ищет задачу сотрудника по номеру.
    /// </summary>
    EmployeeTask? GetByEmployeeAndId(int employeeId, int taskId);
}
