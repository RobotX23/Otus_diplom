using Otus_diplom.Core.Entities;
using Otus_diplom.Core.Services;

namespace Otus_diplom.TelegramBot.Scenarios;

/// <summary>
/// Сценарий настройки уведомления о дедлайне задачи.
/// </summary>
public class TaskDeadlineReminderScenario : IScenario
{
    private const string WaitHoursStep = "WaitHours";

    private readonly IBotSettingsService _botSettingsService;
    private readonly IScenarioContextRepository _contextRepository;

    /// <summary>
    /// Создает сценарий настройки уведомления о дедлайне задачи.
    /// </summary>
    public TaskDeadlineReminderScenario(
        IBotSettingsService botSettingsService,
        IScenarioContextRepository contextRepository)
    {
        _botSettingsService = botSettingsService;
        _contextRepository = contextRepository;
    }

    /// <summary>
    /// Проверяет, может ли сценарий обработать указанный тип.
    /// </summary>
    public bool CanHandle(ScenarioType scenarioType)
    {
        return scenarioType == ScenarioType.TaskDeadlineReminder;
    }

    /// <summary>
    /// Запускает сценарий настройки уведомления.
    /// </summary>
    public ScenarioResult Start(long chatId, User user)
    {
        var context = new ScenarioContext
        {
            ChatId = chatId,
            UserId = user.Id,
            ScenarioType = ScenarioType.TaskDeadlineReminder,
            Step = WaitHoursStep
        };

        _contextRepository.Save(context);
        return new ScenarioResult
        {
            Message = "Введите настройку уведомления в часах от 0 до 23"
        };
    }

    /// <summary>
    /// Обрабатывает сообщение пользователя внутри сценария.
    /// </summary>
    public ScenarioResult HandleMessage(ScenarioContext context, User user, string text)
    {
        _botSettingsService.SetTaskDeadlineReminderHours(user, text);
        _contextRepository.Delete(context.ChatId);

        return new ScenarioResult
        {
            Message = "Настройка уведомления сохранены"
        };
    }

    /// <summary>
    /// Обрабатывает callback от inline-кнопки внутри сценария.
    /// </summary>
    public ScenarioResult HandleCallback(ScenarioContext context, User user, string callbackData)
    {
        return new ScenarioResult
        {
            Message = "Введите значение сообщением в чат."
        };
    }
}
