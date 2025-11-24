using System.Diagnostics.CodeAnalysis;

namespace Keepi.Core;

public interface IResult
{
    bool Succeeded { get; }
}

public interface IMaybeErrorResult<TError> : IResult
{
    bool TrySuccess([NotNullWhen(returnValue: false)] out TError? errorResult);
}

public interface IValueOrErrorResult<TSuccess, TError> : IResult
{
    bool TrySuccess(
        [NotNullWhen(returnValue: true)] out TSuccess? successResult,
        [NotNullWhen(returnValue: false)] out TError? errorResult
    );
}

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

    private sealed class MaybeErrorResult<TError> : IMaybeErrorResult<TError>
    {
        private readonly TError? errorOrNull;

        public bool Succeeded { get; }

        public MaybeErrorResult(bool succeeded, TError? errorResult)
        {
            if (!succeeded && errorResult == null)
            {
                throw new ArgumentNullException(
                    message: "NULL error result is not supported on failure",
                    paramName: nameof(errorResult)
                );
            }

            errorOrNull = errorResult;
            Succeeded = succeeded;
        }

        public bool TrySuccess([NotNullWhen(false)] out TError? errorResult)
        {
            errorResult = errorOrNull;
            return Succeeded;
        }
    }

    private sealed class ValueOrErrorResult<TSuccess, TError>
        : IValueOrErrorResult<TSuccess, TError>
    {
        private readonly TError? errorOrNull;
        private readonly TSuccess? successOrNull;

        public bool Succeeded { get; }

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

            successOrNull = successResult;
            errorOrNull = errorResult;
            Succeeded = succeeded;
        }

        public bool TrySuccess(
            [NotNullWhen(true)] out TSuccess? successResult,
            [NotNullWhen(false)] out TError? errorResult
        )
        {
            successResult = successOrNull;
            errorResult = errorOrNull;
            return Succeeded;
        }
    }
}
