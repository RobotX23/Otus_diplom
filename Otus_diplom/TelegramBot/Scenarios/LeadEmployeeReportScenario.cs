using Otus_diplom.Core.DataAccess;
using Otus_diplom.Core.Entities;
using Otus_diplom.Core.Services;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_diplom.TelegramBot.Scenarios;

/// <summary>
/// Сценарий просмотра отчета сотрудника lead.
/// </summary>
public class LeadEmployeeReportScenario : IScenario
{
    private const string SelectEmployeeStep = "SelectEmployee";
    private const string ReportCardStep = "ReportCard";
    private const string EmployeeCallbackPrefix = "lead_report:employee:";
    private const string BackToEmployeesCallback = "lead_report:back:employees";

    private readonly IReportService _reportService;
    private readonly IUserRepository _userRepository;
    private readonly IScenarioContextRepository _contextRepository;

    /// <summary>
    /// Создает сценарий просмотра отчета сотрудника lead.
    /// </summary>
    public LeadEmployeeReportScenario(
        IReportService reportService,
        IUserRepository userRepository,
        IScenarioContextRepository contextRepository)
    {
        _reportService = reportService;
        _userRepository = userRepository;
        _contextRepository = contextRepository;
    }

    /// <summary>
    /// Проверяет, может ли сценарий обработать указанный тип.
    /// </summary>
    public bool CanHandle(ScenarioType scenarioType)
    {
        return scenarioType == ScenarioType.LeadEmployeeReport;
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
            ScenarioType = ScenarioType.LeadEmployeeReport,
            Step = SelectEmployeeStep
        };

        _contextRepository.Save(context);
        return CreateEmployeesResult(false);
    }

    /// <summary>
    /// Обрабатывает текстовые сообщения внутри сценария.
    /// </summary>
    public ScenarioResult HandleMessage(ScenarioContext context, User user, string text)
    {
        return new ScenarioResult
        {
            Message = "Выберите сотрудника с помощью кнопок под сообщением."
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
            _contextRepository.Save(context);
            return CreateEmployeesResult(true);
        }

        if (context.Step == SelectEmployeeStep &&
            callbackData.StartsWith(EmployeeCallbackPrefix, StringComparison.Ordinal))
        {
            return ShowEmployeeReport(context, callbackData[EmployeeCallbackPrefix.Length..]);
        }

        return CreateExpiredResult(context);
    }

    /// <summary>
    /// Показывает отчет выбранного сотрудника.
    /// </summary>
    private ScenarioResult ShowEmployeeReport(ScenarioContext context, string employeeIdText)
    {
        if (!int.TryParse(employeeIdText, out var employeeId))
        {
            return CreateExpiredResult(context);
        }

        var employee = _userRepository.GetById(employeeId);
        if (employee is null)
        {
            return CreateExpiredResult(context);
        }

        context.Step = ReportCardStep;
        _contextRepository.Save(context);

        var report = _reportService.GetTodayReport(employee);
        return new ScenarioResult
        {
            Message = report is null || !report.IsSent
                ? $"У сотрудника {employee.FullName} сегодня еще нет отчета."
                : FormatEmployeeReport(employee, report),
            Keyboard = CreateBackKeyboard(),
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
            Message = "Выберите сотрудника для просмотра отчета",
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
    /// Создает клавиатуру возврата к списку сотрудников.
    /// </summary>
    private static InlineKeyboardMarkup CreateBackKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Назад", BackToEmployeesCallback) }
        });
    }

    /// <summary>
    /// Формирует текст отчета сотрудника.
    /// </summary>
    private static string FormatEmployeeReport(User employee, DailyReport report)
    {
        return
            $"Отчет сотрудника {employee.FullName} за сегодня:\n" +
            $"Отправлен: {(report.IsSent ? "да" : "нет")}\n" +
            "Выполнено:\n" +
            FormatNumberedList(report.CompletedTasks) +
            "Проблемы:\n" +
            FormatNumberedList(report.Blocks);
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
            Message = "Команда устарела. Откройте отчет сотрудника заново.",
            EditCurrentMessage = true
        };
    }
}
