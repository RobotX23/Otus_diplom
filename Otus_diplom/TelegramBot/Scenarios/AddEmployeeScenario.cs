using Otus_diplom.Core.Entities;
using Otus_diplom.Core.Services;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_diplom.TelegramBot.Scenarios;

/// <summary>
/// Сценарий добавления нового сотрудника администратором.
/// </summary>
public class AddEmployeeScenario : IScenario
{
    private const string WaitUsernameStep = "WaitUsername";
    private const string WaitConfirmationStep = "WaitConfirmation";
    private const string UsernameKey = "TelegramUsername";

    private readonly IUserService _userService;
    private readonly IScenarioContextRepository _contextRepository;

    /// <summary>
    /// Создает сценарий добавления сотрудника.
    /// </summary>
    public AddEmployeeScenario(IUserService userService, IScenarioContextRepository contextRepository)
    {
        _userService = userService;
        _contextRepository = contextRepository;
    }

    /// <summary>
    /// Проверяет, может ли сценарий обработать указанный тип.
    /// </summary>
    public bool CanHandle(ScenarioType scenarioType)
    {
        return scenarioType == ScenarioType.AddEmployee;
    }

    /// <summary>
    /// Запускает сценарий добавления сотрудника.
    /// </summary>
    public ScenarioResult Start(long chatId, User user)
    {
        var context = new ScenarioContext
        {
            ChatId = chatId,
            UserId = user.Id,
            ScenarioType = ScenarioType.AddEmployee,
            Step = WaitUsernameStep
        };

        _contextRepository.Save(context);
        return new ScenarioResult
        {
            Message = "Введите Username Телеграммы"
        };
    }

    /// <summary>
    /// Обрабатывает сообщение пользователя внутри сценария.
    /// </summary>
    public ScenarioResult HandleMessage(ScenarioContext context, User user, string text)
    {
        if (context.Step == WaitUsernameStep)
        {
            var telegramUsername = NormalizeTelegramUsername(text);
            context.Data[UsernameKey] = telegramUsername;
            context.Step = WaitConfirmationStep;
            _contextRepository.Save(context);

            return new ScenarioResult
            {
                Message = "Сохранить изменения?",
                Keyboard = CreateYesNoKeyboard()
            };
        }

        if (context.Step == WaitConfirmationStep && text.Equals("Да", StringComparison.CurrentCultureIgnoreCase))
        {
            var employee = _userService.AddEmployeeByTelegramUsername(user, context.Data[UsernameKey]);
            _contextRepository.Delete(context.ChatId);

            return new ScenarioResult
            {
                Message = $"Пользователь {employee.FullName} сохранён"
            };
        }

        if (context.Step == WaitConfirmationStep && text.Equals("Нет", StringComparison.CurrentCultureIgnoreCase))
        {
            _contextRepository.Delete(context.ChatId);

            return new ScenarioResult
            {
                Message = "Создание пользователя прервано"
            };
        }

        return new ScenarioResult
        {
            Message = "Нажмите Да или Нет.",
            Keyboard = CreateYesNoKeyboard()
        };
    }

    /// <summary>
    /// Приводит username Telegram к единому виду.
    /// </summary>
    private static string NormalizeTelegramUsername(string text)
    {
        return text.Trim().TrimStart('@');
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
