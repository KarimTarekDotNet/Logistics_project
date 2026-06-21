namespace Application.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Value { get; private set; }
        public string? Error { get; private set; }
        public int StatusCode { get; private set; }

        private Result() { }

        public static Result<T> Success(T value, int statusCode = 200)
            => new() { IsSuccess = true, Value = value, StatusCode = statusCode };

        public static Result<T> Failure(string error, int statusCode = 400)
            => new() { IsSuccess = false, Error = error, StatusCode = statusCode };

        public static Result<T> NotFound(string error = "Resource not found.")
            => new() { IsSuccess = false, Error = error, StatusCode = 404 };

        public static Result<T> Unauthorized(string error = "Unauthorized.")
            => new() { IsSuccess = false, Error = error, StatusCode = 401 };

        public static Result<T> Forbidden(string error = "Access denied.")
            => new() { IsSuccess = false, Error = error, StatusCode = 403 };
    }

    public class Result
    {
        public bool IsSuccess { get; private set; }
        public string? Error { get; private set; }
        public int StatusCode { get; private set; }

        private Result() { }

        public static Result Success(int statusCode = 200)
            => new() { IsSuccess = true, StatusCode = statusCode };

        public static Result Failure(string error, int statusCode = 400)
            => new() { IsSuccess = false, Error = error, StatusCode = statusCode };

        public static Result NotFound(string error = "Resource not found.")
            => new() { IsSuccess = false, Error = error, StatusCode = 404 };

        public static Result Unauthorized(string error = "Unauthorized.")
            => new() { IsSuccess = false, Error = error, StatusCode = 401 };

        public static Result Forbidden(string error = "Access denied.")
            => new() { IsSuccess = false, Error = error, StatusCode = 403 };
    }
}
