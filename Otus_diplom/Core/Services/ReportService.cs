using Otus_diplom.Core.DataAccess;
using Otus_diplom.Core.Entities;
using Otus_diplom.Core.Exceptions;

namespace Otus_diplom.Core.Services;

/// <summary>
/// Сервис для работы с ежедневными отчетами сотрудников.
/// </summary>
public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly int _maxCompletedTaskLength;

    /// <summary>
    /// Создает сервис отчетов, получает репозиторий отчетов и ограничение длины выполненной задачи.
    /// </summary>
    public ReportService(IReportRepository reportRepository, int maxCompletedTaskLength)
    {
        _reportRepository = reportRepository;
        _maxCompletedTaskLength = maxCompletedTaskLength;
    }

    /// <summary>
    /// Отмечает сегодняшний отчет сотрудника как отправленный.
    /// </summary>
    public DailyReport SendReport(User employee)
    {
        var report = GetOrCreateTodayReport(employee);
        report.IsSent = true;
        _reportRepository.Save(report);
        return report;
    }

    /// <summary>
    /// Добавляет выполненную задачу в сегодняшний отчет сотрудника.
    /// </summary>
    public void AddCompletedTask(User employee, string text)
    {
        ValidateCompletedTaskText(text);

        var report = GetOrCreateTodayReport(employee);
        ValidateReportIsNotSent(report);

        report.CompletedTasks.Add(text);
        _reportRepository.Save(report);
    }

    /// <summary>
    /// Удаляет выполненную задачу из сегодняшнего отчета, если отчет еще не отправлен.
    /// </summary>
    public bool RemoveCompletedTask(User employee, int taskIndex)
    {
        var report = GetTodayReport(employee);
        if (report is null || report.IsSent || taskIndex < 0 || taskIndex >= report.CompletedTasks.Count)
        {
            return false;
        }

        report.CompletedTasks.RemoveAt(taskIndex);
        _reportRepository.Save(report);
        return true;
    }

    /// <summary>
    /// Удаляет проблему из сегодняшнего отчета, если отчет еще не отправлен.
    /// </summary>
    public bool RemoveBlock(User employee, int blockIndex)
    {
        var report = GetTodayReport(employee);
        if (report is null || report.IsSent || blockIndex < 0 || blockIndex >= report.Blocks.Count)
        {
            return false;
        }

        report.Blocks.RemoveAt(blockIndex);
        _reportRepository.Save(report);
        return true;
    }

    /// <summary>
    /// Добавляет проблему или блокер в сегодняшний отчет сотрудника.
    /// </summary>
    public void AddBlock(User employee, string text)
    {
        var report = GetOrCreateTodayReport(employee);
        ValidateReportIsNotSent(report);

        report.Blocks.Add(text);
        _reportRepository.Save(report);
    }

    /// <summary>
    /// Возвращает сегодняшний отчет сотрудника, если он уже создан.
    /// </summary>
    public DailyReport? GetTodayReport(User employee)
    {
        return _reportRepository.GetByEmployeeAndDate(employee.Id, DateOnly.FromDateTime(DateTime.Today));
    }

    /// <summary>
    /// Возвращает все отчеты, созданные за сегодняшний день.
    /// </summary>
    public List<DailyReport> GetTodayReports()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return _reportRepository.GetByDate(today);
    }

    /// <summary>
    /// Находит сегодняшний отчет сотрудника или создает новый.
    /// </summary>
    private DailyReport GetOrCreateTodayReport(User employee)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var report = _reportRepository.GetByEmployeeAndDate(employee.Id, today);

        if (report is not null)
        {
            return report;
        }

        report = new DailyReport
        {
            EmployeeId = employee.Id,
            Date = today
        };

        _reportRepository.Add(report);
        return report;
    }

    /// <summary>
    /// Проверяет текст выполненной задачи для отчета.
    /// </summary>
    private void ValidateCompletedTaskText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException("Текст выполненной задачи не может быть пустым.");
        }

        if (text.Length > _maxCompletedTaskLength)
        {
            throw new DomainException($"Текст выполненной задачи не должен быть длиннее {_maxCompletedTaskLength} символов.");
        }
    }

    /// <summary>
    /// Проверяет, что отчет еще не отправлен.
    /// </summary>
    private static void ValidateReportIsNotSent(DailyReport report)
    {
        if (report.IsSent)
        {
            throw new DomainException("Отчет за сегодня уже отправлен. Добавлять задачи и проблемы больше нельзя.");
        }
    }
}
