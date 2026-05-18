using Otus_diplom.Core.Entities;
using Otus_diplom.Core.Services;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_diplom.TelegramBot.Scenarios;

/// <summary>
/// Сценарий подтверждения отправки ежедневного отчета.
/// </summary>
public class SendReportScenario : IScenario
{
    private const string WaitConfirmationStep = "WaitConfirmation";

    private readonly IReportService _reportService;
    private readonly IScenarioContextRepository _contextRepository;

    /// <summary>
    /// Создает сценарий подтверждения отправки отчета.
    /// </summary>
    public SendReportScenario(IReportService reportService, IScenarioContextRepository contextRepository)
    {
        _reportService = reportService;
        _contextRepository = contextRepository;
    }

    /// <summary>
    /// Проверяет, может ли сценарий обработать указанный тип.
    /// </summary>
    public bool CanHandle(ScenarioType scenarioType)
    {
        return scenarioType == ScenarioType.SendReport;
    }

    /// <summary>
    /// Запускает сценарий подтверждения отправки отчета.
    /// </summary>
    public ScenarioResult Start(long chatId, User user)
    {
        var report = _reportService.GetTodayReport(user);
        if (report?.IsSent == true)
        {
            _contextRepository.Delete(chatId);
            return new ScenarioResult
            {
                Message = "Отчет за сегодня уже отправлен."
            };
        }

        var context = new ScenarioContext
        {
            ChatId = chatId,
            UserId = user.Id,
            ScenarioType = ScenarioType.SendReport,
            Step = WaitConfirmationStep
        };

        _contextRepository.Save(context);
        return new ScenarioResult
        {
            Message = "Отправить отчет за сегодня?",
            Keyboard = CreateYesNoKeyboard()
        };
    }

    /// <summary>
    /// Обрабатывает ответ пользователя на подтверждение отправки отчета.
    /// </summary>
    public ScenarioResult HandleMessage(ScenarioContext context, User user, string text)
    {
        if (text.Equals("Да", StringComparison.CurrentCultureIgnoreCase))
        {
            _reportService.SendReport(user);
            _contextRepository.Delete(context.ChatId);

            return new ScenarioResult
            {
                Message = "Отчет отправлен."
            };
        }

        if (text.Equals("Нет", StringComparison.CurrentCultureIgnoreCase))
        {
            _contextRepository.Delete(context.ChatId);

            return new ScenarioResult
            {
                Message = "Отправка отчета отменена."
            };
        }

        return new ScenarioResult
        {
            Message = "Нажмите Да или Нет.",
            Keyboard = CreateYesNoKeyboard()
        };
    }

    /// <summary>
    /// Обрабатывает callback от inline-кнопки внутри сценария.
    /// </summary>
    public ScenarioResult HandleCallback(ScenarioContext context, User user, string callbackData)
    {
        return new ScenarioResult
        {
            Message = "Для этого сценария используйте обычные кнопки Да или Нет."
        };
    }

    /// <summary>
    /// Создает клавиатуру подтверждения отправки отчета.
    /// </summary>
    private static ReplyKeyboardMarkup CreateYesNoKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new[]
            {
                new KeyboardButton("Да"),
                new KeyboardButton("Нет")
            }
        })
        {
            ResizeKeyboard = true,
            IsPersistent = true
        };
    }
}
