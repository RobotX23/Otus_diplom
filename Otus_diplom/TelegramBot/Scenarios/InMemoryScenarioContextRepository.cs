namespace Otus_diplom.TelegramBot.Scenarios;

/// <summary>
/// In-memory хранилище контекстов сценариев.
/// </summary>
public class InMemoryScenarioContextRepository : IScenarioContextRepository
{
    private readonly Dictionary<long, ScenarioContext> _contexts = new();

    /// <summary>
    /// Возвращает активный контекст по chat id.
    /// </summary>
    public ScenarioContext? GetByChatId(long chatId)
    {
        return _contexts.GetValueOrDefault(chatId);
    }

    /// <summary>
    /// Сохраняет контекст сценария.
    /// </summary>
    public void Save(ScenarioContext context)
    {
        _contexts[context.ChatId] = context;
    }

    /// <summary>
    /// Удаляет контекст сценария.
    /// </summary>
    public void Delete(long chatId)
    {
        _contexts.Remove(chatId);
    }
}
