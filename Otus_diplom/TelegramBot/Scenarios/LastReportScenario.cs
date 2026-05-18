using Otus_diplom.Core.Entities;
using Otus_diplom.Core.Services;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_diplom.TelegramBot.Scenarios;

/// <summary>
/// Сценарий просмотра последнего отчета сотрудника.
/// </summary>
public class LastReportScenario : IScenario
{
    private const string SelectSectionStep = "SelectSection";
    private const string SelectTaskStep = "SelectTask";
    private const string SelectedTaskStep = "SelectedTask";
    private const string SelectBlockStep = "SelectBlock";
    private const string SelectedBlockStep = "SelectedBlock";
    private const string ConfirmDeleteTaskStep = "ConfirmDeleteTask";
    private const string ConfirmDeleteBlockStep = "ConfirmDeleteBlock";
    private const string TaskIndexKey = "TaskIndex";
    private const string BlockIndexKey = "BlockIndex";
    private const string SelectTasksCallback = "last_report:tasks";
    private const string SelectBlocksCallback = "last_report:blocks";
    private const string SelectTaskCallbackPrefix = "last_report:task:";
    private const string SelectBlockCallbackPrefix = "last_report:block:";
    private const string DeleteTaskCallback = "last_report:delete";
    private const string DeleteBlockCallback = "last_report:delete_block";
    private const string ConfirmDeleteCallback = "last_report:yes";
    private const string CancelDeleteCallback = "last_report:no";
    private const string BackCallback = "last_report:back";

    private readonly IReportService _reportService;
    private readonly IScenarioContextRepository _contextRepository;

    /// <summary>
    /// Создает сценарий просмотра последнего отчета.
    /// </summary>
    public LastReportScenario(IReportService reportService, IScenarioContextRepository contextRepository)
    {
        _reportService = reportService;
        _contextRepository = contextRepository;
    }

    /// <summary>
    /// Проверяет, может ли сценарий обработать указанный тип.
    /// </summary>
    public bool CanHandle(ScenarioType scenarioType)
    {
        return scenarioType == ScenarioType.LastReport;
    }

    /// <summary>
    /// Запускает сценарий просмотра последнего отчета.
    /// </summary>
    public ScenarioResult Start(long chatId, User user)
    {
        var report = _reportService.GetTodayReport(user);
        if (report is null)
        {
            return new ScenarioResult
            {
                Message = "За сегодня отчет еще не создан."
            };
        }

        if (report.IsSent)
        {
            _contextRepository.Delete(chatId);
            return new ScenarioResult
            {
                Message = FormatSentReport(report)
            };
        }

        var context = new ScenarioContext
        {
            ChatId = chatId,
            UserId = user.Id,
            ScenarioType = ScenarioType.LastReport,
            Step = SelectSectionStep
        };

        _contextRepository.Save(context);
        return CreateSectionResult(false);
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
        if (context.Step == SelectSectionStep && callbackData == SelectTasksCallback)
        {
            return ShowTasks(context, user);
        }

        if (context.Step == SelectSectionStep && callbackData == SelectBlocksCallback)
        {
            return ShowBlocks(context, user);
        }

        if (callbackData == BackCallback)
        {
            return HandleBack(context, user);
        }

        if (context.Step == SelectTaskStep && callbackData.StartsWith(SelectTaskCallbackPrefix, StringComparison.Ordinal))
        {
            return ShowTask(context, user, callbackData[SelectTaskCallbackPrefix.Length..]);
        }

        if (context.Step == SelectBlockStep && callbackData.StartsWith(SelectBlockCallbackPrefix, StringComparison.Ordinal))
        {
            return ShowBlock(context, user, callbackData[SelectBlockCallbackPrefix.Length..]);
        }

        if (context.Step == SelectedTaskStep && callbackData == DeleteTaskCallback)
        {
            context.Step = ConfirmDeleteTaskStep;
            _contextRepository.Save(context);
            return new ScenarioResult
            {
                Message = "Вы точно хотите удалить задачу?",
                Keyboard = CreateConfirmKeyboard(),
                EditCurrentMessage = true
            };
        }

        if (context.Step == SelectedBlockStep && callbackData == DeleteBlockCallback)
        {
            context.Step = ConfirmDeleteBlockStep;
            _contextRepository.Save(context);
            return new ScenarioResult
            {
                Message = "Вы точно хотите удалить проблему?",
                Keyboard = CreateConfirmKeyboard(),
                EditCurrentMessage = true
            };
        }

        if (context.Step == ConfirmDeleteTaskStep && callbackData == ConfirmDeleteCallback)
        {
            var taskIndex = int.Parse(context.Data[TaskIndexKey]);
            var isDeleted = _reportService.RemoveCompletedTask(user, taskIndex);
            _contextRepository.Delete(context.ChatId);
            return new ScenarioResult
            {
                Message = isDeleted ? "Задача удалена из отчета" : "Не удалось удалить задачу",
                EditCurrentMessage = true
            };
        }

        if (context.Step == ConfirmDeleteBlockStep && callbackData == ConfirmDeleteCallback)
        {
            var blockIndex = int.Parse(context.Data[BlockIndexKey]);
            var isDeleted = _reportService.RemoveBlock(user, blockIndex);
            _contextRepository.Delete(context.ChatId);
            return new ScenarioResult
            {
                Message = isDeleted ? "Проблема удалена из отчета" : "Не удалось удалить проблему",
                EditCurrentMessage = true
            };
        }

        if ((context.Step == ConfirmDeleteTaskStep || context.Step == ConfirmDeleteBlockStep) &&
            callbackData == CancelDeleteCallback)
        {
            _contextRepository.Delete(context.ChatId);
            return new ScenarioResult
            {
                Message = "Удаление отменено",
                EditCurrentMessage = true
            };
        }

        return CreateExpiredResult(context);
    }

