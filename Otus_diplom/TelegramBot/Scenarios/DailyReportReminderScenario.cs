using Otus_diplom.Core.Entities;
using Otus_diplom.Core.Services;

namespace Otus_diplom.TelegramBot.Scenarios;

/// <summary>
/// Сценарий настройки времени напоминания об отчете.
/// </summary>
public class DailyReportReminderScenario : IScenario
{
    private const string WaitTimeStep = "WaitTime";

    private readonly IBotSettingsService _botSettingsService;
    private readonly IScenarioContextRepository _contextRepository;

    /// <summary>
    /// Создает сценарий настройки времени напоминания об отчете.
    /// </summary>
    public DailyReportReminderScenario(
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
        return scenarioType == ScenarioType.DailyReportReminder;
    }

    /// <summary>
    /// Запускает сценарий настройки времени напоминания.
    /// </summary>
    public ScenarioResult Start(long chatId, User user)
    {
        var context = new ScenarioContext
        {
            ChatId = chatId,
            UserId = user.Id,
            ScenarioType = ScenarioType.DailyReportReminder,
            Step = WaitTimeStep
        };

        _contextRepository.Save(context);
        return new ScenarioResult
        {
            Message = "Введите время напоминания об отчете в формате 00:00 до 23:59"
        };
    }

    /// <summary>
    /// Обрабатывает сообщение пользователя внутри сценария.
    /// </summary>
    public ScenarioResult HandleMessage(ScenarioContext context, User user, string text)
    {
        _botSettingsService.SetDailyReportReminderTime(user, text);
        _contextRepository.Delete(context.ChatId);

        return new ScenarioResult
        {
            Message = "Настройка напоминания об отчете сохранена"
        };
    }

    /// <summary>
    /// Обрабатывает callback от inline-кнопки внутри сценария.
    /// </summary>
    public ScenarioResult HandleCallback(ScenarioContext context, User user, string callbackData)
    {
        return new ScenarioResult
        {
            Message = "Введите время сообщением в чат."
        };
    }
}
