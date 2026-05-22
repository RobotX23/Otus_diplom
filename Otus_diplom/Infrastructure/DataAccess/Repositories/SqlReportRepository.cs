using LinqToDB;
using Otus_diplom.Core.DataAccess;
using Otus_diplom.Core.Entities;
using Otus_diplom.Infrastructure.DataAccess;
using Otus_diplom.Infrastructure.DataAccess.Models;

namespace Otus_diplom.Infrastructure.DataAccess.Repositories;

/// <summary>
/// SQL-репозиторий отчетов.
/// </summary>
public class SqlReportRepository : IReportRepository
{
    private readonly IDataContextFactory _dataContextFactory;

    /// <summary>
    /// Создает SQL-репозиторий отчетов.
    /// </summary>
    public SqlReportRepository(IDataContextFactory dataContextFactory)
    {
        _dataContextFactory = dataContextFactory;
    }

    /// <summary>
    /// Возвращает отчет сотрудника за выбранную дату.
    /// </summary>
    public DailyReport? GetByEmployeeAndDate(int employeeId, DateOnly date)
    {
        using var db = _dataContextFactory.CreateDataContext();
        var model = db.DailyReports.FirstOrDefault(report =>
            report.EmployeeId == employeeId && report.Date == date);

        return model is null ? null : LoadReport(db, model);
    }

    /// <summary>
    /// Возвращает все отчеты за выбранную дату.
    /// </summary>
    public List<DailyReport> GetByDate(DateOnly date)
    {
        using var db = _dataContextFactory.CreateDataContext();
        return db.DailyReports
            .Where(report => report.Date == date)
            .ToList()
            .Select(model => LoadReport(db, model))
            .ToList();
    }

    /// <summary>
    /// Добавляет новый отчет в базу данных.
    /// </summary>
    public void Add(DailyReport report)
    {
        using var db = _dataContextFactory.CreateDataContext();
        var model = ModelMapper.ToModel(report);
        var id = db.InsertWithInt32Identity(model);
        report.Id = id;
        SaveReportDetails(db, report);
    }

    /// <summary>
    /// Сохраняет отчет и связанные с ним выполненные задачи и проблемы.
    /// </summary>
    public void Save(DailyReport report)
    {
        using var db = _dataContextFactory.CreateDataContext();

        db.DailyReports
            .Where(item => item.Id == report.Id)
            .Set(item => item.IsSent, report.IsSent)
            .Update();

        SaveReportDetails(db, report);
    }

    /// <summary>
    /// Загружает отчет вместе с выполненными задачами и проблемами.
    /// </summary>
    private static DailyReport LoadReport(AppDataContext db, SqlDailyReportModel model)
    {
        var report = ModelMapper.ToEntity(model);

        var completedTasks = db.ReportCompletedTasks
            .Where(task => task.ReportId == model.Id)
            .OrderBy(task => task.Id)
            .Select(task => task.TaskText)
            .ToList();

        var blocks = db.ReportBlocks
            .Where(block => block.ReportId == model.Id)
            .OrderBy(block => block.Id)
            .Select(block => block.BlockText)
            .ToList();

        report.CompletedTasks.AddRange(completedTasks);
        report.Blocks.AddRange(blocks);

        return report;
    }

    /// <summary>
    /// Перезаписывает связанные строки отчета.
    /// </summary>
    private static void SaveReportDetails(AppDataContext db, DailyReport report)
    {
        db.ReportCompletedTasks
            .Where(task => task.ReportId == report.Id)
            .Delete();

        db.ReportBlocks
            .Where(block => block.ReportId == report.Id)
            .Delete();

        foreach (var completedTask in report.CompletedTasks)
        {
            db.Insert(new SqlReportCompletedTaskModel
            {
                ReportId = report.Id,
                TaskText = completedTask
            });
        }

        foreach (var block in report.Blocks)
        {
            db.Insert(new SqlReportBlockModel
            {
                ReportId = report.Id,
                BlockText = block
            });
        }
    }
}
