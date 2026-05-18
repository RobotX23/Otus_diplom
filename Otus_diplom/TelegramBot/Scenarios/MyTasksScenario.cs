using Otus_diplom.Core.Entities;
using Otus_diplom.Core.Services;
using Telegram.Bot.Types.ReplyMarkups;
using TaskStatus = Otus_diplom.Core.Entities.TaskStatus;

namespace Otus_diplom.TelegramBot.Scenarios;

/// <summary>
/// Сценарий просмотра и изменения задач сотрудника.
/// </summary>
public class MyTasksScenario : IScenario
{
    private const string SelectStatusStep = "SelectStatus";
    private const string SelectDateStep = "SelectDate";
    private const string SelectTaskStep = "SelectTask";
    private const string WaitCloseCommentStep = "WaitCloseComment";
    private const string WaitCloseConfirmationStep = "WaitCloseConfirmation";
    private const string StatusKey = "Status";
    private const string DateKey = "Date";
    private const string TaskIdKey = "TaskId";
    private const string CommentKey = "Comment";
    private const string AllStatus = "All";
    private const string StatusCallbackPrefix = "my_tasks:status:";
    private const string DateCallbackPrefix = "my_tasks:date:";
    private const string TaskCallbackPrefix = "my_tasks:task:";
    private const string StartCallbackPrefix = "my_tasks:start:";
    private const string CloseCallbackPrefix = "my_tasks:close:";
    private const string BackToStatusesCallback = "my_tasks:back:statuses";
    private const string BackToDatesCallback = "my_tasks:back:dates";
    private const string BackToTasksCallback = "my_tasks:back:tasks";

    private readonly ITaskService _taskService;
    private readonly IScenarioContextRepository _contextRepository;

    /// <summary>
    /// Создает сценарий просмотра задач сотрудника.
    /// </summary>
    public MyTasksScenario(ITaskService taskService, IScenarioContextRepository contextRepository)
    {
        _taskService = taskService;
        _contextRepository = contextRepository;
    }

    /// <summary>
    /// Проверяет, может ли сценарий обработать указанный тип.
    /// </summary>
    public bool CanHandle(ScenarioType scenarioType)
    {
        return scenarioType == ScenarioType.MyTasks;
    }

    /// <summary>
    /// Запускает сценарий просмотра задач.
    /// </summary>
    public ScenarioResult Start(long chatId, User user)
    {
        var context = new ScenarioContext
        {
            ChatId = chatId,
            UserId = user.Id,
            ScenarioType = ScenarioType.MyTasks,
            Step = SelectStatusStep
        };

        _contextRepository.Save(context);
        return new ScenarioResult
        {
            Message = "Выберите список задач",
            Keyboard = CreateStatusKeyboard()
        };
    }

    /// <summary>
    /// Обрабатывает сообщение пользователя внутри сценария.
    /// </summary>
    public ScenarioResult HandleMessage(ScenarioContext context, User user, string text)
    {
        if (context.Step == WaitCloseCommentStep)
        {
            context.Data[CommentKey] = text.Trim();
            context.Step = WaitCloseConfirmationStep;
            _contextRepository.Save(context);

            return new ScenarioResult
            {
                Message = "Отправить изменения?",
                Keyboard = CreateYesNoKeyboard()
            };
        }

        if (context.Step == WaitCloseConfirmationStep && text.Equals("Да", StringComparison.CurrentCultureIgnoreCase))
        {
            var taskId = int.Parse(context.Data[TaskIdKey]);
            var isClosed = _taskService.CloseTask(user, taskId, context.Data[CommentKey]);
            _contextRepository.Delete(context.ChatId);

            return new ScenarioResult
            {
                Message = isClosed ? "Задача закрыта" : "Не удалось закрыть задачу"
            };
        }

        if (context.Step == WaitCloseConfirmationStep && text.Equals("Нет", StringComparison.CurrentCultureIgnoreCase))
        {
            _contextRepository.Delete(context.ChatId);
            return new ScenarioResult
            {
                Message = "Закрытие задачи отменено"
            };
        }

        return new ScenarioResult
        {
            Message = "Выберите действие с помощью кнопок."
        };
    }

