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
        report.CompletedTasks.Add(text);
        _reportRepository.Save(report);
    }

    /// <summary>
    /// Добавляет проблему или блокер в сегодняшний отчет сотрудника.
    /// </summary>
    public void AddBlock(User employee, string text)
    {
        var report = GetOrCreateTodayReport(employee);
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
            Id = _reportRepository.GetNextId(),
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
}
