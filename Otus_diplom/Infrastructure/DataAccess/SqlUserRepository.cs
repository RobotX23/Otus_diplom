using Otus_diplom.Core.DataAccess;
using Otus_diplom.Core.Entities;

namespace Otus_diplom.Infrastructure.DataAccess;

/// <summary>
/// SQL-репозиторий пользователей.
/// </summary>
public class SqlUserRepository : IUserRepository
{
    private readonly IDataContextFactory _dataContextFactory;

    /// <summary>
    /// Создает SQL-репозиторий пользователей.
    /// </summary>
    public SqlUserRepository(IDataContextFactory dataContextFactory)
    {
        _dataContextFactory = dataContextFactory;
    }

    /// <summary>
    /// Возвращает всех пользователей из базы данных.
    /// </summary>
    public List<User> GetAll()
    {
        using var db = _dataContextFactory.CreateDataContext();
        return db.Users.Select(ModelMapper.ToEntity).ToList();
    }

    /// <summary>
    /// Возвращает всех пользователей с ролью сотрудника.
    /// </summary>
    public List<User> GetEmployees()
    {
        using var db = _dataContextFactory.CreateDataContext();
        return db.Users
            .Where(user => user.Role == UserRole.Employee.ToString())
            .Select(ModelMapper.ToEntity)
            .ToList();
    }

    /// <summary>
    /// Ищет пользователя по номеру в базе данных.
    /// </summary>
    public User? GetById(int id)
    {
        using var db = _dataContextFactory.CreateDataContext();
        var model = db.Users.FirstOrDefault(user => user.Id == id);
        return model is null ? null : ModelMapper.ToEntity(model);
    }

    /// <summary>
    /// Ищет пользователя по идентификатору Telegram-чата.
    /// </summary>
    public User? GetByTelegramChatId(long telegramChatId)
    {
        using var db = _dataContextFactory.CreateDataContext();
        var model = db.Users.FirstOrDefault(user => user.TelegramChatId == telegramChatId);
        return model is null ? null : ModelMapper.ToEntity(model);
    }

    /// <summary>
    /// Ищет пользователя по полному имени.
    /// </summary>
    public User? GetByFullName(string fullName)
    {
        using var db = _dataContextFactory.CreateDataContext();
        var normalizedName = fullName.Trim();
        var model = db.Users.FirstOrDefault(user => user.FullName == normalizedName);
        return model is null ? null : ModelMapper.ToEntity(model);
    }
}
