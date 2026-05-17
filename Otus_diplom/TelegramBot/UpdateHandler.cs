using Otus_diplom.Core.DataAccess;
using Otus_diplom.Core.Entities;
using Otus_diplom.Core.Exceptions;
using Otus_diplom.Core.Services;
using TaskStatus = Otus_diplom.Core.Entities.TaskStatus;

namespace Otus_diplom.TelegramBot;

/// <summary>
/// Обработчик текстовых Telegram-команд.
/// </summary>
public class UpdateHandler
{
    private readonly IReportService _reportService;
    private readonly ITaskService _taskService;
    private readonly IUserRepository _userRepository;
    private readonly IMessageSender _messageSender;

    /// <summary>
    /// Создает обработчик Telegram-команд.
    /// </summary>
    public UpdateHandler(
        IReportService reportService,
        ITaskService taskService,
        IUserRepository userRepository,
        IMessageSender messageSender)
    {
        _reportService = reportService;
        _taskService = taskService;
        _userRepository = userRepository;
        _messageSender = messageSender;
    }

    /// <summary>
    /// Обрабатывает входящий текст команды.
    /// </summary>
    public void HandleTextMessage(long chatId, string text)
    {
        try
        {
            HandleTextMessageInternal(chatId, text);
        }
        catch (DomainException exception)
        {
            Console.WriteLine($"Ошибка бизнес-логики: {exception.Message}");
            Send(chatId, exception.Message);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Ошибка обработки команды: {exception}");
            Send(chatId, "Произошла ошибка при обработке команды.");
        }
    }

    /// <summary>
    /// Выполняет основную обработку команды.
    /// </summary>
    private void HandleTextMessageInternal(long chatId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Send(chatId, "Введите команду.");
            return;
        }

        var commandText = text.Trim();

        if (commandText == "/start")
        {
            SendStart(chatId);
            return;
        }

        if (commandText == "/help")
        {
            SendHelp(chatId);
            return;
        }

        if (commandText == "/info")
        {
            SendInfo(chatId);
            return;
        }

        var user = _userRepository.GetByTelegramChatId(chatId);
        if (user is null)
        {
            Send(chatId, "Пользователь не найден. Обратитесь к администратору.");
            return;
        }

