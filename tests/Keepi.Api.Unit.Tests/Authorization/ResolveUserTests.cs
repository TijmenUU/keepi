using System.Security.Claims;
using Keepi.Api.Authorization;
using Keepi.Core;
using Keepi.Core.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.Unit.Tests.Authorization;

public class ResolveUserTests
{
    [Fact]
    public async Task Execute_returns_expected_user_for_existing_user()
    {
        var context = new TestContext()
            .WithExistingUser(
                id: 42,
                name: "Bob52",
                emailAddress: "bob@example.com",
                entriesPermission: UserPermission.ReadAndModify,
                exportsPermission: UserPermission.ReadAndModify,
                projectsPermission: UserPermission.ReadAndModify,
                usersPermission: UserPermission.ReadAndModify
            )
            .WithHttpContextIdentity(
                new ClaimsIdentity(
                    claims:
                    [
                        new(type: ClaimTypes.Name, value: "Bob52"),
                        new(type: ClaimTypes.Email, value: "bob@example.com"),
                        new(type: ClaimTypes.NameIdentifier, value: "github-33"),
                    ],
                    authenticationType: "GitHub"
                )
            );
        var helper = context.BuildHelper();

        var result = await helper.Execute(cancellationToken: CancellationToken.None);
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new ResolvedUser(
                Id: 42,
                Name: "Bob52",
                EmailAddress: "bob@example.com",
                EntriesPermission: UserPermission.ReadAndModify,
                ExportsPermission: UserPermission.ReadAndModify,
                ProjectsPermission: UserPermission.ReadAndModify,
                UsersPermission: UserPermission.ReadAndModify
            )
        );

