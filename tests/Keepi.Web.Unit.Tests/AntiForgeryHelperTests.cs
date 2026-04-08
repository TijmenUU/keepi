using Keepi.Generators;
using Microsoft.AspNetCore.Http;

namespace Keepi.Web.Unit.Tests;

public class AntiForgeryHelperTests
{
    [Fact]
    public async Task IsValidRequest_returns_true_for_API_calls()
    {
        var context = new AntiForgeryHelperTestContext().WithAntiforgeryIsRequestValidAsyncCall(
            returnValue: true
        );

        var httpRequestMock = new Mock<HttpRequest>();
        httpRequestMock.Setup(x => x.Path).Returns(new PathString("/api/sub/path"));

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.Request).Returns(httpRequestMock.Object);

        var result = await context
            .BuildTarget()
            .IsValidRequest(context: httpContextMock.Object, apiBasePath: "/api");

        result.ShouldBeTrue();

        context.AntiforgeryMock.Verify(x => x.IsRequestValidAsync(httpContextMock.Object));
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task IsValidRequest_returns_always_true_for_non_API_calls()
    {
        var context = new AntiForgeryHelperTestContext();

        var httpRequestMock = new Mock<HttpRequest>();
        httpRequestMock.Setup(x => x.Path).Returns(new PathString("/bapi/sub/path"));

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.Request).Returns(httpRequestMock.Object);

        var result = await context
            .BuildTarget()
            .IsValidRequest(context: httpContextMock.Object, apiBasePath: "/api");

        result.ShouldBeTrue();

        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task IsValidRequest_returns_false_for_API_call_that_fails_forgery_check()
    {
        var context = new AntiForgeryHelperTestContext().WithAntiforgeryIsRequestValidAsyncCall(
            returnValue: false
        );

        var httpRequestMock = new Mock<HttpRequest>();
        httpRequestMock.Setup(x => x.Path).Returns(new PathString("/api/sub/path"));
        httpRequestMock.Setup(x => x.Scheme).Returns("https");
        httpRequestMock.Setup(x => x.Host).Returns(new HostString("example.com"));
        httpRequestMock.Setup(x => x.PathBase).Returns(new PathString());
        httpRequestMock.Setup(x => x.QueryString).Returns(new QueryString());

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.Request).Returns(httpRequestMock.Object);

        var result = await context
            .BuildTarget()
            .IsValidRequest(context: httpContextMock.Object, apiBasePath: "/api");

        result.ShouldBeFalse();

        context.AntiforgeryMock.Verify(x => x.IsRequestValidAsync(httpContextMock.Object));
        context.VerifyNoOtherCalls();
    }
}

[GenerateTestContext(target: typeof(AntiForgeryHelper), GenerateWithMethods = true)]
internal partial class AntiForgeryHelperTestContext { }