    /// <summary>
    /// Обрабатывает callback от inline-кнопки внутри сценария.
    /// </summary>
    public ScenarioResult HandleCallback(ScenarioContext context, User user, string callbackData)
    {
        if (callbackData == BackToStatusesCallback)
        {
            context.Step = SelectStatusStep;
            _contextRepository.Save(context);
            return new ScenarioResult
            {
                Message = "Выберите список задач",
                Keyboard = CreateStatusKeyboard(),
                EditCurrentMessage = true
            };
        }

        if (callbackData == BackToDatesCallback)
        {
            return ShowDates(context, user);
        }

        if (callbackData == BackToTasksCallback)
        {
            return ShowTasks(context, user);
        }

        if (context.Step == SelectStatusStep && callbackData.StartsWith(StatusCallbackPrefix, StringComparison.Ordinal))
        {
            context.Data[StatusKey] = callbackData[StatusCallbackPrefix.Length..];
            context.Step = SelectDateStep;
            _contextRepository.Save(context);
            return ShowDates(context, user);
        }

        if (context.Step == SelectDateStep && callbackData.StartsWith(DateCallbackPrefix, StringComparison.Ordinal))
        {
            context.Data[DateKey] = callbackData[DateCallbackPrefix.Length..];
            context.Step = SelectTaskStep;
            _contextRepository.Save(context);
            return ShowTasks(context, user);
        }

        if (context.Step == SelectTaskStep && callbackData.StartsWith(TaskCallbackPrefix, StringComparison.Ordinal))
        {
            return ShowTaskDetails(context, user, callbackData[TaskCallbackPrefix.Length..]);
        }

        if (callbackData.StartsWith(StartCallbackPrefix, StringComparison.Ordinal))
        {
            return StartTask(context, user, callbackData[StartCallbackPrefix.Length..]);
        }

        if (callbackData.StartsWith(CloseCallbackPrefix, StringComparison.Ordinal))
        {
            return AskCloseComment(context, callbackData[CloseCallbackPrefix.Length..]);
        }

        _contextRepository.Delete(context.ChatId);
        return new ScenarioResult
        {
            Message = "Команда устарела. Откройте список задач заново.",
            EditCurrentMessage = true
        };
    }

    /// <summary>
    /// Показывает даты дедлайнов по выбранному статусу.
    /// </summary>
    private ScenarioResult ShowDates(ScenarioContext context, User user)
    {
        context.Step = SelectDateStep;
        _contextRepository.Save(context);

        var dates = GetFilteredTasks(user, context.Data[StatusKey])
            .Select(task => task.Deadline)
            .Distinct()
            .OrderBy(date => date)
            .ToList();

        if (dates.Count == 0)
        {
            return new ScenarioResult
            {
                Message = "Задачи не найдены",
                Keyboard = CreateBackToStatusesKeyboard(),
                EditCurrentMessage = true
            };
        }

        return new ScenarioResult
        {
            Message = "Выберите дату дедлайна",
            Keyboard = CreateDatesKeyboard(dates),
            EditCurrentMessage = true
        };
    }

    /// <summary>
    /// Показывает задачи за выбранную дату.
    /// </summary>
    private ScenarioResult ShowTasks(ScenarioContext context, User user)
    {
        var date = DateOnly.ParseExact(context.Data[DateKey], "yyyy-MM-dd");
        var tasks = GetFilteredTasks(user, context.Data[StatusKey])
            .Where(task => task.Deadline == date)
            .OrderBy(task => task.Id)
            .ToList();

        return new ScenarioResult
        {
            Message = "Выберите задачу",
            Keyboard = CreateTasksKeyboard(tasks),
            EditCurrentMessage = true
        };
    }

    /// <summary>
    /// Показывает выбранную задачу и доступные действия.
    /// </summary>
    private ScenarioResult ShowTaskDetails(ScenarioContext context, User user, string taskIdText)
    {
        if (!int.TryParse(taskIdText, out var taskId))
        {
            return CreateExpiredResult(context);
        }

        var task = _taskService.GetEmployeeTasks(user).FirstOrDefault(task => task.Id == taskId);
        if (task is null)
        {
            return CreateExpiredResult(context);
        }

        context.Data[TaskIdKey] = task.Id.ToString();
        _contextRepository.Save(context);

        return new ScenarioResult
        {
            Message = FormatTask(task),
            Keyboard = CreateTaskActionKeyboard(task),
            EditCurrentMessage = true
        };
    }

    /// <summary>
    /// Переводит задачу в работу.
    /// </summary>
    private ScenarioResult StartTask(ScenarioContext context, User user, string taskIdText)
    {
        if (!int.TryParse(taskIdText, out var taskId))
        {
            return CreateExpiredResult(context);
        }

        var isStarted = _taskService.StartTask(user, taskId);
        _contextRepository.Delete(context.ChatId);
        return new ScenarioResult
        {
            Message = isStarted ? "Задача взята в работу" : "Не удалось взять задачу в работу",
            EditCurrentMessage = true
        };
    }

    /// <summary>
    /// Просит пользователя оставить комментарий для закрытия задачи.
    /// </summary>
    private ScenarioResult AskCloseComment(ScenarioContext context, string taskIdText)
    {
        if (!int.TryParse(taskIdText, out var taskId))
        {
            return CreateExpiredResult(context);
        }

        context.Data[TaskIdKey] = taskId.ToString();
        context.Step = WaitCloseCommentStep;
        _contextRepository.Save(context);

        return new ScenarioResult
        {
            Message = "Оставьте комментарий к задаче",
            EditCurrentMessage = true
        };
    }

