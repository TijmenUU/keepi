namespace Keepi.Core.Unit.Tests;

public class ResultTests
{
    [Fact]
    public void Success_without_value_should_return_expected_object()
    {
        var result = Result.Success<TestError>();

        result.Succeeded.ShouldBeTrue();
        result.TrySuccess(out _).ShouldBeTrue();
    }

    [Fact]
    public void Success_with_value_should_return_expected_object()
    {
        var result = Result.Success<int, TestError>(42);

        result.Succeeded.ShouldBeTrue();
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();
        successResult.ShouldBe(42);
    }

    [Fact]
    public void Failure_without_success_value_should_return_expected_object()
    {
        var result = Result.Failure(TestError.SecondValue);

        result.Succeeded.ShouldBeFalse();
        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(TestError.SecondValue);
    }

    [Fact]
    public void Failure_with_success_value_should_return_expected_object()
    {
        var result = Result.Failure<int, TestError>(TestError.SecondValue);

        result.Succeeded.ShouldBeFalse();
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(TestError.SecondValue);
    }

    enum TestError
    {
        FirstValue,
        SecondValue,
        ThirdValue,
    }
}
