namespace Convivia.Application.Exceptions
{
    public abstract class AppException : Exception
    {
        protected AppException(string? message = null, Exception? inner = null)
            : base(message, inner) { }
    }
}
