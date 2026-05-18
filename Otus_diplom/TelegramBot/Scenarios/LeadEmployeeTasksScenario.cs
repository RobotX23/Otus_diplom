using Otus_diplom.Core.DataAccess;
using Otus_diplom.Core.Entities;
using Otus_diplom.Core.Services;
using Telegram.Bot.Types.ReplyMarkups;
using TaskStatus = Otus_diplom.Core.Entities.TaskStatus;

namespace Otus_diplom.TelegramBot.Scenarios;

/// <summary>
/// Сценарий просмотра задач сотрудников lead.
/// </summary>
public class LeadEmployeeTasksScenario : IScenario
{
    private const string SelectEmployeeStep = "SelectEmployee";
    private const string SelectTaskStep = "SelectTask";
    private const string TaskCardStep = "TaskCard";
    private const string EmployeeIdKey = "EmployeeId";
    private const string EmployeeCallbackPrefix = "admin_tasks:employee:";
    private const string TaskCallbackPrefix = "admin_tasks:task:";
    private const string BackToEmployeesCallback = "admin_tasks:back:employees";
    private const string BackToTasksCallback = "admin_tasks:back:tasks";

    private readonly ITaskService _taskService;
    private readonly IUserRepository _userRepository;
    private readonly IScenarioContextRepository _contextRepository;

    /// <summary>
    /// Создает сценарий просмотра задач сотрудников lead.
    /// </summary>
    public LeadEmployeeTasksScenario(
        ITaskService taskService,
        IUserRepository userRepository,
        IScenarioContextRepository contextRepository)
    {
        _taskService = taskService;
        _userRepository = userRepository;
        _contextRepository = contextRepository;
    }

    /// <summary>
    /// Проверяет, может ли сценарий обработать указанный тип.
    /// </summary>
    public bool CanHandle(ScenarioType scenarioType)
    {
        return scenarioType == ScenarioType.LeadEmployeeTasks;
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
            ScenarioType = ScenarioType.LeadEmployeeTasks,
            Step = SelectEmployeeStep
        };

