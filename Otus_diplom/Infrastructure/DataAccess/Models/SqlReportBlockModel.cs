using LinqToDB.Mapping;

namespace Otus_diplom.Infrastructure.DataAccess.Models;

/// <summary>
/// Модель проблемы или блокера внутри ежедневного отчета.
/// </summary>
[Table(Name = "report_blocks")]
public class SqlReportBlockModel
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
    /// Текст проблемы или блокера.
    /// </summary>
    [Column(Name = "block_text")]
    public string BlockText { get; set; } = string.Empty;
}
