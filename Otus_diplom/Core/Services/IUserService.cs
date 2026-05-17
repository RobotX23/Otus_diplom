using Otus_diplom.Core.Entities;

namespace Otus_diplom.Core.Services;

/// <summary>
/// Сервис для работы с пользователями бота.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Добавляет нового сотрудника по username Telegram.
    /// </summary>
    User AddEmployeeByTelegramUsername(User admin, string telegramUsername);

    /// <summary>
    /// Привязывает chat id Telegram к пользователю, найденному по username.
    /// </summary>
    User? AttachTelegramChatId(string? telegramUsername, long chatId);
}
