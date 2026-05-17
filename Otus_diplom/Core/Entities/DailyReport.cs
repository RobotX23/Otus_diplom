namespace Otus_diplom.Core.Entities;

/// <summary>
/// Ежедневный отчет сотрудника.
/// </summary>
public class DailyReport
{
    /// <summary>
    /// Уникальный номер отчета.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Номер сотрудника, которому принадлежит отчет.
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// Дата отчета.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Признак того, что сотрудник подтвердил отправку отчета.
    /// </summary>
    public bool IsSent { get; set; }

    /// <summary>
    /// Список выполненных задач за день.
    /// </summary>
    public List<string> CompletedTasks { get; } = new();

    /// <summary>
    /// Список проблем или блокеров за день.
    /// </summary>
    public List<string> Blocks { get; } = new();
}
