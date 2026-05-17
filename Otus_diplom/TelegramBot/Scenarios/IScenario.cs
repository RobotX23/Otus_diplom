using Otus_diplom.Core.Entities;

namespace Otus_diplom.TelegramBot.Scenarios;

/// <summary>
/// Контракт многошагового сценария Telegram-бота.
/// </summary>
public interface IScenario
{
    /// <summary>
    /// Проверяет, может ли сценарий обработать указанный тип.
    /// </summary>
    bool CanHandle(ScenarioType scenarioType);

    /// <summary>
    /// Запускает сценарий.
    /// </summary>
    ScenarioResult Start(long chatId, User user);

    /// <summary>
    /// Обрабатывает сообщение пользователя внутри сценария.
    /// </summary>
    ScenarioResult HandleMessage(ScenarioContext context, User user, string text);
}