    /// <summary>
    /// Показывает список выполненных задач.
    /// </summary>
    private ScenarioResult ShowTasks(ScenarioContext context, User user)
    {
        var report = _reportService.GetTodayReport(user);
        if (report is null || report.IsSent)
        {
            return CreateExpiredResult(context);
        }

        context.Step = SelectTaskStep;
        _contextRepository.Save(context);
        return CreateTaskListResult(report, true);
    }

    /// <summary>
    /// Показывает список проблем.
    /// </summary>
    private ScenarioResult ShowBlocks(ScenarioContext context, User user)
    {
        var report = _reportService.GetTodayReport(user);
        if (report is null || report.IsSent)
        {
            return CreateExpiredResult(context);
        }

        context.Step = SelectBlockStep;
        _contextRepository.Save(context);
        return CreateBlockListResult(report, true);
    }

    /// <summary>
    /// Показывает выбранную задачу.
    /// </summary>
    private ScenarioResult ShowTask(ScenarioContext context, User user, string taskIndexText)
    {
        if (!int.TryParse(taskIndexText, out var taskIndex))
        {
            return CreateExpiredResult(context);
        }

        var report = _reportService.GetTodayReport(user);
        if (report is null || report.IsSent || taskIndex < 0 || taskIndex >= report.CompletedTasks.Count)
        {
            return CreateExpiredResult(context);
        }

        context.Step = SelectedTaskStep;
        context.Data[TaskIndexKey] = taskIndex.ToString();
        _contextRepository.Save(context);

        return new ScenarioResult
        {
            Message = $"Задача:\n{report.CompletedTasks[taskIndex]}",
            Keyboard = CreateTaskActionKeyboard(),
            EditCurrentMessage = true
        };
    }

    /// <summary>
    /// Показывает выбранную проблему.
    /// </summary>
    private ScenarioResult ShowBlock(ScenarioContext context, User user, string blockIndexText)
    {
        if (!int.TryParse(blockIndexText, out var blockIndex))
        {
            return CreateExpiredResult(context);
        }

        var report = _reportService.GetTodayReport(user);
        if (report is null || report.IsSent || blockIndex < 0 || blockIndex >= report.Blocks.Count)
        {
            return CreateExpiredResult(context);
        }

        context.Step = SelectedBlockStep;
        context.Data[BlockIndexKey] = blockIndex.ToString();
        _contextRepository.Save(context);

        return new ScenarioResult
        {
            Message = $"Проблема:\n{report.Blocks[blockIndex]}",
            Keyboard = CreateBlockActionKeyboard(),
            EditCurrentMessage = true
        };
    }

    /// <summary>
    /// Возвращает пользователя на предыдущий шаг сценария.
    /// </summary>
    private ScenarioResult HandleBack(ScenarioContext context, User user)
    {
        var report = _reportService.GetTodayReport(user);
        if (report is null || report.IsSent)
        {
            return CreateExpiredResult(context);
        }

        if (context.Step == SelectTaskStep || context.Step == SelectBlockStep)
        {
            context.Step = SelectSectionStep;
            _contextRepository.Save(context);
            return CreateSectionResult(true);
        }

        if (context.Step == SelectedTaskStep)
        {
            context.Step = SelectTaskStep;
            _contextRepository.Save(context);
            return CreateTaskListResult(report, true);
        }

        if (context.Step == SelectedBlockStep)
        {
            context.Step = SelectBlockStep;
            _contextRepository.Save(context);
            return CreateBlockListResult(report, true);
        }

        return CreateSectionResult(true);
    }

    /// <summary>
    /// Создает результат с выбором раздела неотправленного отчета.
    /// </summary>
    private static ScenarioResult CreateSectionResult(bool editCurrentMessage)
    {
        return new ScenarioResult
        {
            Message = "Выберите список последнего отчета",
            Keyboard = CreateSectionKeyboard(),
            EditCurrentMessage = editCurrentMessage
        };
    }

