namespace Otus_diplom.Core.Exceptions;

/// <summary>
/// Исключение для ошибок бизнес-логики.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Создает исключение с текстом ошибки бизнес-логики.
    /// </summary>
    public DomainException(string message)
        : base(message)
    {
    }
}
