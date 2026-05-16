namespace Otus_diplom.Models;

/// <summary>
/// Роль пользователя в учебном боте.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Сотрудник, который отправляет отчеты и работает с назначенными задачами.
    /// </summary>
    Employee,

    /// <summary>
    /// Lead, который просматривает отчеты и назначает задачи сотрудникам.
    /// </summary>
    Lead
}