    /// <summary>
    /// Создает результат со списком задач неотправленного отчета.
    /// </summary>
    private static ScenarioResult CreateTaskListResult(DailyReport report, bool editCurrentMessage)
    {
        if (report.CompletedTasks.Count == 0)
        {
            return new ScenarioResult
            {
                Message = "В отчете пока нет выполненных задач.",
                Keyboard = CreateBackKeyboard(),
                EditCurrentMessage = editCurrentMessage
            };
        }

        return new ScenarioResult
        {
            Message = "Выберите задачу из последнего отчета",
            Keyboard = CreateTasksKeyboard(report.CompletedTasks),
            EditCurrentMessage = editCurrentMessage
        };
    }

    /// <summary>
    /// Создает результат со списком проблем неотправленного отчета.
    /// </summary>
    private static ScenarioResult CreateBlockListResult(DailyReport report, bool editCurrentMessage)
    {
        if (report.Blocks.Count == 0)
        {
            return new ScenarioResult
            {
                Message = "В отчете пока нет проблем.",
                Keyboard = CreateBackKeyboard(),
                EditCurrentMessage = editCurrentMessage
            };
        }

        return new ScenarioResult
        {
            Message = "Выберите проблему из последнего отчета",
            Keyboard = CreateBlocksKeyboard(report.Blocks),
            EditCurrentMessage = editCurrentMessage
        };
    }

    /// <summary>
    /// Формирует текст отправленного отчета.
    /// </summary>
    private static string FormatSentReport(DailyReport report)
    {
        return
            "Ваш последний отчет:\n" +
            $"Дата: {report.Date:dd.MM.yyyy}\n" +
            "Выполнено:\n" +
            FormatNumberedList(report.CompletedTasks) + "\n" +
            "Проблемы:\n" +
            FormatNumberedList(report.Blocks);
    }

    /// <summary>
    /// Создает клавиатуру со списком задач отчета.
    /// </summary>
    private static InlineKeyboardMarkup CreateTasksKeyboard(List<string> tasks)
    {
        var rows = tasks.Select((task, index) => new[]
            {
                InlineKeyboardButton.WithCallbackData($"{index + 1}. {task}", $"{SelectTaskCallbackPrefix}{index}")
            })
            .ToList();

        rows.Add(new[] { InlineKeyboardButton.WithCallbackData("Назад", BackCallback) });

        return new InlineKeyboardMarkup(rows);
    }

    /// <summary>
    /// Создает клавиатуру с выбором раздела отчета.
    /// </summary>
    private static InlineKeyboardMarkup CreateSectionKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Задачи", SelectTasksCallback),
                InlineKeyboardButton.WithCallbackData("Проблемы", SelectBlocksCallback)
            }
        });
    }

    /// <summary>
    /// Создает клавиатуру со списком проблем отчета.
    /// </summary>
    private static InlineKeyboardMarkup CreateBlocksKeyboard(List<string> blocks)
    {
        var rows = blocks.Select((block, index) => new[]
            {
                InlineKeyboardButton.WithCallbackData($"{index + 1}. {block}", $"{SelectBlockCallbackPrefix}{index}")
            })
            .ToList();

        rows.Add(new[] { InlineKeyboardButton.WithCallbackData("Назад", BackCallback) });

        return new InlineKeyboardMarkup(rows);
    }

    /// <summary>
    /// Создает клавиатуру действий по выбранной задаче.
    /// </summary>
    private static InlineKeyboardMarkup CreateTaskActionKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Удалить задачу", DeleteTaskCallback) },
            new[] { InlineKeyboardButton.WithCallbackData("Назад", BackCallback) }
        });
    }

    /// <summary>
    /// Создает клавиатуру действий по выбранной проблеме.
    /// </summary>
    private static InlineKeyboardMarkup CreateBlockActionKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Удалить проблему", DeleteBlockCallback) },
            new[] { InlineKeyboardButton.WithCallbackData("Назад", BackCallback) }
        });
    }

    /// <summary>
    /// Создает клавиатуру с кнопкой назад.
    /// </summary>
    private static InlineKeyboardMarkup CreateBackKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Назад", BackCallback) }
        });
    }

    /// <summary>
    /// Создает клавиатуру подтверждения удаления.
    /// </summary>
    private static InlineKeyboardMarkup CreateConfirmKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Да", ConfirmDeleteCallback),
                InlineKeyboardButton.WithCallbackData("Нет", CancelDeleteCallback)
            }
        });
    }

    /// <summary>
    /// Форматирует список строк с номерами.
    /// </summary>
    private static string FormatNumberedList(List<string> values)
    {
        if (values.Count == 0)
        {
            return "нет\n";
        }

        return string.Join('\n', values.Select((value, index) => $"{index + 1}. {value}")) + "\n";
    }

    /// <summary>
    /// Возвращает сообщение об устаревшем действии.
    /// </summary>
    private ScenarioResult CreateExpiredResult(ScenarioContext context)
    {
        _contextRepository.Delete(context.ChatId);
        return new ScenarioResult
        {
            Message = "Команда устарела. Откройте последний отчет заново.",
            EditCurrentMessage = true
        };
    }
}
