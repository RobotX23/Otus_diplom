namespace Otus_diplom.Models;

/// <summary>
/// Задача, которую lead назначает сотруднику.
/// </summary>
public class EmployeeTask
{
    /// <summary>
    /// Уникальный номер задачи.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Номер сотрудника, которому назначена задача.
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// Название или краткое описание задачи.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Срок выполнения задачи.
    /// </summary>
    public DateOnly Deadline { get; set; }

    /// <summary>
    /// Текущий статус задачи.
    /// </summary>
    public TaskStatus Status { get; set; } = TaskStatus.Open;

    /// <summary>
    /// Комментарий сотрудника при закрытии задачи.
    /// </summary>
    public string? ClosingComment { get; set; }
}
