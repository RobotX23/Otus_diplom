namespace Otus_diplom.Core.Exceptions;

/// <summary>
/// Исключение для ошибок бизнес-логики.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}
