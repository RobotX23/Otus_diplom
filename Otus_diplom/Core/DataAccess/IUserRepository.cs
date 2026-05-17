using Otus_diplom.Core.Entities;

namespace Otus_diplom.Core.DataAccess;

/// <summary>
/// Контракт репозитория пользователей.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Возвращает всех пользователей.
    /// </summary>
    List<User> GetAll();

    /// <summary>
    /// Возвращает всех сотрудников.
    /// </summary>
    List<User> GetEmployees();

    /// <summary>
    /// Ищет пользователя по номеру.
    /// </summary>
    User? GetById(int id);

    /// <summary>
    /// Ищет пользователя по идентификатору Telegram-чата.
    /// </summary>
    User? GetByTelegramChatId(long telegramChatId);

    /// <summary>
    /// Ищет пользователя по username Telegram.
    /// </summary>
    User? GetByTelegramUsername(string telegramUsername);

    /// <summary>
    /// Ищет пользователя по имени.
    /// </summary>
    User? GetByFullName(string fullName);

    /// <summary>
    /// Добавляет нового пользователя.
    /// </summary>
    void Add(User user);

    /// <summary>
    /// Сохраняет изменения пользователя.
    /// </summary>
    void Save(User user);

    /// <summary>
    /// Удаляет пользователя по номеру.
    /// </summary>
    bool Delete(int id);
}
