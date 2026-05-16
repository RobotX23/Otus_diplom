using Otus_diplom.Models;

namespace Otus_diplom.Data;

/// <summary>
/// Хранилище данных.
/// </summary>
public class InMemoryStorage
{
    private int _reportId = 1;
    private int _taskId = 1;

    /// <summary>
    /// Список пользователей бота.
    /// </summary>
    public List<User> Users { get; } = new();

    /// <summary>
    /// Список ежедневных отчетов сотрудников.
    /// </summary>
    public List<DailyReport> Reports { get; } = new();

    /// <summary>
    /// Список задач, назначенных сотрудникам.
    /// </summary>
    public List<EmployeeTask> Tasks { get; } = new();

    /// <summary>
    /// Заполняет хранилище тестовыми пользователями и стартовой задачей.
    /// </summary>
    public void Seed()
    {
        Users.Add(new User { Id = 1, FullName = "Иван Иванов", Role = UserRole.Employee });
        Users.Add(new User { Id = 2, FullName = "Петр Петров", Role = UserRole.Employee });
        Users.Add(new User { Id = 3, FullName = "Анна Lead", Role = UserRole.Lead });

        Tasks.Add(new EmployeeTask
        {
            Id = GetNextTaskId(),
            EmployeeId = 1,
            Title = "Подготовить отчет по проекту",
            Deadline = DateOnly.FromDateTime(DateTime.Today.AddDays(3))
        });
    }

    /// <summary>
    /// Возвращает следующий уникальный номер отчета.
    /// </summary>
    public int GetNextReportId()
    {
        return _reportId++;
    }

    /// <summary>
    /// Возвращает следующий уникальный номер задачи.
    /// </summary>
    public int GetNextTaskId()
    {
        return _taskId++;
    }
}
