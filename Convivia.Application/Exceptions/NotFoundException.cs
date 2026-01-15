namespace Convivia.Application.Exceptions
{
    public class NotFoundException : AppException
    {
        public string Entity { get; }
        public object? Key { get; }

        public NotFoundException(string entity, object? key = null, string? message = null)
            : base(message ?? $"{entity} not found")
        {
            Entity = entity;
            Key = key;
        }
    }
}
