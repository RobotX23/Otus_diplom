using Otus_diplom.Data;
using Otus_diplom.Models;
using TaskStatus = Otus_diplom.Models.TaskStatus;

namespace Otus_diplom.Services;

/// <summary>
/// Сервис для работы с задачами сотрудников.
/// </summary>
public class TaskService
{
    private readonly InMemoryStorage _storage;

    /// <summary>
    /// Создает сервис задач и получает доступ к хранилищу в памяти.
    /// </summary>
    public TaskService(InMemoryStorage storage)
    {
        _storage = storage;
    }

    /// <summary>
    /// Создает новую задачу для сотрудника со статусом "открыто".
    /// </summary>
    public EmployeeTask AssignTask(User employee, string title, DateOnly deadline)
    {
        var task = new EmployeeTask
        {
            Id = _storage.GetNextTaskId(),
            EmployeeId = employee.Id,
            Title = title,
            Deadline = deadline,
            Status = TaskStatus.Open
        };

        _storage.Tasks.Add(task);
        return task;
    }

    /// <summary>
    /// Возвращает список задач конкретного сотрудника.
    /// </summary>
    public List<EmployeeTask> GetEmployeeTasks(User employee)
    {
        return _storage.Tasks
            .Where(task => task.EmployeeId == employee.Id)
            .OrderBy(task => task.Id)
            .ToList();
    }

    /// <summary>
    /// Возвращает список всех задач всех сотрудников.
    /// </summary>
    public List<EmployeeTask> GetAllTasks()
    {
        return _storage.Tasks.OrderBy(task => task.EmployeeId).ThenBy(task => task.Id).ToList();
    }

    /// <summary>
    /// Переводит задачу сотрудника в статус "в работе".
    /// </summary>
    public bool StartTask(User employee, int taskId)
    {
        var task = FindEmployeeTask(employee, taskId);
        if (task is null || task.Status == TaskStatus.Closed)
        {
            return false;
        }

        task.Status = TaskStatus.InProgress;
        return true;
    }

    /// <summary>
    /// Закрывает задачу сотрудника, если передан комментарий.
    /// </summary>
    public bool CloseTask(User employee, int taskId, string comment)
    {
        var task = FindEmployeeTask(employee, taskId);
        if (task is null || string.IsNullOrWhiteSpace(comment))
        {
            return false;
        }

        task.Status = TaskStatus.Closed;
        task.ClosingComment = comment;
        return true;
    }

    /// <summary>
    /// Ищет задачу по номеру и проверяет, что она принадлежит сотруднику.
    /// </summary>
    private EmployeeTask? FindEmployeeTask(User employee, int taskId)
    {
        return _storage.Tasks.FirstOrDefault(task => task.Id == taskId && task.EmployeeId == employee.Id);
    }
}
