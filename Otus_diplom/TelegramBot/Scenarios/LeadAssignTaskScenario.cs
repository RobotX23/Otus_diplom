using Otus_diplom.Core.DataAccess;
using Otus_diplom.Core.Entities;
using Otus_diplom.Core.Services;
using Otus_diplom.TelegramBot.Messaging;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_diplom.TelegramBot.Scenarios;

/// <summary>
/// Сценарий назначения задачи сотруднику lead.
/// </summary>
public class LeadAssignTaskScenario : IScenario
{
    private const string SelectEmployeeStep = "SelectEmployee";
    private const string WaitTaskTextStep = "WaitTaskText";
    private const string WaitDeadlineStep = "WaitDeadline";
    private const string WaitConfirmationStep = "WaitConfirmation";
    private const string EmployeeIdKey = "EmployeeId";
    private const string TaskTextKey = "TaskText";
    private const string DeadlineKey = "Deadline";
    private const string EmployeeCallbackPrefix = "lead_assign_task:employee:";
    private const string BackButtonText = "Назад";

    private readonly ITaskService _taskService;
    private readonly IUserRepository _userRepository;
    private readonly IMessageSender _messageSender;
    private readonly IScenarioContextRepository _contextRepository;

    /// <summary>
    /// Создает сценарий назначения задачи сотруднику.
    /// </summary>
    public LeadAssignTaskScenario(
        ITaskService taskService,
        IUserRepository userRepository,
        IMessageSender messageSender,
        IScenarioContextRepository contextRepository)
    {
        _taskService = taskService;
        _userRepository = userRepository;
        _messageSender = messageSender;
        _contextRepository = contextRepository;
    }

    /// <summary>
    /// Проверяет, может ли сценарий обработать указанный тип.
    /// </summary>
    public bool CanHandle(ScenarioType scenarioType)
    {
        return scenarioType == ScenarioType.LeadAssignTask;
    }

    /// <summary>
    /// Запускает сценарий выбора сотрудника.
    /// </summary>
    public ScenarioResult Start(long chatId, User user)
    {
        var context = new ScenarioContext
        {
            ChatId = chatId,
            UserId = user.Id,
            ScenarioType = ScenarioType.LeadAssignTask,
            Step = SelectEmployeeStep
        };

        _contextRepository.Save(context);
        return CreateEmployeesResult(false);
    }

    /// <summary>
    /// Обрабатывает текстовые сообщения сценария.
    /// </summary>
    public ScenarioResult HandleMessage(ScenarioContext context, User user, string text)
    {
        if (text.Equals(BackButtonText, StringComparison.CurrentCultureIgnoreCase))
        {
            return HandleBack(context);
        }

        if (context.Step == WaitTaskTextStep)
        {
            context.Data[TaskTextKey] = text.Trim();
            context.Step = WaitDeadlineStep;
            _contextRepository.Save(context);

            return new ScenarioResult
            {
                Message = "Введите дедлайн задачи. Пример: 20.05.2026",
                Keyboard = CreateBackKeyboard()
            };
        }

        if (context.Step == WaitDeadlineStep)
        {
            if (!DateOnly.TryParse(text.Trim(), out var deadline))
            {
                return new ScenarioResult
                {
                    Message = "Не удалось распознать дату. Пример: 20.05.2026",
                    Keyboard = CreateBackKeyboard()
                };
            }

            context.Data[DeadlineKey] = deadline.ToString("yyyy-MM-dd");
            context.Step = WaitConfirmationStep;
            _contextRepository.Save(context);

            return new ScenarioResult
            {
                Message = "Отправить задание пользователю?",
                Keyboard = CreateYesNoKeyboard()
            };
        }

        if (context.Step == WaitConfirmationStep && text.Equals("Да", StringComparison.CurrentCultureIgnoreCase))
        {
            return AssignTask(context, user);
        }

        if (context.Step == WaitConfirmationStep && text.Equals("Нет", StringComparison.CurrentCultureIgnoreCase))
        {
            _contextRepository.Delete(context.ChatId);
            return new ScenarioResult
            {
                Message = "Задача не назначена."
            };
        }

        return new ScenarioResult
        {
            Message = "Выберите действие с помощью кнопок."
        };
    }

    /// <summary>
    /// Обрабатывает callback выбора сотрудника.
    /// </summary>
    public ScenarioResult HandleCallback(ScenarioContext context, User user, string callbackData)
    {
        if (context.Step == SelectEmployeeStep &&
            callbackData.StartsWith(EmployeeCallbackPrefix, StringComparison.Ordinal))
        {
            return SelectEmployee(context, callbackData[EmployeeCallbackPrefix.Length..]);
        }

        _contextRepository.Delete(context.ChatId);
        return new ScenarioResult
        {
            Message = "Команда устарела. Запустите назначение задачи заново.",
            EditCurrentMessage = true
        };
    }

