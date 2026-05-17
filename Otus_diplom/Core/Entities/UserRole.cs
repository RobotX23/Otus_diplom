namespace Otus_diplom.Core.Entities;

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
    Lead,

    /// <summary>
    /// Администратор, который настраивает пользователей и параметры бота.
    /// </summary>
    Administrator
}
