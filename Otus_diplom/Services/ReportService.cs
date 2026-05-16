using Otus_diplom.Data;
using Otus_diplom.Models;

namespace Otus_diplom.Services;

/// <summary>
/// Сервис для работы с ежедневными отчетами сотрудников.
/// </summary>
public class ReportService
{
    private readonly InMemoryStorage _storage;

    /// <summary>
    /// Создает сервис отчетов и получает доступ к хранилищу в памяти.
    /// </summary>
    public ReportService(InMemoryStorage storage)
    {
        _storage = storage;
    }

    /// <summary>
    /// Отмечает сегодняшний отчет сотрудника как отправленный.
    /// </summary>
    public DailyReport SendReport(User employee)
    {
        var report = GetOrCreateTodayReport(employee);
        report.IsSent = true;
        return report;
    }

    /// <summary>
    /// Добавляет выполненную задачу в сегодняшний отчет сотрудника.
    /// </summary>
    public void AddCompletedTask(User employee, string text)
    {
        var report = GetOrCreateTodayReport(employee);
        report.CompletedTasks.Add(text);
    }

    /// <summary>
    /// Добавляет проблему или блокер в сегодняшний отчет сотрудника.
    /// </summary>
    public void AddBlock(User employee, string text)
    {
        var report = GetOrCreateTodayReport(employee);
        report.Blocks.Add(text);
    }

    /// <summary>
    /// Возвращает сегодняшний отчет сотрудника, если он уже создан.
    /// </summary>
    public DailyReport? GetTodayReport(User employee)
    {
        return _storage.Reports.FirstOrDefault(report =>
            report.EmployeeId == employee.Id && report.Date == DateOnly.FromDateTime(DateTime.Today));
    }

    /// <summary>
    /// Возвращает все отчеты, созданные за сегодняшний день.
    /// </summary>
    public List<DailyReport> GetTodayReports()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return _storage.Reports.Where(report => report.Date == today).ToList();
    }

    /// <summary>
    /// Находит сегодняшний отчет сотрудника или создает новый.
    /// </summary>
    private DailyReport GetOrCreateTodayReport(User employee)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var report = _storage.Reports.FirstOrDefault(item =>
            item.EmployeeId == employee.Id && item.Date == today);

        if (report is not null)
        {
            return report;
        }

        report = new DailyReport
        {
            Id = _storage.GetNextReportId(),
            EmployeeId = employee.Id,
            Date = today
        };

        _storage.Reports.Add(report);
        return report;
    }
}