    /// <summary>
    /// Сохраняет выбранного сотрудника и просит ввести задачу.
    /// </summary>
    private ScenarioResult SelectEmployee(ScenarioContext context, string employeeIdText)
    {
        if (!int.TryParse(employeeIdText, out var employeeId) || _userRepository.GetById(employeeId) is null)
        {
            _contextRepository.Delete(context.ChatId);
            return new ScenarioResult
            {
                Message = "Сотрудник не найден. Запустите назначение задачи заново.",
                EditCurrentMessage = true
            };
        }

        context.Data[EmployeeIdKey] = employeeId.ToString();
        context.Step = WaitTaskTextStep;
        _contextRepository.Save(context);

        return new ScenarioResult
        {
            Message = "Введите задачу.",
            Keyboard = CreateBackKeyboard(),
            SendNewMessage = true
        };
    }

    /// <summary>
    /// Назначает задачу выбранному сотруднику.
    /// </summary>
    private ScenarioResult AssignTask(ScenarioContext context, User lead)
    {
        var employeeId = int.Parse(context.Data[EmployeeIdKey]);
        var employee = _userRepository.GetById(employeeId);
        if (employee is null)
        {
            _contextRepository.Delete(context.ChatId);
            return new ScenarioResult
            {
                Message = "Сотрудник не найден. Задача не назначена."
            };
        }

        var deadline = DateOnly.Parse(context.Data[DeadlineKey]);
        var task = _taskService.AssignTask(lead, employee, context.Data[TaskTextKey], deadline);
        _contextRepository.Delete(context.ChatId);
        if (employee.TelegramChatId.HasValue)
        {
            _ = SendTaskNotificationAsync(employee.TelegramChatId.Value, task);
        }

        return new ScenarioResult
        {
            Message = $"Задача назначена сотруднику {employee.FullName}."
        };
    }

    /// <summary>
    /// Отправляет сотруднику уведомление о новой задаче.
    /// </summary>
    private async Task SendTaskNotificationAsync(long telegramChatId, EmployeeTask task)
    {
        try
        {
            await _messageSender.SendMessageAsync(telegramChatId,
                "Вам назначена новая задача:\n" +
                $"{task.Title}\n" +
                $"Срок: {task.Deadline:dd.MM.yyyy}\n" +
                "Статус: открыто");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Ошибка отправки уведомления сотруднику о новой задаче: {exception}");
        }
    }

    /// <summary>
    /// Возвращает пользователя на предыдущий шаг.
    /// </summary>
    private ScenarioResult HandleBack(ScenarioContext context)
    {
        if (context.Step == WaitTaskTextStep)
        {
            context.Step = SelectEmployeeStep;
            context.Data.Remove(EmployeeIdKey);
            _contextRepository.Save(context);
            return CreateEmployeesResult(false);
        }

        if (context.Step == WaitDeadlineStep)
        {
            context.Step = WaitTaskTextStep;
            context.Data.Remove(TaskTextKey);
            _contextRepository.Save(context);
            return new ScenarioResult
            {
                Message = "Введите задачу.",
                Keyboard = CreateBackKeyboard()
            };
        }

        return new ScenarioResult
        {
            Message = "Назад доступен на шаге ввода задачи или дедлайна."
        };
    }

    /// <summary>
    /// Создает результат со списком сотрудников.
    /// </summary>
    private ScenarioResult CreateEmployeesResult(bool editCurrentMessage)
    {
        var employees = _userRepository.GetEmployees()
            .OrderBy(employee => employee.FullName)
            .ToList();

        if (employees.Count == 0)
        {
            return new ScenarioResult
            {
                Message = "Сотрудники не найдены.",
                EditCurrentMessage = editCurrentMessage
            };
        }

        return new ScenarioResult
        {
            Message = "Выберите сотрудника",
            Keyboard = CreateEmployeesKeyboard(employees),
            EditCurrentMessage = editCurrentMessage
        };
    }

    /// <summary>
    /// Создает клавиатуру со списком сотрудников.
    /// </summary>
    private static InlineKeyboardMarkup CreateEmployeesKeyboard(List<User> employees)
    {
        var rows = employees.Select(employee => new[]
        {
            InlineKeyboardButton.WithCallbackData(employee.FullName, $"{EmployeeCallbackPrefix}{employee.Id}")
        });

        return new InlineKeyboardMarkup(rows);
    }

    /// <summary>
    /// Создает обычную клавиатуру с кнопкой Назад.
    /// </summary>
    private static ReplyKeyboardMarkup CreateBackKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new[] { new KeyboardButton(BackButtonText) }
        })
        {
            ResizeKeyboard = true,
            IsPersistent = true
        };
    }

    /// <summary>
    /// Создает клавиатуру подтверждения.
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
