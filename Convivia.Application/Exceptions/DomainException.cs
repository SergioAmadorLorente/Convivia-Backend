namespace Convivia.Application.Exceptions
{
    public class DomainException : AppException
    {
        public DomainException(string? message = null) : base(message) { }
    }
}
