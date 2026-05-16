using Otus_diplom.Data;
using Otus_diplom.Models;
using Otus_diplom.Services;
using TaskStatus = Otus_diplom.Models.TaskStatus;

namespace Otus_diplom;

/// <summary>
/// Консольный прототип
/// </summary>
public class ConsoleBot
{
    private readonly ReportService _reportService;
    private readonly TaskService _taskService;
    private readonly InMemoryStorage _storage;
    private User _currentUser;

    /// <summary>
    /// Создает консольного бота и выбирает первого пользователя как текущего.
    /// </summary>
    public ConsoleBot(ReportService reportService, TaskService taskService, InMemoryStorage storage)
    {
        _reportService = reportService;
        _taskService = taskService;
        _storage = storage;
        _currentUser = _storage.Users.First();
    }

    /// <summary>
    /// Запускает основной цикл чтения команд из консоли.
    /// </summary>
    public void Run()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        PrintWelcome();

        while (true)
        {
            Console.Write($"{_currentUser.FullName} ({GetRoleName(_currentUser.Role)})> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            if (input.Equals("/exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Работа завершена.");
                return;
            }

            HandleCommand(input.Trim());
        }
    }

    /// <summary>
    /// Определяет команду пользователя и вызывает нужный обработчик.
    /// </summary>
    private void HandleCommand(string input)
    {
        if (input == "/help")
        {
            PrintHelp();
        }
        else if (input.StartsWith("/login ", StringComparison.OrdinalIgnoreCase))
        {
            Login(input["/login ".Length..]);
        }
        else if (input == "/report")
        {
            SendReport();
        }
        else if (input == "/task")
        {
            AddCompletedTask();
        }
        else if (input == "/block")
        {
            AddBlock();
        }
        else if (input == "/my_tasks")
        {
            PrintEmployeeTasks(_currentUser);
        }
        else if (input.StartsWith("/start_task ", StringComparison.OrdinalIgnoreCase))
        {
            StartTask(input["/start_task ".Length..]);
        }
        else if (input.StartsWith("/close_task ", StringComparison.OrdinalIgnoreCase))
        {
            CloseTask(input["/close_task ".Length..]);
        }
        else if (input == "/reports")
        {
            PrintTodayReports();
        }
        else if (input.StartsWith("/employee_report ", StringComparison.OrdinalIgnoreCase))
        {
            PrintEmployeeReport(input["/employee_report ".Length..]);
        }
        else if (input.StartsWith("/assign_task ", StringComparison.OrdinalIgnoreCase))
        {
            AssignTask(input["/assign_task ".Length..]);
        }
        else if (input.StartsWith("/employee_tasks ", StringComparison.OrdinalIgnoreCase))
        {
            PrintEmployeeTasksByName(input["/employee_tasks ".Length..]);
        }
        else if (input == "/team_tasks")
        {
            PrintTeamTasks();
        }
        else
        {
            Console.WriteLine("Команда не распознана. Введите /help для просмотра команд.");
        }
    }

    /// <summary>
    /// Показывает стартовое сообщение и список тестовых пользователей.
    /// </summary>
    private void PrintWelcome()
    {
        Console.WriteLine("Учебный консольный прототип Telegram-бота отчетности.");
        Console.WriteLine("Введите /help для просмотра команд.");
        Console.WriteLine("Введите /exit для выхода.");
        Console.WriteLine();
        PrintUsers();
        Console.WriteLine();
    }

    /// <summary>
    /// Показывает список доступных команд.
    /// </summary>
    private void PrintHelp()
    {
        Console.WriteLine("Общие команды:");
        Console.WriteLine("/login Иван Иванов - войти под пользователем");
        Console.WriteLine("/login 3 - войти под пользователем по номеру");
        Console.WriteLine("/help - показать команды");
        Console.WriteLine("/exit - выйти");
        Console.WriteLine();
        Console.WriteLine("Команды сотрудника:");
        Console.WriteLine("/report - отправить ежедневный отчет");
        Console.WriteLine("/task - добавить выполненную задачу");
        Console.WriteLine("/block - указать проблему или блокер");
        Console.WriteLine("/my_tasks - посмотреть свои задачи");
        Console.WriteLine("/start_task 1 - перевести задачу в работу");
        Console.WriteLine("/close_task 1 - закрыть задачу с комментарием");
        Console.WriteLine();
        Console.WriteLine("Команды lead:");
        Console.WriteLine("/reports - посмотреть отчеты сотрудников за сегодня");
        Console.WriteLine("/employee_report Иван Иванов - посмотреть отчет сотрудника");
        Console.WriteLine("/assign_task Иван Иванов | Подготовить отчет | 20.05.2026 - назначить задачу");
        Console.WriteLine("/employee_tasks Иван Иванов - посмотреть задачи сотрудника");
        Console.WriteLine("/team_tasks - посмотреть задачи всей группы");
    }

    /// <summary>
    /// Выводит тестовых пользователей, заведенных в памяти приложения.
    /// </summary>
    private void PrintUsers()
    {
        Console.WriteLine("Тестовые пользователи:");
        foreach (var user in _storage.Users)
        {
            Console.WriteLine($"- {user.FullName} ({GetRoleName(user.Role)})");
        }
    }

    /// <summary>
    /// Переключает текущего пользователя по имени или номеру.
    /// </summary>
    private void Login(string fullName)
    {
        var user = FindUser(fullName);
        if (user is null)
        {
            Console.WriteLine("Пользователь не найден.");
            return;
        }

        _currentUser = user;
        Console.WriteLine($"Выполнен вход: {_currentUser.FullName} ({GetRoleName(_currentUser.Role)}).");
    }

    /// <summary>
    /// Обрабатывает отправку ежедневного отчета сотрудником.
    /// </summary>
    private void SendReport()
    {
        if (!CheckEmployeeRole())
        {
            return;
        }

        Console.WriteLine("Отправить отчет?");
        var answer = Console.ReadLine();

        if (IsPositiveAnswer(answer))
        {
            _reportService.SendReport(_currentUser);
            Console.WriteLine("Отчет отправлен.");
            return;
        }

        Console.WriteLine("Отчет не отправлен.");
    }

    /// <summary>
    /// Добавляет выполненную задачу в сегодняшний отчет сотрудника.
    /// </summary>
    private void AddCompletedTask()
    {
        if (!CheckEmployeeRole())
        {
            return;
        }

        Console.WriteLine("Введите выполненную задачу.");
        var text = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(text))
        {
            Console.WriteLine("Задача не добавлена: текст пустой.");
            return;
        }

        _reportService.AddCompletedTask(_currentUser, text);
        Console.WriteLine("Выполненные задачи добавлены в отчет.");
    }

    /// <summary>
    /// Добавляет проблему или блокер в сегодняшний отчет сотрудника.
    /// </summary>
    private void AddBlock()
    {
        if (!CheckEmployeeRole())
        {
            return;
        }

        Console.WriteLine("Какая проблема возникла?");
        var text = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(text))
        {
            Console.WriteLine("Проблема не добавлена: текст пустой.");
            return;
        }

        _reportService.AddBlock(_currentUser, text);
        Console.WriteLine("Проблемы добавлены в отчет.");
    }

    /// <summary>
    /// Переводит выбранную задачу текущего сотрудника в работу.
    /// </summary>
    private void StartTask(string value)
    {
        if (!CheckEmployeeRole())
        {
            return;
        }

        if (!int.TryParse(value, out var taskId))
        {
            Console.WriteLine("Укажите номер задачи. Пример: /start_task 1");
            return;
        }

        var isStarted = _taskService.StartTask(_currentUser, taskId);
        Console.WriteLine(isStarted
            ? "Задача переведена в статус \"в работе\"."
            : "Задача не найдена или уже закрыта.");
    }

    /// <summary>
    /// Закрывает выбранную задачу текущего сотрудника после ввода комментария.
    /// </summary>
    private void CloseTask(string value)
    {
        if (!CheckEmployeeRole())
        {
            return;
        }

        if (!int.TryParse(value, out var taskId))
        {
            Console.WriteLine("Укажите номер задачи. Пример: /close_task 1");
            return;
        }

        Console.WriteLine("Введите комментарий по выполненной задаче.");
        var comment = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(comment))
        {
            Console.WriteLine("Для закрытия задачи необходимо оставить комментарий.");
            return;
        }

        var isClosed = _taskService.CloseTask(_currentUser, taskId, comment);
        Console.WriteLine(isClosed
            ? "Задача закрыта. Комментарий сохранен."
            : "Задача не найдена.");
    }

    /// <summary>
    /// Показывает lead список сотрудников и статус отправки отчета за сегодня.
    /// </summary>
    private void PrintTodayReports()
    {
        if (!CheckLeadRole())
        {
            return;
        }

        Console.WriteLine("Отчеты сотрудников за сегодня:");
        foreach (var employee in GetEmployees())
        {
            var report = _reportService.GetTodayReport(employee);
            var status = report?.IsSent == true ? "отчет отправлен" : "отчет не отправлен";
            Console.WriteLine($"{employee.FullName} - {status}.");
        }
    }

    /// <summary>
    /// Показывает lead сегодняшний отчет конкретного сотрудника.
    /// </summary>
    private void PrintEmployeeReport(string fullName)
    {
        if (!CheckLeadRole())
        {
            return;
        }

        var employee = FindEmployee(fullName);
        if (employee is null)
        {
            PrintEmployeeNotFound();
            return;
        }

        var report = _reportService.GetTodayReport(employee);
        if (report is null)
        {
            Console.WriteLine($"У сотрудника {employee.FullName} сегодня еще нет отчета.");
            return;
        }

        Console.WriteLine($"Отчет сотрудника {employee.FullName} за сегодня:");
        Console.WriteLine($"Отправлен: {(report.IsSent ? "да" : "нет")}");
        Console.WriteLine($"Выполнено: {FormatList(report.CompletedTasks)}");
        Console.WriteLine($"Проблемы: {FormatList(report.Blocks)}");
    }

    /// <summary>
    /// Назначает сотруднику задачу от имени lead.
    /// </summary>
    private void AssignTask(string value)
    {
        if (!CheckLeadRole())
        {
            return;
        }

        var parts = value.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
        {
            Console.WriteLine("Используйте формат: /assign_task Иван Иванов | Название задачи | 20.05.2026");
            return;
        }

        var employee = FindEmployee(parts[0]);
        if (employee is null)
        {
            PrintEmployeeNotFound();
            return;
        }

        if (!DateOnly.TryParse(parts[2], out var deadline))
        {
            Console.WriteLine("Не удалось распознать дату. Пример даты: 20.05.2026");
            return;
        }

        var task = _taskService.AssignTask(employee, parts[1], deadline);

        Console.WriteLine($"Задача назначена сотруднику {employee.FullName}.");
        Console.WriteLine($"Статус задачи: {GetTaskStatusName(task.Status)}.");
        Console.WriteLine($"Срок: {task.Deadline:dd.MM.yyyy}.");
        Console.WriteLine();
        Console.WriteLine("Сообщение сотруднику:");
        Console.WriteLine("Вам назначена новая задача:");
        Console.WriteLine(task.Title);
        Console.WriteLine($"Срок: {task.Deadline:dd.MM.yyyy}");
        Console.WriteLine($"Статус: {GetTaskStatusName(task.Status)}");
    }

    /// <summary>
    /// Показывает lead список задач сотрудника по имени или номеру.
    /// </summary>
    private void PrintEmployeeTasksByName(string fullName)
    {
        if (!CheckLeadRole())
        {
            return;
        }

        var employee = FindEmployee(fullName);
        if (employee is null)
        {
            PrintEmployeeNotFound();
            return;
        }

        PrintEmployeeTasks(employee);
    }

    /// <summary>
    /// Показывает lead задачи всех сотрудников.
    /// </summary>
    private void PrintTeamTasks()
    {
        if (!CheckLeadRole())
        {
            return;
        }

        Console.WriteLine("Задачи всей группы:");
        foreach (var employee in GetEmployees())
        {
            Console.WriteLine();
            Console.WriteLine($"{employee.FullName}:");
            PrintEmployeeTasks(employee);
        }
    }

    /// <summary>
    /// Выводит список задач переданного сотрудника.
    /// </summary>
    private void PrintEmployeeTasks(User employee)
    {
        var tasks = _taskService.GetEmployeeTasks(employee);
        if (tasks.Count == 0)
        {
            Console.WriteLine("Задачи не найдены.");
            return;
        }

        Console.WriteLine($"Задачи сотрудника {employee.FullName}:");
        foreach (var task in tasks)
        {
            Console.WriteLine($"{task.Id}. {task.Title}");
            Console.WriteLine($"Статус: {GetTaskStatusName(task.Status)}");
            Console.WriteLine($"Срок: {task.Deadline:dd.MM.yyyy}");

            if (task.Status == TaskStatus.Closed)
            {
                Console.WriteLine($"Комментарий: {task.ClosingComment}");
            }

            Console.WriteLine();
        }
    }

    /// <summary>
    /// Проверяет, что текущий пользователь является сотрудником.
    /// </summary>
    private bool CheckEmployeeRole()
    {
        if (_currentUser.Role == UserRole.Employee)
        {
            return true;
        }

        Console.WriteLine("Команда доступна только сотруднику.");
        return false;
    }

    /// <summary>
    /// Проверяет, что текущий пользователь является lead.
    /// </summary>
    private bool CheckLeadRole()
    {
        if (_currentUser.Role == UserRole.Lead)
        {
            return true;
        }

        Console.WriteLine("Команда доступна только lead.");
        return false;
    }

    /// <summary>
    /// Ищет пользователя по имени или номеру.
    /// </summary>
    private User? FindUser(string fullName)
    {
        if (int.TryParse(fullName.Trim(), out var id))
        {
            return _storage.Users.FirstOrDefault(user => user.Id == id);
        }

        return _storage.Users.FirstOrDefault(user =>
            user.FullName.Equals(fullName.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Ищет сотрудника по номеру, полному имени или части имени.
    /// </summary>
    private User? FindEmployee(string fullName)
    {
        var normalizedFullName = NormalizeText(fullName);

        if (int.TryParse(normalizedFullName, out var id))
        {
            return GetEmployees().FirstOrDefault(user => user.Id == id);
        }

        var employees = GetEmployees();
        var exactMatch = employees.FirstOrDefault(user =>
            NormalizeText(user.FullName).Equals(normalizedFullName, StringComparison.CurrentCultureIgnoreCase));

        if (exactMatch is not null)
        {
            return exactMatch;
        }

        return employees.FirstOrDefault(user =>
            NormalizeText(user.FullName).Contains(normalizedFullName, StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// Возвращает всех пользователей с ролью сотрудника.
    /// </summary>
    private List<User> GetEmployees()
    {
        return _storage.Users.Where(user => user.Role == UserRole.Employee).ToList();
    }

    /// <summary>
    /// Форматирует список строк для вывода в консоль.
    /// </summary>
    private static string FormatList(List<string> values)
    {
        return values.Count == 0 ? "нет" : string.Join("; ", values);
    }

    /// <summary>
    /// Возвращает русское название роли пользователя.
    /// </summary>
    private static string GetRoleName(UserRole role)
    {
        return role == UserRole.Lead ? "lead" : "сотрудник";
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
    /// Показывает сообщение о том, что сотрудник не найден, и выводит доступных сотрудников.
    /// </summary>
    private void PrintEmployeeNotFound()
    {
        Console.WriteLine("Сотрудник не найден.");
        Console.WriteLine("Доступные сотрудники:");

        foreach (var employee in GetEmployees())
        {
            Console.WriteLine($"{employee.Id}. {employee.FullName}");
        }
    }

    /// <summary>
    /// Проверяет, что пользователь ответил положительно на вопрос бота.
    /// </summary>
    private static bool IsPositiveAnswer(string? answer)
    {
        if (string.IsNullOrWhiteSpace(answer))
        {
            return false;
        }

        var normalizedAnswer = answer.Trim();
        return normalizedAnswer.Equals("Да", StringComparison.OrdinalIgnoreCase)
            || normalizedAnswer.Equals("yes", StringComparison.OrdinalIgnoreCase)
            || normalizedAnswer.Equals("y", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Убирает лишние пробелы из текста перед поиском.
    /// </summary>
    private static string NormalizeText(string text)
    {
        return string.Join(' ', text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
