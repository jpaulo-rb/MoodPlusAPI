namespace MoodPlusAPI.Utils
{
    public class Result
    {
        public bool IsSuccess { get; }
        public ApiError ApiError { get; }

        protected Result(bool isSuccess, ApiError? apiError)
        {
            IsSuccess = isSuccess;
            ApiError = apiError;
        }

        public static Result Success() => new(true, null);
        public static Result Failure(ApiError apiError) => new(false, apiError);
    }

    public class Result<T> : Result
    {
        public T? Value { get; }

        private Result(T value) : base(true, null)
        {
            Value = value;
        }

        private Result(ApiError apiError) : base(false, apiError)
        {
        }

        public static Result<T> Success(T value) => new(value);
        public static new Result<T> Failure(ApiError apiError) => new(apiError);
    }
}