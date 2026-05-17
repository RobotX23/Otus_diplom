using LinqToDB.Mapping;

namespace Otus_diplom.Infrastructure.DataAccess.Models;

/// <summary>
/// Модель ежедневного отчета для будущей таблицы базы данных.
/// </summary>
[Table(Name = "daily_reports")]
public class SqlDailyReportModel
{
    /// <summary>
    /// Номер отчета в базе данных.
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
    /// Дата отчета.
    /// </summary>
    [Column(Name = "report_date")]
    public DateOnly Date { get; set; }

    /// <summary>
    /// Признак отправки отчета.
    /// </summary>
    [Column(Name = "is_sent")]
    public bool IsSent { get; set; }
}
