using LinqToDB;
using Otus_diplom.Core.DataAccess;
using Otus_diplom.Core.Entities;

namespace Otus_diplom.Infrastructure.DataAccess;

/// <summary>
/// SQL-репозиторий задач.
/// </summary>
public class SqlTaskRepository : ITaskRepository
{
    private readonly IDataContextFactory _dataContextFactory;

    /// <summary>
    /// Создает SQL-репозиторий задач.
    /// </summary>
    public SqlTaskRepository(IDataContextFactory dataContextFactory)
    {
        _dataContextFactory = dataContextFactory;
    }

    /// <summary>
    /// Добавляет новую задачу в базу данных.
    /// </summary>
    public void Add(EmployeeTask task)
    {
        using var db = _dataContextFactory.CreateDataContext();
        var model = ModelMapper.ToModel(task);
        var id = db.InsertWithInt32Identity(model);
        task.Id = id;
    }

    /// <summary>
    /// Сохраняет измененный статус и комментарий задачи.
    /// </summary>
    public void Save(EmployeeTask task)
    {
        using var db = _dataContextFactory.CreateDataContext();
        var model = ModelMapper.ToModel(task);

        db.EmployeeTasks
            .Where(item => item.Id == task.Id)
            .Set(item => item.Status, model.Status)
            .Set(item => item.ClosingComment, model.ClosingComment)
            .Update();
    }

    /// <summary>
    /// Возвращает задачи конкретного сотрудника из базы данных.
    /// </summary>
    public List<EmployeeTask> GetByEmployeeId(int employeeId)
    {
        using var db = _dataContextFactory.CreateDataContext();
        return db.EmployeeTasks
            .Where(task => task.EmployeeId == employeeId)
            .OrderBy(task => task.Id)
            .Select(ModelMapper.ToEntity)
            .ToList();
    }

    /// <summary>
    /// Возвращает все задачи из базы данных.
    /// </summary>
    public List<EmployeeTask> GetAll()
    {
        using var db = _dataContextFactory.CreateDataContext();
        return db.EmployeeTasks
            .OrderBy(task => task.EmployeeId)
            .ThenBy(task => task.Id)
            .Select(ModelMapper.ToEntity)
            .ToList();
    }

    /// <summary>
    /// Ищет задачу сотрудника по номеру.
    /// </summary>
    public EmployeeTask? GetByEmployeeAndId(int employeeId, int taskId)
    {
        using var db = _dataContextFactory.CreateDataContext();
        var model = db.EmployeeTasks.FirstOrDefault(task => task.EmployeeId == employeeId && task.Id == taskId);
        return model is null ? null : ModelMapper.ToEntity(model);
    }
}
