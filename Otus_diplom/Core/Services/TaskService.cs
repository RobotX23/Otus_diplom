using Otus_diplom.Core.DataAccess;
using Otus_diplom.Core.Entities;
using Otus_diplom.Core.Exceptions;
using TaskStatus = Otus_diplom.Core.Entities.TaskStatus;

namespace Otus_diplom.Core.Services;

/// <summary>
/// Сервис для работы с задачами сотрудников.
/// </summary>
public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly int _maxTaskTitleLength;

    /// <summary>
    /// Создает сервис задач, получает репозиторий задач и ограничение длины названия задачи.
    /// </summary>
    public TaskService(ITaskRepository taskRepository, int maxTaskTitleLength)
    {
        _taskRepository = taskRepository;
        _maxTaskTitleLength = maxTaskTitleLength;
    }

    /// <summary>
    /// Создает новую задачу для сотрудника со статусом "открыто".
    /// </summary>
    public EmployeeTask AssignTask(User lead, User employee, string title, DateOnly deadline)
    {
        ValidateTaskTitle(employee, title);

        var task = new EmployeeTask
        {
            Id = _taskRepository.GetNextId(),
            EmployeeId = employee.Id,
            LeadId = lead.Id,
            Title = title.Trim(),
            Deadline = deadline,
            Status = TaskStatus.Open
        };

        _taskRepository.Add(task);
        return task;
    }

    /// <summary>
    /// Возвращает список задач конкретного сотрудника.
    /// </summary>
    public List<EmployeeTask> GetEmployeeTasks(User employee)
    {
        return _taskRepository.GetByEmployeeId(employee.Id);
    }

    /// <summary>
    /// Возвращает список всех задач всех сотрудников.
    /// </summary>
    public List<EmployeeTask> GetAllTasks()
    {
        return _taskRepository.GetAll();
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
        _taskRepository.Save(task);
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
        _taskRepository.Save(task);
        return true;
    }

    /// <summary>
    /// Ищет задачу по номеру и проверяет, что она принадлежит сотруднику.
    /// </summary>
    private EmployeeTask? FindEmployeeTask(User employee, int taskId)
    {
        return _taskRepository.GetByEmployeeAndId(employee.Id, taskId);
    }

    /// <summary>
    /// Проверяет название задачи перед созданием.
    /// </summary>
    private void ValidateTaskTitle(User employee, string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Название задачи не может быть пустым.");
        }

        var normalizedTitle = title.Trim();
        if (normalizedTitle.Length > _maxTaskTitleLength)
        {
            throw new DomainException($"Название задачи не должно быть длиннее {_maxTaskTitleLength} символов.");
        }

        var hasDuplicate = _taskRepository.GetByEmployeeId(employee.Id)
            .Any(task => task.Title.Equals(normalizedTitle, StringComparison.CurrentCultureIgnoreCase));

        if (hasDuplicate)
        {
            throw new DomainException("Такая задача уже назначена этому сотруднику.");
        }
    }
}
