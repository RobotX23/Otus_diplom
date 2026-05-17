using LinqToDB.Mapping;

namespace Otus_diplom.Infrastructure.DataAccess.Models;

/// <summary>
/// Модель выполненной задачи внутри ежедневного отчета.
/// </summary>
[Table(Name = "report_completed_tasks")]
public class SqlReportCompletedTaskModel
{
    /// <summary>
    /// Номер записи.
    /// </summary>
    [PrimaryKey]
    [Identity]
    [Column(Name = "id")]
    public int Id { get; set; }

    /// <summary>
    /// Номер отчета.
    /// </summary>
    [Column(Name = "report_id")]
    public int ReportId { get; set; }

    /// <summary>
    /// Текст выполненной задачи.
    /// </summary>
    [Column(Name = "task_text")]
    public string TaskText { get; set; } = string.Empty;
}
