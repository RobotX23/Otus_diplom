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
    public User AddEmployeeByTelegramUsername(User admin, string telegramUsername)
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

        var employee = new User
        {
            FullName = normalizedUsername,
            TelegramUsername = normalizedUsername,
            TelegramChatId = null,
            Role = UserRole.Employee
        };

        _userRepository.Add(employee);
        return employee;
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
}
