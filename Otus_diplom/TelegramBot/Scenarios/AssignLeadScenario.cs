using Otus_diplom.Core.Entities;
using Otus_diplom.Core.Services;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_diplom.TelegramBot.Scenarios;

/// <summary>
/// Сценарий назначения нового lead администратором.
/// </summary>
public class AssignLeadScenario : IScenario
{
    private const string WaitUserStep = "WaitUser";
    private const string WaitConfirmationStep = "WaitConfirmation";
    private const string UserIdKey = "UserId";
    private const string UserNameKey = "UserName";
    private const string SelectUserCallbackPrefix = "assign_lead:user:";
    private const string ConfirmCallbackData = "assign_lead:yes";
    private const string CancelCallbackData = "assign_lead:no";

    private readonly IUserService _userService;
    private readonly IScenarioContextRepository _contextRepository;

    /// <summary>
    /// Создает сценарий назначения lead.
    /// </summary>
    public AssignLeadScenario(IUserService userService, IScenarioContextRepository contextRepository)
    {
        _userService = userService;
        _contextRepository = contextRepository;
    }

    /// <summary>
    /// Проверяет, может ли сценарий обработать указанный тип.
    /// </summary>
    public bool CanHandle(ScenarioType scenarioType)
    {
        return scenarioType == ScenarioType.AssignLead;
    }

    /// <summary>
    /// Запускает сценарий назначения lead.
    /// </summary>
    public ScenarioResult Start(long chatId, User user)
    {
        var candidates = _userService.GetLeadCandidates(user);
        if (candidates.Count == 0)
        {
            return new ScenarioResult
            {
                Message = "Нет сотрудников для назначения lead."
            };
        }

        var context = new ScenarioContext
        {
            ChatId = chatId,
            UserId = user.Id,
            ScenarioType = ScenarioType.AssignLead,
            Step = WaitUserStep
        };

        _contextRepository.Save(context);
        return new ScenarioResult
        {
            Message = "Выберите сотрудника для назначения lead",
            Keyboard = CreateUsersKeyboard(candidates)
        };
    }

    /// <summary>
    /// Обрабатывает сообщение пользователя внутри сценария.
    /// </summary>
    public ScenarioResult HandleMessage(ScenarioContext context, User user, string text)
    {
        return new ScenarioResult
        {
            Message = "Выберите действие с помощью кнопок под сообщением."
        };
    }

    /// <summary>
    /// Обрабатывает callback от inline-кнопки внутри сценария.
    /// </summary>
    public ScenarioResult HandleCallback(ScenarioContext context, User user, string callbackData)
    {
        if (context.Step == WaitUserStep && callbackData.StartsWith(SelectUserCallbackPrefix, StringComparison.Ordinal))
        {
            return SelectUser(context, user, callbackData);
        }

        if (context.Step == WaitConfirmationStep && callbackData == ConfirmCallbackData)
        {
            var lead = _userService.AssignLead(user, int.Parse(context.Data[UserIdKey]));
            _contextRepository.Delete(context.ChatId);

            return new ScenarioResult
            {
                Message = $"lead {lead.FullName} назначен",
                EditCurrentMessage = true
            };
        }

        if (context.Step == WaitConfirmationStep && callbackData == CancelCallbackData)
        {
            _contextRepository.Delete(context.ChatId);

            return new ScenarioResult
            {
                Message = "Команда отменена",
                EditCurrentMessage = true
            };
        }

        _contextRepository.Delete(context.ChatId);
        return new ScenarioResult
        {
            Message = "Команда устарела. Запустите назначение lead заново.",
            EditCurrentMessage = true
        };
    }

    /// <summary>
    /// Выбирает сотрудника и показывает подтверждение.
    /// </summary>
    private ScenarioResult SelectUser(ScenarioContext context, User admin, string callbackData)
    {
        var userIdText = callbackData[SelectUserCallbackPrefix.Length..];
        if (!int.TryParse(userIdText, out var userId))
        {
            return new ScenarioResult
            {
                Message = "Не удалось определить сотрудника.",
                EditCurrentMessage = true
            };
        }

        var selectedUser = _userService.GetLeadCandidates(admin).FirstOrDefault(user => user.Id == userId);
        if (selectedUser is null)
        {
            return new ScenarioResult
            {
                Message = "Сотрудник не найден. Запустите назначение lead заново.",
                EditCurrentMessage = true
            };
        }

        context.Data[UserIdKey] = selectedUser.Id.ToString();
        context.Data[UserNameKey] = selectedUser.FullName;
        context.Step = WaitConfirmationStep;
        _contextRepository.Save(context);

        return new ScenarioResult
        {
            Message = $"Назначить {selectedUser.FullName} lead?",
            Keyboard = CreateYesNoKeyboard(),
            EditCurrentMessage = true
        };
    }

    /// <summary>
    /// Создает inline-клавиатуру со списком сотрудников.
    /// </summary>
    private static InlineKeyboardMarkup CreateUsersKeyboard(List<User> users)
    {
        return new InlineKeyboardMarkup(users.Select(user => new[]
        {
            InlineKeyboardButton.WithCallbackData(FormatUserButton(user), $"{SelectUserCallbackPrefix}{user.Id}")
        }));
    }

    /// <summary>
    /// Создает inline-клавиатуру подтверждения действия.
    /// </summary>
    private static InlineKeyboardMarkup CreateYesNoKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Да", ConfirmCallbackData),
                InlineKeyboardButton.WithCallbackData("Нет", CancelCallbackData)
            }
        });
    }

    /// <summary>
    /// Формирует текст кнопки пользователя.
    /// </summary>
    private static string FormatUserButton(User user)
    {
        return $"{user.FullName}";
    }
}
