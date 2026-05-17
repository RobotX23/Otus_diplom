namespace Otus_diplom.TelegramBot.Scenarios;

/// <summary>
/// Репозиторий контекстов многошаговых сценариев.
/// </summary>
public interface IScenarioContextRepository
{
    /// <summary>
    /// Возвращает активный контекст по chat id.
    /// </summary>
    ScenarioContext? GetByChatId(long chatId);

    /// <summary>
    /// Сохраняет контекст сценария.
    /// </summary>
    void Save(ScenarioContext context);

    /// <summary>
    /// Удаляет контекст сценария.
    /// </summary>
    void Delete(long chatId);
}
