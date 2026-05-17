using LinqToDB.Mapping;

namespace Otus_diplom.Infrastructure.DataAccess.Models;

/// <summary>
/// Модель задачи сотрудника для будущей таблицы базы данных.
/// </summary>
[Table(Name = "employee_tasks")]
public class SqlEmployeeTaskModel
{
    /// <summary>
    /// Номер задачи в базе данных.
    /// </summary>
    [PrimaryKey]
    [Identity]
    [Column(Name = "id")]
    public int Id { get; set; }

    /// <summary>
    /// Номер сотрудника.
    /// </summary>
    [Column(Name = "employee_id")]
    public int EmployeeId { get; set; }

    /// <summary>
    /// Номер lead, который назначил задачу.
    /// </summary>
    [Column(Name = "lead_id")]
    public int LeadId { get; set; }

    /// <summary>
    /// Название задачи.
    /// </summary>
    [Column(Name = "title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Срок выполнения.
    /// </summary>
    [Column(Name = "deadline")]
    public DateOnly Deadline { get; set; }

    /// <summary>
    /// Статус задачи.
    /// </summary>
    [Column(Name = "status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Комментарий при закрытии задачи.
    /// </summary>
    [Column(Name = "closing_comment")]
    public string? ClosingComment { get; set; }
}