        if (commandText == "/report")
        {
            SendReport(chatId, user);
        }
        else if (commandText.StartsWith("/task ", StringComparison.OrdinalIgnoreCase))
        {
            AddCompletedTask(chatId, user, commandText["/task ".Length..]);
        }
        else if (commandText.StartsWith("/block ", StringComparison.OrdinalIgnoreCase))
        {
            AddBlock(chatId, user, commandText["/block ".Length..]);
        }
        else if (commandText == "/my_last_report")
        {
            SendMyLastReport(chatId, user);
        }
        else if (commandText == "/my_tasks")
        {
            SendEmployeeTasks(chatId, user);
        }
        else if (commandText.StartsWith("/start_task ", StringComparison.OrdinalIgnoreCase))
        {
            StartTask(chatId, user, commandText["/start_task ".Length..]);
        }
        else if (commandText.StartsWith("/close_task ", StringComparison.OrdinalIgnoreCase))
        {
            CloseTask(chatId, user, commandText["/close_task ".Length..]);
        }
        else if (commandText == "/reports")
        {
            SendReports(chatId, user);
        }
        else if (commandText == "/employees")
        {
            SendEmployees(chatId, user);
        }
        else if (commandText == "/missing_reports")
        {
            SendMissingReports(chatId, user);
        }
        else if (commandText == "/summary")
        {
            SendSummary(chatId, user);
        }
        else if (commandText.StartsWith("/employee_report ", StringComparison.OrdinalIgnoreCase))
        {
            SendEmployeeReport(chatId, user, commandText["/employee_report ".Length..]);
        }
        else if (commandText.StartsWith("/assign_task ", StringComparison.OrdinalIgnoreCase))
        {
            AssignTask(chatId, user, commandText["/assign_task ".Length..]);
        }
        else if (commandText.StartsWith("/employee_tasks ", StringComparison.OrdinalIgnoreCase))
        {
            SendEmployeeTasksByName(chatId, user, commandText["/employee_tasks ".Length..]);
        }
        else if (commandText == "/team_tasks")
        {
            SendTeamTasks(chatId, user);
        }
        else
        {
            Send(chatId, "Команда не распознана. Введите /help для просмотра команд.");
        }
    }

    /// <summary>
    /// Отправляет приветственное сообщение.
    /// </summary>
    private void SendStart(long chatId)
    {
        Send(chatId, "Бот отчетности сотрудников запущен. Введите /help для просмотра команд.");
    }

    /// <summary>
    /// Отправляет список доступных команд.
    /// </summary>
    private void SendHelp(long chatId)
    {
        Send(chatId,
            "Справка по командам\n\n" +
            "Общие команды:\n" +
            "/start - начать работу с ботом.\n" +
            "/help - показать подробную справку.\n" +
            "/info - описание программы и версия.\n\n" +
            "Команды сотрудника:\n" +
            "/report - отправить ежедневный отчет за сегодня.\n" +
            "/task текст - добавить выполненную задачу в отчет.\n" +
            "Пример: /task Исправил ошибку в форме отчета\n" +
            "/block текст - добавить проблему или блокер.\n" +
            "Пример: /block Нет доступа к базе данных\n" +
            "/my_last_report - посмотреть свой отчет за сегодня.\n" +
            "/my_tasks - посмотреть свои назначенные задачи.\n" +
            "/start_task номер - перевести задачу в работу.\n" +
            "Пример: /start_task 1\n" +
            "/close_task номер комментарий - закрыть задачу с комментарием.\n" +
            "Пример: /close_task 1 Задача выполнена и проверена\n\n" +
            "Команды lead:\n" +
            "/reports - посмотреть отчеты сотрудников за сегодня.\n" +
            "/employees - посмотреть список сотрудников.\n" +
            "/missing_reports - посмотреть, кто не отправил отчет.\n" +
            "/summary - посмотреть краткую сводку по отчетам.\n" +
            "/employee_report имя - посмотреть отчет конкретного сотрудника.\n" +
            "Пример: /employee_report Иван Иванов\n" +
            "/assign_task имя | задача | дата - назначить задачу сотруднику.\n" +
            "Пример: /assign_task Иван Иванов | Подготовить отчет | 20.05.2026\n" +
            "/employee_tasks имя - посмотреть задачи сотрудника.\n" +
            "Пример: /employee_tasks Иван Иванов\n" +
            "/team_tasks - посмотреть задачи всей группы.");
    }

    /// <summary>
    /// Отправляет описание программы и версию.
    /// </summary>
    private void SendInfo(long chatId)
    {
        Send(chatId,
            "О программе\n\n" +
            "Бот отчета - Telegram-бот для организации отчетности сотрудников перед lead.\n" +
            "Бот позволяет сотрудникам отправлять ежедневные отчеты, указывать выполненные задачи и проблемы, " +
            "а lead может назначать задачи, смотреть отчеты и контролировать статусы задач.\n\n" +
            "Версия: 1.0.0");
    }

    /// <summary>
    /// Отмечает отчет сотрудника как отправленный.
    /// </summary>
    private void SendReport(long chatId, User user)
    {
        if (!CheckEmployeeRole(chatId, user))
        {
            return;
        }

        _reportService.SendReport(user);
        Send(chatId, "Отчет отправлен.");
    }

    /// <summary>
    /// Добавляет выполненную задачу в отчет сотрудника.
    /// </summary>
    private void AddCompletedTask(long chatId, User user, string text)
    {
        if (!CheckEmployeeRole(chatId, user))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            Send(chatId, "Введите выполненную задачу после команды /task.");
            return;
        }

        _reportService.AddCompletedTask(user, text.Trim());
        Send(chatId, "Выполненные задачи добавлены в отчет.");
    }

    /// <summary>
    /// Добавляет проблему или блокер в отчет сотрудника.
    /// </summary>
    private void AddBlock(long chatId, User user, string text)
    {
        if (!CheckEmployeeRole(chatId, user))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            Send(chatId, "Введите проблему после команды /block.");
            return;
        }

        _reportService.AddBlock(user, text.Trim());
        Send(chatId, "Проблемы добавлены в отчет.");
    }

    /// <summary>
    /// Показывает сотруднику его сегодняшний отчет.
    /// </summary>
    private void SendMyLastReport(long chatId, User user)
    {
        if (!CheckEmployeeRole(chatId, user))
        {
            return;
        }

        var report = _reportService.GetTodayReport(user);
        if (report is null)
        {
            Send(chatId, "За сегодня отчет еще не создан.");
            return;
        }

        Send(chatId,
            "Ваш последний отчет:\n" +
            $"Дата: {report.Date:dd.MM.yyyy}\n" +
            $"Отправлен: {(report.IsSent ? "да" : "нет")}\n" +
            $"Выполнено: {FormatList(report.CompletedTasks)}\n" +
            $"Проблемы: {FormatList(report.Blocks)}");
    }

    /// <summary>
    /// Переводит задачу сотрудника в работу.
    /// </summary>
    private void StartTask(long chatId, User user, string taskIdText)
    {
        if (!CheckEmployeeRole(chatId, user))
        {
            return;
        }

        if (!int.TryParse(taskIdText.Trim(), out var taskId))
        {
            Send(chatId, "Укажите номер задачи. Пример: /start_task 1");
            return;
        }

        var isStarted = _taskService.StartTask(user, taskId);
        Send(chatId, isStarted
            ? "Задача переведена в статус \"в работе\"."
            : "Задача не найдена или уже закрыта.");
    }

    /// <summary>
    /// Закрывает задачу сотрудника с комментарием.
    /// </summary>
    private void CloseTask(long chatId, User user, string value)
    {
        if (!CheckEmployeeRole(chatId, user))
        {
            return;
        }

        var spaceIndex = value.IndexOf(' ');
        if (spaceIndex < 0 || !int.TryParse(value[..spaceIndex], out var taskId))
        {
            Send(chatId, "Укажите номер задачи и комментарий. Пример: /close_task 1 задача выполнена");
            return;
        }

        var comment = value[(spaceIndex + 1)..].Trim();
        if (string.IsNullOrWhiteSpace(comment))
        {
            Send(chatId, "Для закрытия задачи необходимо оставить комментарий.");
            return;
        }

        var isClosed = _taskService.CloseTask(user, taskId, comment);
        Send(chatId, isClosed
            ? "Задача закрыта. Комментарий сохранен."
            : "Задача не найдена.");
    }

    /// <summary>
    /// Показывает lead список отчетов за сегодня.
    /// </summary>
    private void SendReports(long chatId, User user)
    {
        if (!CheckLeadRole(chatId, user))
        {
            return;
        }

        var reports = _reportService.GetTodayReports();
        var employees = _userRepository.GetEmployees();
        var lines = new List<string> { "Отчеты сотрудников за сегодня:" };

        foreach (var employee in employees)
        {
            var report = reports.FirstOrDefault(item => item.EmployeeId == employee.Id);
            var status = report?.IsSent == true ? "отчет отправлен" : "отчет не отправлен";
            lines.Add($"{employee.FullName} - {status}.");
        }

        Send(chatId, string.Join('\n', lines));
    }

    /// <summary>
    /// Показывает lead список сотрудников.
    /// </summary>
    private void SendEmployees(long chatId, User user)
    {
        if (!CheckLeadRole(chatId, user))
        {
            return;
        }

        var lines = _userRepository.GetEmployees()
            .Select(employee => $"{employee.Id}. {employee.FullName}")
            .ToList();

        Send(chatId, lines.Count == 0
            ? "Сотрудники не найдены."
            : "Список сотрудников:\n" + string.Join('\n', lines));
    }

    /// <summary>
    /// Показывает lead сотрудников, которые сегодня еще не отправили отчет.
    /// </summary>
    private void SendMissingReports(long chatId, User user)
    {
        if (!CheckLeadRole(chatId, user))
        {
            return;
        }

        var reports = _reportService.GetTodayReports();
        var lines = _userRepository.GetEmployees()
            .Where(employee => reports.All(report => report.EmployeeId != employee.Id || !report.IsSent))
            .Select(employee => $"{employee.Id}. {employee.FullName}")
            .ToList();

        Send(chatId, lines.Count == 0
            ? "Сегодня все сотрудники отправили отчет."
            : "Сегодня отчет еще не отправили:\n" + string.Join('\n', lines));
    }

    /// <summary>
    /// Показывает lead краткую сводку по отчетам команды.
    /// </summary>
    private void SendSummary(long chatId, User user)
    {
        if (!CheckLeadRole(chatId, user))
        {
            return;
        }

        var employees = _userRepository.GetEmployees();
        var reports = _reportService.GetTodayReports();
        var sentCount = employees.Count(employee =>
            reports.Any(report => report.EmployeeId == employee.Id && report.IsSent));
        var blocksCount = reports.Count(report => report.Blocks.Count > 0);

        Send(chatId,
            "Сводка по команде за сегодня:\n" +
            $"Всего сотрудников: {employees.Count}\n" +
            $"Отчеты отправили: {sentCount}\n" +
            $"Отчеты не отправили: {employees.Count - sentCount}\n" +
            $"Проблемы указали: {blocksCount}");
    }

    /// <summary>
    /// Показывает lead отчет конкретного сотрудника.
    /// </summary>
    private void SendEmployeeReport(long chatId, User user, string employeeName)
    {
        if (!CheckLeadRole(chatId, user))
        {
            return;
        }

        var employee = FindEmployee(employeeName);
        if (employee is null)
        {
            SendEmployeeNotFound(chatId);
            return;
        }

        var report = _reportService.GetTodayReport(employee);
        if (report is null)
        {
            Send(chatId, $"У сотрудника {employee.FullName} сегодня еще нет отчета.");
            return;
        }

        Send(chatId,
            $"Отчет сотрудника {employee.FullName} за сегодня:\n" +
            $"Отправлен: {(report.IsSent ? "да" : "нет")}\n" +
            $"Выполнено: {FormatList(report.CompletedTasks)}\n" +
            $"Проблемы: {FormatList(report.Blocks)}");
    }

    /// <summary>
    /// Назначает задачу сотруднику от имени lead.
    /// </summary>
    private void AssignTask(long chatId, User lead, string value)
    {
        if (!CheckLeadRole(chatId, lead))
        {
            return;
        }

        var parts = value.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
        {
            Send(chatId, "Используйте формат: /assign_task Иван Иванов | Название задачи | 20.05.2026");
            return;
        }

        var employee = FindEmployee(parts[0]);
        if (employee is null)
        {
            SendEmployeeNotFound(chatId);
            return;
        }

        if (!DateOnly.TryParse(parts[2], out var deadline))
        {
            Send(chatId, "Не удалось распознать дату. Пример даты: 20.05.2026");
            return;
        }

        var task = _taskService.AssignTask(lead, employee, parts[1], deadline);

        Send(chatId,
            $"Задача назначена сотруднику {employee.FullName}.\n" +
            $"Статус задачи: {GetTaskStatusName(task.Status)}.\n" +
            $"Срок: {task.Deadline:dd.MM.yyyy}.");

        if (employee.TelegramChatId != 0)
        {
            Send(employee.TelegramChatId,
                "Вам назначена новая задача:\n" +
                $"{task.Title}\n" +
                $"Срок: {task.Deadline:dd.MM.yyyy}\n" +
                $"Статус: {GetTaskStatusName(task.Status)}");
        }
    }

    /// <summary>
    /// Показывает сотруднику его задачи.
    /// </summary>
    private void SendEmployeeTasks(long chatId, User employee)
    {
        if (!CheckEmployeeRole(chatId, employee))
        {
            return;
        }

        Send(chatId, FormatEmployeeTasks(employee));
    }

    /// <summary>
    /// Показывает lead задачи конкретного сотрудника.
    /// </summary>
    private void SendEmployeeTasksByName(long chatId, User user, string employeeName)
    {
        if (!CheckLeadRole(chatId, user))
        {
            return;
        }

        var employee = FindEmployee(employeeName);
        if (employee is null)
        {
            SendEmployeeNotFound(chatId);
            return;
        }

        Send(chatId, FormatEmployeeTasks(employee));
    }

    /// <summary>
    /// Показывает lead задачи всей группы.
    /// </summary>
    private void SendTeamTasks(long chatId, User user)
    {
        if (!CheckLeadRole(chatId, user))
        {
            return;
        }

        var lines = new List<string> { "Задачи всей группы:" };
        foreach (var employee in _userRepository.GetEmployees())
        {
            lines.Add(string.Empty);
            lines.Add(FormatEmployeeTasks(employee));
        }

        Send(chatId, string.Join('\n', lines));
    }

    /// <summary>
    /// Формирует текст со списком задач сотрудника.
    /// </summary>
    private string FormatEmployeeTasks(User employee)
    {
        var tasks = _taskService.GetEmployeeTasks(employee);
        if (tasks.Count == 0)
        {
            return $"Задачи сотрудника {employee.FullName} не найдены.";
        }

        var lines = new List<string> { $"Задачи сотрудника {employee.FullName}:" };
        foreach (var task in tasks)
        {
            lines.Add($"{task.Id}. {task.Title}");
            lines.Add($"Статус: {GetTaskStatusName(task.Status)}");
            lines.Add($"Срок: {task.Deadline:dd.MM.yyyy}");

            if (task.Status == TaskStatus.Closed)
            {
                lines.Add($"Комментарий: {task.ClosingComment}");
            }

            lines.Add(string.Empty);
        }

        return string.Join('\n', lines).TrimEnd();
    }

    /// <summary>
    /// Проверяет, что пользователь является сотрудником.
    /// </summary>
    private bool CheckEmployeeRole(long chatId, User user)
    {
        if (user.Role == UserRole.Employee)
        {
            return true;
        }

        Send(chatId, "Команда доступна только сотруднику.");
        return false;
    }

    /// <summary>
    /// Проверяет, что пользователь является lead.
    /// </summary>
    private bool CheckLeadRole(long chatId, User user)
    {
        if (user.Role == UserRole.Lead)
        {
            return true;
        }

        Send(chatId, "Команда доступна только lead.");
        return false;
    }

    /// <summary>
    /// Ищет сотрудника по номеру, полному имени или части имени.
    /// </summary>
    private User? FindEmployee(string value)
    {
        var normalizedValue = NormalizeText(value);
        if (int.TryParse(normalizedValue, out var employeeId))
        {
            var employeeById = _userRepository.GetById(employeeId);
            return employeeById?.Role == UserRole.Employee ? employeeById : null;
        }

        var employees = _userRepository.GetEmployees();
        var exactMatch = employees.FirstOrDefault(employee =>
            NormalizeText(employee.FullName).Equals(normalizedValue, StringComparison.CurrentCultureIgnoreCase));

        if (exactMatch is not null)
        {
            return exactMatch;
        }

        return employees.FirstOrDefault(employee =>
            NormalizeText(employee.FullName).Contains(normalizedValue, StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// Отправляет сообщение о том, что сотрудник не найден.
    /// </summary>
    private void SendEmployeeNotFound(long chatId)
    {
        var employees = _userRepository.GetEmployees()
            .Select(employee => $"{employee.Id}. {employee.FullName}");

        Send(chatId, "Сотрудник не найден.\nДоступные сотрудники:\n" + string.Join('\n', employees));
    }

    /// <summary>
    /// Форматирует список строк для вывода.
    /// </summary>
    private static string FormatList(List<string> values)
    {
        return values.Count == 0 ? "нет" : string.Join("; ", values);
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
    /// Убирает лишние пробелы из текста.
    /// </summary>
    private static string NormalizeText(string text)
    {
        return string.Join(' ', text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// Отправляет сообщение пользователю.
    /// </summary>
    private void Send(long chatId, string text)
    {
        try
        {
            _messageSender.SendMessage(chatId, text);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Ошибка отправки сообщения в Telegram: {exception}");
        }
    }
}
