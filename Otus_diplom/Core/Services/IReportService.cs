using Otus_diplom.Core.Entities;

namespace Otus_diplom.Core.Services;

/// <summary>
/// Контракт сервиса отчетов.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Отмечает сегодняшний отчет сотрудника как отправленный.
    /// </summary>
    DailyReport SendReport(User employee);

    /// <summary>
    /// Добавляет выполненную задачу в сегодняшний отчет.
    /// </summary>
    void AddCompletedTask(User employee, string text);

    /// <summary>
    /// Добавляет проблему или блокер в сегодняшний отчет.
    /// </summary>
    void AddBlock(User employee, string text);

    /// <summary>
    /// Возвращает сегодняшний отчет сотрудника.
    /// </summary>
    DailyReport? GetTodayReport(User employee);

    /// <summary>
    /// Возвращает сегодняшние отчеты.
    /// </summary>
    List<DailyReport> GetTodayReports();
}
