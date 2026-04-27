using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string entityName, object key)
            : base($"{entityName} with key '{key}' was not found")
        {
        }

        public NotFoundException() : base()
        {
        }

        public NotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
