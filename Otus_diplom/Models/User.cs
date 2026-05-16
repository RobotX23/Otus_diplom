namespace Otus_diplom.Models;

/// <summary>
/// Пользователь консольного прототипа бота.
/// </summary>
public class User
{
    /// <summary>
    /// Уникальный номер пользователя.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Имя пользователя, которое вводится в командах.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Роль пользователя: сотрудник или lead.
    /// </summary>
    public UserRole Role { get; set; }
}
