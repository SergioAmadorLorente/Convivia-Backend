using System.Collections.Generic;

namespace Convivia.Application.Exceptions
{
    public class ValidationException : AppException
    {
        public IReadOnlyDictionary<string, string[]> Errors { get; }

        public ValidationException(IReadOnlyDictionary<string, string[]> errors, string? message = "Validation failed")
            : base(message)
        {
            Errors = errors ?? new Dictionary<string, string[]>();
        }
    }
}
