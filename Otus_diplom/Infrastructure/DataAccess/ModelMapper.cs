using Otus_diplom.Core.Entities;
using Otus_diplom.Infrastructure.DataAccess.Models;
using TaskStatus = Otus_diplom.Core.Entities.TaskStatus;

namespace Otus_diplom.Infrastructure.DataAccess;

/// <summary>
/// Преобразует модели базы данных в доменные сущности и обратно.
/// </summary>
public static class ModelMapper
{
    /// <summary>
    /// Преобразует модель пользователя из БД в доменную сущность.
    /// </summary>
    public static User ToEntity(SqlUserModel model)
    {
        return new User
        {
            Id = model.Id,
            TelegramChatId = model.TelegramChatId,
            FullName = model.FullName,
            Role = Enum.Parse<UserRole>(model.Role)
        };
    }

    /// <summary>
    /// Преобразует доменного пользователя в модель БД.
    /// </summary>
    public static SqlUserModel ToModel(User user)
    {
        return new SqlUserModel
        {
            Id = user.Id,
            TelegramChatId = user.TelegramChatId,
            FullName = user.FullName,
            Role = user.Role.ToString()
        };
    }

    /// <summary>
    /// Преобразует модель отчета из БД в доменную сущность.
    /// </summary>
    public static DailyReport ToEntity(SqlDailyReportModel model)
    {
        var report = new DailyReport
        {
            Id = model.Id,
            EmployeeId = model.EmployeeId,
            Date = model.Date,
            IsSent = model.IsSent
        };

        return report;
    }

    /// <summary>
    /// Преобразует доменный отчет в модель БД.
    /// </summary>
    public static SqlDailyReportModel ToModel(DailyReport report)
    {
        return new SqlDailyReportModel
        {
            Id = report.Id,
            EmployeeId = report.EmployeeId,
            Date = report.Date,
            IsSent = report.IsSent,
        };
    }

    /// <summary>
    /// Преобразует модель задачи из БД в доменную сущность.
    /// </summary>
    public static EmployeeTask ToEntity(SqlEmployeeTaskModel model)
    {
        return new EmployeeTask
        {
            Id = model.Id,
            EmployeeId = model.EmployeeId,
            LeadId = model.LeadId,
            Title = model.Title,
            Deadline = model.Deadline,
            Status = Enum.Parse<TaskStatus>(model.Status),
            ClosingComment = model.ClosingComment
        };
    }

    /// <summary>
    /// Преобразует доменную задачу в модель БД.
    /// </summary>
    public static SqlEmployeeTaskModel ToModel(EmployeeTask task)
    {
        return new SqlEmployeeTaskModel
        {
            Id = task.Id,
            EmployeeId = task.EmployeeId,
            LeadId = task.LeadId,
            Title = task.Title,
            Deadline = task.Deadline,
            Status = task.Status.ToString(),
            ClosingComment = task.ClosingComment
        };
    }
}