        context.HttpContextAccessorMock.Verify(x => x.HttpContext);
        context.GetOrRegisterNewUserUseCaseMock.Verify(x =>
            x.Execute(
                "github-33",
                "bob@example.com",
                "Bob52",
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_newly_registered_user_for_new_user()
    {
        var context = new TestContext()
            .WithNewlyRegisteredUser(
                id: 42,
                name: "Bob52",
                emailAddress: "bob@example.com",
                entriesPermission: UserPermission.ReadAndModify,
                exportsPermission: UserPermission.ReadAndModify,
                projectsPermission: UserPermission.ReadAndModify,
                usersPermission: UserPermission.ReadAndModify
            )
            .WithHttpContextIdentity(
                new ClaimsIdentity(
                    claims:
                    [
                        new(type: ClaimTypes.Name, value: "Bob52"),
                        new(type: ClaimTypes.Email, value: "bob@example.com"),
                        new(type: ClaimTypes.NameIdentifier, value: "github-33"),
                    ],
                    authenticationType: "GitHub"
                )
            );
        var helper = context.BuildHelper();

        var result = await helper.Execute(cancellationToken: CancellationToken.None);
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new ResolvedUser(
                Id: 42,
                Name: "Bob52",
                EmailAddress: "bob@example.com",
                EntriesPermission: UserPermission.ReadAndModify,
                ExportsPermission: UserPermission.ReadAndModify,
                ProjectsPermission: UserPermission.ReadAndModify,
                UsersPermission: UserPermission.ReadAndModify
            )
        );

        context.HttpContextAccessorMock.Verify(x => x.HttpContext);
        context.GetOrRegisterNewUserUseCaseMock.Verify(x =>
            x.Execute(
                "github-33",
                "bob@example.com",
                "Bob52",
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Theory]
    // Base case
    [InlineData(UserPermission.None, UserPermission.None, UserPermission.None, UserPermission.None)]
    // Entries permission
    [InlineData(UserPermission.Read, UserPermission.None, UserPermission.None, UserPermission.None)]
    [InlineData(
        UserPermission.ReadAndModify,
        UserPermission.None,
        UserPermission.None,
        UserPermission.None
    )]
    // Exports permission
    [InlineData(UserPermission.None, UserPermission.Read, UserPermission.None, UserPermission.None)]
    [InlineData(
        UserPermission.None,
        UserPermission.ReadAndModify,
        UserPermission.None,
        UserPermission.None
    )]
    // Projects permission
    [InlineData(UserPermission.None, UserPermission.None, UserPermission.Read, UserPermission.None)]
    [InlineData(
        UserPermission.None,
        UserPermission.None,
        UserPermission.ReadAndModify,
        UserPermission.None
    )]
    // Users permission
    [InlineData(UserPermission.None, UserPermission.None, UserPermission.None, UserPermission.Read)]
    [InlineData(
        UserPermission.None,
        UserPermission.None,
        UserPermission.None,
        UserPermission.ReadAndModify
    )]
    public async Task Execute_returns_correctly_mapped_user_permissions(
        UserPermission entriesPermission,
        UserPermission exportsPermission,
        UserPermission projectsPermission,
        UserPermission usersPermission
    )
    {
        var context = new TestContext()
            .WithExistingUser(
                id: 42,
                name: "Bob52",
                emailAddress: "bob@example.com",
                entriesPermission: entriesPermission,
                exportsPermission: exportsPermission,
                projectsPermission: projectsPermission,
                usersPermission: usersPermission
            )
            .WithHttpContextIdentity(
                new ClaimsIdentity(
                    claims:
                    [
                        new(type: ClaimTypes.Name, value: "Bob52"),
                        new(type: ClaimTypes.Email, value: "bob@example.com"),
                        new(type: ClaimTypes.NameIdentifier, value: "github-33"),
                    ],
                    authenticationType: "GitHub"
                )
            );
        var helper = context.BuildHelper();

        var result = await helper.Execute(cancellationToken: CancellationToken.None);
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new ResolvedUser(
                Id: 42,
                Name: "Bob52",
                EmailAddress: "bob@example.com",
                EntriesPermission: entriesPermission,
                ExportsPermission: exportsPermission,
                ProjectsPermission: projectsPermission,
                UsersPermission: usersPermission
            )
        );

        context.HttpContextAccessorMock.Verify(x => x.HttpContext);
        context.GetOrRegisterNewUserUseCaseMock.Verify(x =>
            x.Execute(
                "github-33",
                "bob@example.com",
                "Bob52",
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_error_for_user_from_unsupported_identity_provider()
    {
        var context = new TestContext().WithHttpContextIdentity(
            new ClaimsIdentity(
                claims:
                [
                    new(type: ClaimTypes.Name, value: "Bob52"),
                    new(type: ClaimTypes.Email, value: "bob@example.com"),
                    new(type: ClaimTypes.NameIdentifier, value: "github-33"),
                ],
                authenticationType: "Microsoft"
            )
        );
        var helper = context.BuildHelper();

        var result = await helper.Execute(cancellationToken: CancellationToken.None);
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(ResolveUserError.UnsupportedIdentityProvider);

        context.HttpContextAccessorMock.Verify(x => x.HttpContext);
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_error_for_unauthenticated_user()
    {
        var context = new TestContext().WithoutHttpContextIdentity();
        var helper = context.BuildHelper();

        var result = await helper.Execute(cancellationToken: CancellationToken.None);
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(ResolveUserError.UserNotAuthenticated);

        context.HttpContextAccessorMock.Verify(x => x.HttpContext);
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null, "bob@example.com", "github-33")]
    [InlineData("", "bob@example.com", "github-33")]
    [InlineData(" ", "bob@example.com", "github-33")]
    [InlineData("Bob52", null, "github-33")]
    [InlineData("Bob52", "", "github-33")]
    [InlineData("Bob52", " ", "github-33")]
    [InlineData("Bob52", "bob@example.com", null)]
    [InlineData("Bob52", "bob@example.com", "")]
    [InlineData("Bob52", "bob@example.com", " ")]
    public async Task Execute_returns_error_for_user_with_unsupported_values(
        string? name,
        string? email,
        string? subjectClaim
    )
    {
        var claims = new List<Claim>();
        if (name != null)
        {
            claims.Add(new(type: ClaimTypes.Name, value: name));
        }
        if (subjectClaim != null)
        {
            claims.Add(new(type: ClaimTypes.NameIdentifier, value: subjectClaim));
        }
        if (email != null)
        {
            claims.Add(new(type: ClaimTypes.Email, value: email));
        }

        var context = new TestContext().WithHttpContextIdentity(
            new ClaimsIdentity(claims: claims, authenticationType: "GitHub")
        );
        var helper = context.BuildHelper();

        var result = await helper.Execute(cancellationToken: CancellationToken.None);
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(ResolveUserError.MalformedUserClaims);

        context.HttpContextAccessorMock.Verify(x => x.HttpContext);
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(
        GetOrRegisterNewUserUseCaseError.RegistrationFailed,
        ResolveUserError.UserRegistrationFailed
    )]
    [InlineData(GetOrRegisterNewUserUseCaseError.Unknown, ResolveUserError.Unknown)]
    public async Task Execute_returns_error_for_get_or_register_use_case_failures(
        GetOrRegisterNewUserUseCaseError useCaseError,
        ResolveUserError expectedError
    )
    {
        var context = new TestContext()
            .WithGetOrRegisterNewUserUseCaseError(useCaseError)
            .WithHttpContextIdentity(
                new ClaimsIdentity(
                    claims:
                    [
                        new(type: ClaimTypes.Name, value: "Bob52"),
                        new(type: ClaimTypes.Email, value: "bob@example.com"),
                        new(type: ClaimTypes.NameIdentifier, value: "github-33"),
                    ],
                    authenticationType: "GitHub"
                )
            );

        var result = await context.BuildHelper().Execute(CancellationToken.None);
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(expectedError);

        context.HttpContextAccessorMock.Verify(x => x.HttpContext);
        context.GetOrRegisterNewUserUseCaseMock.Verify(x =>
            x.Execute(
                "github-33",
                "bob@example.com",
                "Bob52",
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    private class TestContext
    {
        public Mock<IHttpContextAccessor> HttpContextAccessorMock { get; } =
            new(MockBehavior.Strict);
        public Mock<IGetOrRegisterNewUserUseCase> GetOrRegisterNewUserUseCaseMock { get; } =
            new(MockBehavior.Strict);
        public Mock<ILogger<ResolveUser>> LoggerMock { get; } = new(MockBehavior.Strict);

        public TestContext WithHttpContextIdentity(ClaimsIdentity identity)
        {
            HttpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(
                    new DefaultHttpContext() { User = new ClaimsPrincipal(identity: identity) }
                );

            return this;
        }

        public TestContext WithoutHttpContextIdentity()
        {
            HttpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());

            return this;
        }

        public TestContext WithExistingUser(
            int id,
            string name,
            string emailAddress,
            UserPermission entriesPermission,
            UserPermission exportsPermission,
            UserPermission projectsPermission,
            UserPermission usersPermission
        )
        {
            return WithGetOrRegisterNewUserUseCaseSuccess(
                id: id,
                name: name,
                emailAddress: emailAddress,
                entriesPermission: entriesPermission,
                exportsPermission: exportsPermission,
                projectsPermission: projectsPermission,
                usersPermission: usersPermission,
                newlyRegistered: false
            );
        }

        public TestContext WithNewlyRegisteredUser(
            int id,
            string name,
            string emailAddress,
            UserPermission entriesPermission,
            UserPermission exportsPermission,
            UserPermission projectsPermission,
            UserPermission usersPermission
        )
        {
            return WithGetOrRegisterNewUserUseCaseSuccess(
                id: id,
                name: name,
                emailAddress: emailAddress,
                entriesPermission: entriesPermission,
                exportsPermission: exportsPermission,
                projectsPermission: projectsPermission,
                usersPermission: usersPermission,
                newlyRegistered: true
            );
        }

        private TestContext WithGetOrRegisterNewUserUseCaseSuccess(
            int id,
            string name,
            string emailAddress,
            UserPermission entriesPermission,
            UserPermission exportsPermission,
            UserPermission projectsPermission,
            UserPermission usersPermission,
            bool newlyRegistered
        ) =>
            WithGetOrRegisterNewUserUseCaseResult(
                Result.Success<GetOrRegisterNewUserUseCaseOutput, GetOrRegisterNewUserUseCaseError>(
                    new GetOrRegisterNewUserUseCaseOutput(
                        User: new(
                            Id: id,
                            Name: name,
                            EmailAddress: emailAddress,
                            IdentityOrigin: UserIdentityProvider.GitHub,
                            EntriesPermission: entriesPermission,
                            ExportsPermission: exportsPermission,
                            ProjectsPermission: projectsPermission,
                            UsersPermission: usersPermission
                        ),
                        NewlyRegistered: newlyRegistered
                    )
                )
            );

        public TestContext WithGetOrRegisterNewUserUseCaseError(
            GetOrRegisterNewUserUseCaseError error
        ) =>
            WithGetOrRegisterNewUserUseCaseResult(
                Result.Failure<GetOrRegisterNewUserUseCaseOutput, GetOrRegisterNewUserUseCaseError>(
                    error
                )
            );

        private TestContext WithGetOrRegisterNewUserUseCaseResult(
            IValueOrErrorResult<
                GetOrRegisterNewUserUseCaseOutput,
                GetOrRegisterNewUserUseCaseError
            > result
        )
        {
            GetOrRegisterNewUserUseCaseMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<UserIdentityProvider>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(result);

            return this;
        }

        public ResolveUser BuildHelper() =>
            new(
                httpContextAccessor: HttpContextAccessorMock.Object,
                getOrRegisterNewUserUseCase: GetOrRegisterNewUserUseCaseMock.Object,
                logger: LoggerMock.Object
            );

        public void VerifyNoOtherCalls()
        {
            HttpContextAccessorMock.VerifyNoOtherCalls();
            GetOrRegisterNewUserUseCaseMock.VerifyNoOtherCalls();
        }
    }
}
