using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using Otus_diplom.Infrastructure.DataAccess.Models;

namespace Otus_diplom.Infrastructure.DataAccess;

/// <summary>
/// Контекст для работы с таблицами приложения.
/// </summary>
public class AppDataContext : DataConnection
{
    /// <summary>
    /// Создает подключение linq2db к PostgreSQL.
    /// </summary>
    public AppDataContext(string connectionString)
        : base(new DataOptions().UsePostgreSQL(connectionString))
    {
    }

    /// <summary>
    /// Таблица пользователей.
    /// </summary>
    public ITable<SqlUserModel> Users => this.GetTable<SqlUserModel>();

    /// <summary>
    /// Таблица ежедневных отчетов.
    /// </summary>
    public ITable<SqlDailyReportModel> DailyReports => this.GetTable<SqlDailyReportModel>();

    /// <summary>
    /// Таблица выполненных задач в отчетах.
    /// </summary>
    public ITable<SqlReportCompletedTaskModel> ReportCompletedTasks => this.GetTable<SqlReportCompletedTaskModel>();

    /// <summary>
    /// Таблица проблем и блокеров в отчетах.
    /// </summary>
    public ITable<SqlReportBlockModel> ReportBlocks => this.GetTable<SqlReportBlockModel>();

    /// <summary>
    /// Таблица задач сотрудников.
    /// </summary>
    public ITable<SqlEmployeeTaskModel> EmployeeTasks => this.GetTable<SqlEmployeeTaskModel>();
}
