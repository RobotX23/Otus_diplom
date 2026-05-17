using Otus_diplom.Core.Entities;
using Otus_diplom.Core.Services;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_diplom.TelegramBot.Scenarios;

/// <summary>
/// Сценарий удаления сотрудника администратором.
/// </summary>
public class DeleteEmployeeScenario : IScenario
{
    private const string WaitUserStep = "WaitUser";
    private const string WaitConfirmationStep = "WaitConfirmation";
    private const string UserIdKey = "UserId";
    private const string UserNameKey = "UserName";
    private const string SelectUserCallbackPrefix = "delete_employee:user:";
    private const string ConfirmCallbackData = "delete_employee:yes";
    private const string CancelCallbackData = "delete_employee:no";

    private readonly IUserService _userService;
    private readonly IScenarioContextRepository _contextRepository;

    /// <summary>
    /// Создает сценарий удаления сотрудника.
    /// </summary>
    public DeleteEmployeeScenario(IUserService userService, IScenarioContextRepository contextRepository)
    {
        _userService = userService;
        _contextRepository = contextRepository;
    }

    /// <summary>
    /// Проверяет, может ли сценарий обработать указанный тип.
    /// </summary>
    public bool CanHandle(ScenarioType scenarioType)
    {
        return scenarioType == ScenarioType.DeleteEmployee;
    }

    /// <summary>
    /// Запускает сценарий удаления сотрудника.
    /// </summary>
    public ScenarioResult Start(long chatId, User user)
    {
        var candidates = _userService.GetDeleteCandidates(user);
        if (candidates.Count == 0)
        {
            return new ScenarioResult
            {
                Message = "Нет сотрудников для удаления."
            };
        }

        var context = new ScenarioContext
        {
            ChatId = chatId,
            UserId = user.Id,
            ScenarioType = ScenarioType.DeleteEmployee,
            Step = WaitUserStep
        };

        _contextRepository.Save(context);
        return new ScenarioResult
        {
            Message = "Выберите сотрудника для удаления",
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
            var deletedUser = _userService.DeleteEmployee(user, int.Parse(context.Data[UserIdKey]));
            _contextRepository.Delete(context.ChatId);

            return new ScenarioResult
            {
                Message = $"Сотрудник {deletedUser.FullName} удален",
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
            Message = "Команда устарела. Запустите удаление сотрудника заново.",
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

        var selectedUser = _userService.GetDeleteCandidates(admin).FirstOrDefault(user => user.Id == userId);
        if (selectedUser is null)
        {
            return new ScenarioResult
            {
                Message = "Сотрудник не найден. Запустите удаление сотрудника заново.",
                EditCurrentMessage = true
            };
        }

        context.Data[UserIdKey] = selectedUser.Id.ToString();
        context.Data[UserNameKey] = selectedUser.FullName;
        context.Step = WaitConfirmationStep;
        _contextRepository.Save(context);

        return new ScenarioResult
        {
            Message = $"Удалить сотрудника {selectedUser.FullName}?",
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
            InlineKeyboardButton.WithCallbackData(user.FullName, $"{SelectUserCallbackPrefix}{user.Id}")
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
}
