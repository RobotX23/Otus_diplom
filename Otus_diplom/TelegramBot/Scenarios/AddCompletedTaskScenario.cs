using Otus_diplom.Core.Entities;
using Otus_diplom.Core.Services;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_diplom.TelegramBot.Scenarios;

/// <summary>
/// Сценарий добавления выполненной задачи в отчет сотрудника.
/// </summary>
public class AddCompletedTaskScenario : IScenario
{
    private const string WaitTaskTextStep = "WaitTaskText";
    private const string WaitConfirmationStep = "WaitConfirmation";
    private const string TaskTextKey = "TaskText";

    private readonly IReportService _reportService;
    private readonly IScenarioContextRepository _contextRepository;

    /// <summary>
    /// Создает сценарий добавления выполненной задачи.
    /// </summary>
    public AddCompletedTaskScenario(IReportService reportService, IScenarioContextRepository contextRepository)
    {
        _reportService = reportService;
        _contextRepository = contextRepository;
    }

    /// <summary>
    /// Проверяет, может ли сценарий обработать указанный тип.
    /// </summary>
    public bool CanHandle(ScenarioType scenarioType)
    {
        return scenarioType == ScenarioType.AddCompletedTask;
    }

    /// <summary>
    /// Запускает сценарий добавления выполненной задачи.
    /// </summary>
    public ScenarioResult Start(long chatId, User user)
    {
        var context = new ScenarioContext
        {
            ChatId = chatId,
            UserId = user.Id,
            ScenarioType = ScenarioType.AddCompletedTask,
            Step = WaitTaskTextStep
        };

        _contextRepository.Save(context);
        return new ScenarioResult
        {
            Message = "Введите выполненную задачу."
        };
    }

    /// <summary>
    /// Обрабатывает сообщение пользователя внутри сценария.
    /// </summary>
    public ScenarioResult HandleMessage(ScenarioContext context, User user, string text)
    {
        if (context.Step == WaitTaskTextStep)
        {
            context.Data[TaskTextKey] = text.Trim();
            context.Step = WaitConfirmationStep;
            _contextRepository.Save(context);

            return new ScenarioResult
            {
                Message = "Сохранить задачу в отчет?",
                Keyboard = CreateYesNoKeyboard()
            };
        }

        if (context.Step == WaitConfirmationStep && text.Equals("Да", StringComparison.CurrentCultureIgnoreCase))
        {
            _reportService.AddCompletedTask(user, context.Data[TaskTextKey]);
            _contextRepository.Delete(context.ChatId);

            return new ScenarioResult
            {
                Message = "Задача добавлена в отчет"
            };
        }

        if (context.Step == WaitConfirmationStep && text.Equals("Нет", StringComparison.CurrentCultureIgnoreCase))
        {
            _contextRepository.Delete(context.ChatId);

            return new ScenarioResult
            {
                Message = "Добавление задачи отменено"
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