    /// <summary>
    /// Возвращает задачи сотрудника с учетом выбранного статуса.
    /// </summary>
    private List<EmployeeTask> GetFilteredTasks(User user, string status)
    {
        var tasks = _taskService.GetEmployeeTasks(user);
        return status switch
        {
            nameof(TaskStatus.Open) => tasks.Where(task => task.Status == TaskStatus.Open).ToList(),
            nameof(TaskStatus.InProgress) => tasks.Where(task => task.Status == TaskStatus.InProgress).ToList(),
            nameof(TaskStatus.Closed) => tasks.Where(task => task.Status == TaskStatus.Closed).ToList(),
            _ => tasks
        };
    }

    /// <summary>
    /// Создает клавиатуру выбора статуса.
    /// </summary>
    private static InlineKeyboardMarkup CreateStatusKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Открытые", $"{StatusCallbackPrefix}{nameof(TaskStatus.Open)}"),
                InlineKeyboardButton.WithCallbackData("В работе", $"{StatusCallbackPrefix}{nameof(TaskStatus.InProgress)}")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Закрытые", $"{StatusCallbackPrefix}{nameof(TaskStatus.Closed)}"),
                InlineKeyboardButton.WithCallbackData("Все задачи", $"{StatusCallbackPrefix}{AllStatus}")
            }
        });
    }

    /// <summary>
    /// Создает клавиатуру выбора даты дедлайна.
    /// </summary>
    private static InlineKeyboardMarkup CreateDatesKeyboard(List<DateOnly> dates)
    {
        var rows = dates
            .Select(date => new[] { InlineKeyboardButton.WithCallbackData(date.ToString("dd.MM.yyyy"), $"{DateCallbackPrefix}{date:yyyy-MM-dd}") })
            .Append(new[] { InlineKeyboardButton.WithCallbackData("Назад", BackToStatusesCallback) });

        return new InlineKeyboardMarkup(rows);
    }

    /// <summary>
    /// Создает клавиатуру выбора задачи.
    /// </summary>
    private static InlineKeyboardMarkup CreateTasksKeyboard(List<EmployeeTask> tasks)
    {
        var rows = tasks
            .Select(task => new[] { InlineKeyboardButton.WithCallbackData(task.Title, $"{TaskCallbackPrefix}{task.Id}") })
            .Append(new[] { InlineKeyboardButton.WithCallbackData("Назад", BackToDatesCallback) });

        return new InlineKeyboardMarkup(rows);
    }

    /// <summary>
    /// Создает клавиатуру действий по задаче.
    /// </summary>
    private static InlineKeyboardMarkup CreateTaskActionKeyboard(EmployeeTask task)
    {
        var rows = new List<InlineKeyboardButton[]>();
        if (task.Status == TaskStatus.Open)
        {
            rows.Add(new[] { InlineKeyboardButton.WithCallbackData("В работу", $"{StartCallbackPrefix}{task.Id}") });
        }
        else if (task.Status == TaskStatus.InProgress)
        {
            rows.Add(new[] { InlineKeyboardButton.WithCallbackData("Закрыть", $"{CloseCallbackPrefix}{task.Id}") });
        }

        rows.Add(new[] { InlineKeyboardButton.WithCallbackData("Назад", BackToTasksCallback) });
        return new InlineKeyboardMarkup(rows);
    }

    /// <summary>
    /// Создает клавиатуру возврата к статусам.
    /// </summary>
    private static InlineKeyboardMarkup CreateBackToStatusesKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Назад", BackToStatusesCallback) }
        });
    }

    /// <summary>
    /// Создает клавиатуру подтверждения закрытия.
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

    /// <summary>
    /// Формирует текст с данными задачи.
    /// </summary>
    private static string FormatTask(EmployeeTask task)
    {
        var text =
            $"Задача: {task.Title}\n" +
            $"Статус: {GetStatusName(task.Status)}\n" +
            $"Дедлайн: {task.Deadline:dd.MM.yyyy}";

        if (task.Status == TaskStatus.Closed && !string.IsNullOrWhiteSpace(task.ClosingComment))
        {
            text += $"\nКомментарий: {task.ClosingComment}";
        }

        return text;
    }

    /// <summary>
    /// Возвращает русское название статуса.
    /// </summary>
    private static string GetStatusName(TaskStatus status)
    {
        return status switch
        {
            TaskStatus.Open => "открыто",
            TaskStatus.InProgress => "в работе",
            TaskStatus.Closed => "закрыта",
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
            Message = "Команда устарела. Откройте список задач заново.",
            EditCurrentMessage = true
        };
    }
}
