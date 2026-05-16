namespace Otus_diplom.Models;

/// <summary>
/// Статус задачи, назначенной сотруднику.
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// Задача создана, но сотрудник еще не взял ее в работу.
    /// </summary>
    Open,

    /// <summary>
    /// Сотрудник начал выполнять задачу.
    /// </summary>
    InProgress,

    /// <summary>
    /// Сотрудник закрыл задачу и оставил комментарий.
    /// </summary>
    Closed
}
