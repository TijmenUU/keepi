using System.Diagnostics.CodeAnalysis;

namespace Keepi.Core;

public static class Result
{
    public static IMaybeErrorResult<TError> Success<TError>()
    {
        return new MaybeErrorResult<TError>(succeeded: true, errorResult: default);
    }

    public static IValueOrErrorResult<TSuccess, TError> Success<TSuccess, TError>(TSuccess value)
    {
        return new ValueOrErrorResult<TSuccess, TError>(
            succeeded: true,
            successResult: value,
            errorResult: default
        );
    }

    public static IMaybeErrorResult<TError> Failure<TError>(TError value)
    {
        return new MaybeErrorResult<TError>(succeeded: false, errorResult: value);
    }

    public static IValueOrErrorResult<TSuccess, TError> Failure<TSuccess, TError>(TError value)
    {
        return new ValueOrErrorResult<TSuccess, TError>(
            succeeded: false,
            successResult: default,
            errorResult: value
        );
    }

    private class MaybeErrorResult<TError> : IMaybeErrorResult<TError>
    {
        public bool Succeeded { get; }
        public TError? ErrorOrNull { get; }

        public MaybeErrorResult(bool succeeded, TError? errorResult)
        {
            if (!succeeded && errorResult == null)
            {
                throw new ArgumentNullException(
                    message: "NULL error result is not supported on failure",
                    paramName: nameof(errorResult)
                );
            }

            Succeeded = succeeded;
            ErrorOrNull = errorResult;
        }

        public bool TrySuccess([NotNullWhen(false)] out TError? errorResult)
        {
            errorResult = ErrorOrNull;
            return Succeeded;
        }
    }

    private class ValueOrErrorResult<TSuccess, TError> : IValueOrErrorResult<TSuccess, TError>
    {
        private TSuccess? SuccessOrNull { get; }

        public TSuccess SuccessOrThrow
        {
            get
            {
                if (!Succeeded || SuccessOrNull == null)
                {
                    throw new InvalidOperationException(
                        "Non successful result cannot return a success value"
                    );
                }

                return SuccessOrNull;
            }
        }

        public bool Succeeded { get; }

        public TError? ErrorOrNull { get; }

        public ValueOrErrorResult(bool succeeded, TSuccess? successResult, TError? errorResult)
        {
            if (succeeded && successResult == null)
            {
                throw new ArgumentNullException(
                    message: "NULL success result is not supported on success",
                    paramName: nameof(successResult)
                );
            }

            if (!succeeded && errorResult == null)
            {
                throw new ArgumentNullException(
                    message: "NULL error result is not supported on failure",
                    paramName: nameof(errorResult)
                );
            }

            Succeeded = succeeded;
            SuccessOrNull = successResult;
            ErrorOrNull = errorResult;
        }

        public bool TrySuccess(
            [NotNullWhen(true)] out TSuccess? successResult,
            [NotNullWhen(false)] out TError? errorResult
        )
        {
            successResult = SuccessOrNull;
            errorResult = ErrorOrNull;
            return Succeeded;
        }
    }
}

public interface IResult
{
    bool Succeeded { get; }
}

public interface IMaybeErrorResult<TError> : IResult
{
    bool TrySuccess([NotNullWhen(returnValue: false)] out TError? errorResult);
    TError? ErrorOrNull { get; }
}

public interface IValueOrErrorResult<TSuccess, TError> : IResult
{
    bool TrySuccess(
        [NotNullWhen(returnValue: true)] out TSuccess? successResult,
        [NotNullWhen(returnValue: false)] out TError? errorResult
    );

    TSuccess SuccessOrThrow { get; }
    TError? ErrorOrNull { get; }
}
