using Otus_diplom.Core.Entities;

namespace Otus_diplom.Core.DataAccess;

/// <summary>
/// Контракт репозитория ежедневных отчетов.
/// </summary>
public interface IReportRepository
{
    /// <summary>
    /// Возвращает отчет сотрудника за дату.
    /// </summary>
    DailyReport? GetByEmployeeAndDate(int employeeId, DateOnly date);

    /// <summary>
    /// Возвращает отчеты за дату.
    /// </summary>
    List<DailyReport> GetByDate(DateOnly date);

    /// <summary>
    /// Добавляет новый отчет.
    /// </summary>
    void Add(DailyReport report);

    /// <summary>
    /// Сохраняет изменения отчета.
    /// </summary>
    void Save(DailyReport report);

    /// <summary>
    /// Возвращает следующий номер отчета.
    /// </summary>
    int GetNextId();
}
