using Otus_diplom.Core.Entities;
using Otus_diplom.Core.Services;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_diplom.TelegramBot.Scenarios;

/// <summary>
/// Сценарий добавления проблемы в отчет сотрудника.
/// </summary>
public class AddBlockScenario : IScenario
{
    private const string WaitBlockTextStep = "WaitBlockText";
    private const string WaitConfirmationStep = "WaitConfirmation";
    private const string BlockTextKey = "BlockText";

    private readonly IReportService _reportService;
    private readonly IScenarioContextRepository _contextRepository;

    /// <summary>
    /// Создает сценарий добавления проблемы.
    /// </summary>
    public AddBlockScenario(IReportService reportService, IScenarioContextRepository contextRepository)
    {
        _reportService = reportService;
        _contextRepository = contextRepository;
    }

    /// <summary>
    /// Проверяет, может ли сценарий обработать указанный тип.
    /// </summary>
    public bool CanHandle(ScenarioType scenarioType)
    {
        return scenarioType == ScenarioType.AddBlock;
    }

    /// <summary>
    /// Запускает сценарий добавления проблемы.
    /// </summary>
    public ScenarioResult Start(long chatId, User user)
    {
        var report = _reportService.GetTodayReport(user);
        if (report?.IsSent == true)
        {
            _contextRepository.Delete(chatId);
            return new ScenarioResult
            {
                Message = "Отчет за сегодня уже отправлен. Добавлять задачи и проблемы больше нельзя."
            };
        }

        var context = new ScenarioContext
        {
            ChatId = chatId,
            UserId = user.Id,
            ScenarioType = ScenarioType.AddBlock,
            Step = WaitBlockTextStep
        };

        _contextRepository.Save(context);
        return new ScenarioResult
        {
            Message = "Введите проблему или блокер."
        };
    }

    /// <summary>
    /// Обрабатывает сообщение пользователя внутри сценария.
    /// </summary>
    public ScenarioResult HandleMessage(ScenarioContext context, User user, string text)
    {
        if (context.Step == WaitBlockTextStep)
        {
            context.Data[BlockTextKey] = text.Trim();
            context.Step = WaitConfirmationStep;
            _contextRepository.Save(context);

            return new ScenarioResult
            {
                Message = "Сохранить проблему в отчет?",
                Keyboard = CreateYesNoKeyboard()
            };
        }

        if (context.Step == WaitConfirmationStep && text.Equals("Да", StringComparison.CurrentCultureIgnoreCase))
        {
            _reportService.AddBlock(user, context.Data[BlockTextKey]);
            _contextRepository.Delete(context.ChatId);

            return new ScenarioResult
            {
                Message = "Проблема добавлена в отчет"
            };
        }

        if (context.Step == WaitConfirmationStep && text.Equals("Нет", StringComparison.CurrentCultureIgnoreCase))
        {
            _contextRepository.Delete(context.ChatId);

            return new ScenarioResult
            {
                Message = "Добавление проблемы отменено"
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
    /// Создает клавиатуру подтверждения действия.
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