        _contextRepository.Save(context);
        return CreateEmployeesResult(true);
    }

    /// <summary>
    /// Обрабатывает текстовые сообщения внутри сценария.
    /// </summary>
    public ScenarioResult HandleMessage(ScenarioContext context, User user, string text)
    {
        return new ScenarioResult
        {
            Message = "Выберите действие с помощью кнопок под сообщением."
        };
    }

    /// <summary>
    /// Обрабатывает callback от inline-кнопок сценария.
    /// </summary>
    public ScenarioResult HandleCallback(ScenarioContext context, User user, string callbackData)
    {
        if (callbackData == BackToEmployeesCallback)
        {
            context.Step = SelectEmployeeStep;
            context.Data.Remove(EmployeeIdKey);
            _contextRepository.Save(context);
            return CreateEmployeesResult(true);
        }

        if (callbackData == BackToTasksCallback)
        {
            return ShowEmployeeTasks(context);
        }

        if (context.Step == SelectEmployeeStep &&
            callbackData.StartsWith(EmployeeCallbackPrefix, StringComparison.Ordinal))
        {
            return SelectEmployee(context, callbackData[EmployeeCallbackPrefix.Length..]);
        }

        if (context.Step == SelectTaskStep &&
            callbackData.StartsWith(TaskCallbackPrefix, StringComparison.Ordinal))
        {
            return ShowTaskCard(context, callbackData[TaskCallbackPrefix.Length..]);
        }

        return CreateExpiredResult(context);
    }

    /// <summary>
    /// Сохраняет выбранного сотрудника и показывает его задачи.
    /// </summary>
    private ScenarioResult SelectEmployee(ScenarioContext context, string employeeIdText)
    {
        if (!int.TryParse(employeeIdText, out var employeeId) || _userRepository.GetById(employeeId) is null)
        {
            return CreateExpiredResult(context);
        }

        context.Step = SelectTaskStep;
        context.Data[EmployeeIdKey] = employeeId.ToString();
        _contextRepository.Save(context);
        return ShowEmployeeTasks(context);
    }

    /// <summary>
    /// Показывает список задач выбранного сотрудника.
    /// </summary>
    private ScenarioResult ShowEmployeeTasks(ScenarioContext context)
    {
        if (!context.Data.TryGetValue(EmployeeIdKey, out var employeeIdText) ||
            !int.TryParse(employeeIdText, out var employeeId))
        {
            return CreateExpiredResult(context);
        }

        var employee = _userRepository.GetById(employeeId);
        if (employee is null)
        {
            return CreateExpiredResult(context);
        }

        var tasks = _taskService.GetEmployeeTasks(employee)
            .OrderBy(task => task.Deadline)
            .ThenBy(task => task.Id)
            .ToList();

        context.Step = SelectTaskStep;
        _contextRepository.Save(context);

        if (tasks.Count == 0)
        {
            return new ScenarioResult
            {
                Message = $"У сотрудника {employee.FullName} нет задач.",
                Keyboard = CreateBackToEmployeesKeyboard(),
                EditCurrentMessage = true
            };
        }

        return new ScenarioResult
        {
            Message = $"Выберите задачу сотрудника {employee.FullName}",
            Keyboard = CreateTasksKeyboard(tasks),
            EditCurrentMessage = true
        };
    }

    /// <summary>
    /// Показывает информационную карточку задачи.
    /// </summary>
    private ScenarioResult ShowTaskCard(ScenarioContext context, string taskIdText)
    {
        if (!int.TryParse(taskIdText, out var taskId) ||
            !context.Data.TryGetValue(EmployeeIdKey, out var employeeIdText) ||
            !int.TryParse(employeeIdText, out var employeeId))
        {
            return CreateExpiredResult(context);
        }

        var employee = _userRepository.GetById(employeeId);
        if (employee is null)
        {
            return CreateExpiredResult(context);
        }

        var task = _taskService.GetEmployeeTasks(employee)
            .FirstOrDefault(item => item.Id == taskId);
        if (task is null)
        {
            return CreateExpiredResult(context);
        }

        context.Step = TaskCardStep;
        _contextRepository.Save(context);

        return new ScenarioResult
        {
            Message = FormatTaskCard(employee, task),
            Keyboard = CreateBackToTasksKeyboard(),
            EditCurrentMessage = true
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
        var rows = employees
            .Select(employee => new[]
            {
                InlineKeyboardButton.WithCallbackData(employee.FullName, $"{EmployeeCallbackPrefix}{employee.Id}")
            })
            .ToList();

        return new InlineKeyboardMarkup(rows);
    }

    /// <summary>
    /// Создает клавиатуру со списком задач.
    /// </summary>
    private static InlineKeyboardMarkup CreateTasksKeyboard(List<EmployeeTask> tasks)
    {
        var rows = tasks
            .Select(task => new[]
            {
                InlineKeyboardButton.WithCallbackData(task.Title, $"{TaskCallbackPrefix}{task.Id}")
            })
            .ToList();

        rows.Add(new[] { InlineKeyboardButton.WithCallbackData("Назад", BackToEmployeesCallback) });
        return new InlineKeyboardMarkup(rows);
    }

    /// <summary>
    /// Создает клавиатуру возврата к списку сотрудников.
    /// </summary>
    private static InlineKeyboardMarkup CreateBackToEmployeesKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Назад", BackToEmployeesCallback) }
        });
    }

    /// <summary>
    /// Создает клавиатуру возврата к списку задач.
    /// </summary>
    private static InlineKeyboardMarkup CreateBackToTasksKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Назад", BackToTasksCallback) }
        });
    }

    /// <summary>
    /// Формирует карточку задачи.
    /// </summary>
    private static string FormatTaskCard(User employee, EmployeeTask task)
    {
        var text =
            "Карточка задачи\n" +
            $"Сотрудник: {employee.FullName}\n" +
            $"Задача: {task.Title}\n" +
            $"Статус: {GetTaskStatusName(task.Status)}\n" +
            $"Срок: {task.Deadline:dd.MM.yyyy}";

        if (task.Status == TaskStatus.Closed)
        {
            text += $"\nКомментарий: {task.ClosingComment}";
        }

        return text;
    }

    /// <summary>
    /// Возвращает русское название статуса задачи.
    /// </summary>
    private static string GetTaskStatusName(TaskStatus status)
    {
        return status switch
        {
            TaskStatus.Open => "открыто",
            TaskStatus.InProgress => "в работе",
            TaskStatus.Closed => "закрыто",
            _ => "неизвестно"
        };
    }

    /// <summary>
    /// Возвращает сообщение об устаревшем действии.
    /// </summary>
    private ScenarioResult CreateExpiredResult(ScenarioContext context)
    {
        _contextRepository.Delete(context.ChatId);
        return new ScenarioResult
        {
            Message = "Команда устарела. Откройте задачи сотрудника заново.",
            EditCurrentMessage = true
        };
    }
}
