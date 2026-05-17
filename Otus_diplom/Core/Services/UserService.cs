using Otus_diplom.Core.DataAccess;
using Otus_diplom.Core.Entities;
using Otus_diplom.Core.Exceptions;

namespace Otus_diplom.Core.Services;

/// <summary>
/// Содержит бизнес-логику работы с пользователями.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Создает сервис пользователей.
    /// </summary>
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Добавляет нового сотрудника по username Telegram.
    /// </summary>
    public User AddEmployee(User admin, string telegramUsername, string fullName)
    {
        if (admin.Role != UserRole.Administrator)
        {
            throw new DomainException("Команда доступна только администратору.");
        }

        var normalizedUsername = NormalizeTelegramUsername(telegramUsername);
        if (string.IsNullOrWhiteSpace(normalizedUsername) || normalizedUsername.Contains(' '))
        {
            throw new DomainException("Username указан неверно. Введите username без пробелов, например: ivan_ivanov");
        }

        if (_userRepository.GetByTelegramUsername(normalizedUsername) is not null ||
            _userRepository.GetByFullName(normalizedUsername) is not null)
        {
            throw new DomainException("Пользователь с таким username уже существует.");
        }

        var normalizedFullName = NormalizeFullName(fullName);
        if (string.IsNullOrWhiteSpace(normalizedFullName))
        {
            throw new DomainException("Введите ФИО сотрудника.");
        }

        if (_userRepository.GetByFullName(normalizedFullName) is not null)
        {
            throw new DomainException("Пользователь с таким ФИО уже существует.");
        }

        var employee = new User
        {
            FullName = normalizedFullName,
            TelegramUsername = normalizedUsername,
            TelegramChatId = null,
            Role = UserRole.Employee
        };

        _userRepository.Add(employee);
        return employee;
    }

    /// <summary>
    /// Возвращает сотрудников, которых можно назначить lead.
    /// </summary>
    public List<User> GetLeadCandidates(User admin)
    {
        if (admin.Role != UserRole.Administrator)
        {
            throw new DomainException("Команда доступна только администратору.");
        }

        return _userRepository.GetAll()
            .Where(user => user.Role == UserRole.Employee)
            .OrderBy(user => user.FullName)
            .ToList();
    }

    /// <summary>
    /// Назначает выбранного сотрудника lead.
    /// </summary>
    public User AssignLead(User admin, int userId)
    {
        if (admin.Role != UserRole.Administrator)
        {
            throw new DomainException("Команда доступна только администратору.");
        }

        var selectedUser = _userRepository.GetById(userId);
        if (selectedUser is null || selectedUser.Role != UserRole.Employee)
        {
            throw new DomainException("Сотрудник для назначения lead не найден.");
        }

        foreach (var user in _userRepository.GetAll().Where(user => user.Role == UserRole.Lead))
        {
            user.Role = UserRole.Employee;
            _userRepository.Save(user);
        }

        selectedUser.Role = UserRole.Lead;
        _userRepository.Save(selectedUser);
        return selectedUser;
    }

    /// <summary>
    /// Возвращает сотрудников, которых можно удалить.
    /// </summary>
    public List<User> GetDeleteCandidates(User admin)
    {
        if (admin.Role != UserRole.Administrator)
        {
            throw new DomainException("Команда доступна только администратору.");
        }

        return _userRepository.GetAll()
            .Where(user => user.Role == UserRole.Employee)
            .OrderBy(user => user.FullName)
            .ToList();
    }

    /// <summary>
    /// Удаляет выбранного сотрудника.
    /// </summary>
    public User DeleteEmployee(User admin, int userId)
    {
        if (admin.Role != UserRole.Administrator)
        {
            throw new DomainException("Команда доступна только администратору.");
        }

        var selectedUser = _userRepository.GetById(userId);
        if (selectedUser is null || selectedUser.Role != UserRole.Employee)
        {
            throw new DomainException("Можно удалить только сотрудника. Администратора и lead удалить нельзя.");
        }

        if (!_userRepository.Delete(selectedUser.Id))
        {
            throw new DomainException("Не удалось удалить сотрудника.");
        }

        return selectedUser;
    }

    /// <summary>
    /// Привязывает chat id Telegram к пользователю, найденному по username.
    /// </summary>
    public User? AttachTelegramChatId(string? telegramUsername, long chatId)
    {
        if (string.IsNullOrWhiteSpace(telegramUsername))
        {
            return null;
        }

        var user = _userRepository.GetByTelegramUsername(telegramUsername);
        if (user is null)
        {
            return null;
        }

        user.TelegramChatId = chatId;
        _userRepository.Save(user);
        return user;
    }

    /// <summary>
    /// Приводит username Telegram к единому виду.
    /// </summary>
    private static string NormalizeTelegramUsername(string telegramUsername)
    {
        return telegramUsername.Trim().TrimStart('@');
    }

    /// <summary>
    /// Убирает лишние пробелы из ФИО сотрудника.
    /// </summary>
    private static string NormalizeFullName(string fullName)
    {
        return string.Join(' ', fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
