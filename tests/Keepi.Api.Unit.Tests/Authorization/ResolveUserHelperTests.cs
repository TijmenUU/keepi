using System.Security.Claims;
using Keepi.Api.Authorization;
using Keepi.Core;
using Keepi.Core.Users;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.Unit.Tests.Authorization;

public class ResolveUserHelperTests
{
    [Fact]
    public async Task GetUserOrNull_returns_expected_user_for_existing_user()
    {
        var context = new TestContext().WithExistingUser(
            id: 42,
            name: "Bob52",
            emailAddress: "bob@example.com"
        );
        var helper = context.BuildHelper();

        var claimsPrincipal = new ClaimsPrincipal(
            identity: new ClaimsIdentity(
                claims:
                [
                    new(type: ClaimTypes.Name, value: "Bob52"),
                    new(type: ClaimTypes.Email, value: "bob@example.com"),
                    new(type: ClaimTypes.NameIdentifier, value: "github-33"),
                ],
                authenticationType: "GitHub"
            )
        );
        var result = await helper.GetUserOrNull(
            userClaimsPrincipal: claimsPrincipal,
            cancellationToken: CancellationToken.None
        );

        result.ShouldBeEquivalentTo(
            new ResolvedUser(Id: 42, Name: "Bob52", EmailAddress: "bob@example.com")
        );

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
    public async Task GetUserOrNull_returns_newly_registered_user_for_new_user()
    {
        var context = new TestContext().WithNewlyRegisteredUser(
            id: 42,
            name: "Bob52",
            emailAddress: "bob@example.com"
        );
        var helper = context.BuildHelper();

        var claimsPrincipal = new ClaimsPrincipal(
            identity: new ClaimsIdentity(
                claims:
                [
                    new(type: ClaimTypes.Name, value: "Bob52"),
                    new(type: ClaimTypes.Email, value: "bob@example.com"),
                    new(type: ClaimTypes.NameIdentifier, value: "github-33"),
                ],
                authenticationType: "GitHub"
            )
        );
        var result = await helper.GetUserOrNull(
            userClaimsPrincipal: claimsPrincipal,
            cancellationToken: CancellationToken.None
        );

        result.ShouldBeEquivalentTo(
            new ResolvedUser(Id: 42, Name: "Bob52", EmailAddress: "bob@example.com")
        );

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
    public async Task GetUserOrNull_returns_null_for_user_from_unsupported_identity_provider()
    {
        var context = new TestContext().WithExistingUser(
            id: 42,
            name: "Bob52",
            emailAddress: "bob@example.com"
        );
        var helper = context.BuildHelper();

        var claimsPrincipal = new ClaimsPrincipal(
            identity: new ClaimsIdentity(
                claims:
                [
                    new(type: ClaimTypes.Name, value: "Bob52"),
                    new(type: ClaimTypes.Email, value: "bob@example.com"),
                    new(type: ClaimTypes.NameIdentifier, value: "github-33"),
                ],
                authenticationType: "Microsoft"
            )
        );
        var result = await helper.GetUserOrNull(
            userClaimsPrincipal: claimsPrincipal,
            cancellationToken: CancellationToken.None
        );

        result.ShouldBeNull();

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
    public async Task GetUserOrNull_returns_null_for_user_with_unsupported_values(
        string? name,
        string? email,
        string? subjectClaim
    )
    {
        var context = new TestContext().WithExistingUser(
            id: 42,
            name: "Bob52",
            emailAddress: "bob@example.com"
        );
        var helper = context.BuildHelper();

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

        var claimsPrincipal = new ClaimsPrincipal(
            identity: new ClaimsIdentity(claims: claims, authenticationType: "GitHub")
        );
        var result = await helper.GetUserOrNull(
            userClaimsPrincipal: claimsPrincipal,
            cancellationToken: CancellationToken.None
        );

        result.ShouldBeNull();

        context.VerifyNoOtherCalls();
    }

    private class TestContext
    {
        public Mock<IGetOrRegisterNewUserUseCase> GetOrRegisterNewUserUseCaseMock { get; } =
            new(MockBehavior.Strict);
        public Mock<ILogger<ResolveUserHelper>> LoggerMock { get; } = new(MockBehavior.Strict);

        public TestContext WithExistingUser(int id, string name, string emailAddress)
        {
            return WithGetOrRegisterNewUserUseCaseResult(
                id: id,
                name: name,
                emailAddress: emailAddress,
                newlyRegistered: false
            );
        }

        public TestContext WithNewlyRegisteredUser(int id, string name, string emailAddress)
        {
            return WithGetOrRegisterNewUserUseCaseResult(
                id: id,
                name: name,
                emailAddress: emailAddress,
                newlyRegistered: true
            );
        }

        private TestContext WithGetOrRegisterNewUserUseCaseResult(
            int id,
            string name,
            string emailAddress,
            bool newlyRegistered
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
                .ReturnsAsync(
                    Result.Success<
                        GetOrRegisterNewUserUseCaseOutput,
                        GetOrRegisterNewUserUseCaseError
                    >(
                        new GetOrRegisterNewUserUseCaseOutput(
                            User: new(
                                Id: id,
                                Name: name,
                                EmailAddress: emailAddress,
                                IdentityOrigin: UserIdentityProvider.GitHub,
                                EntriesPermission: UserPermission.ReadAndModify,
                                ExportsPermission: UserPermission.ReadAndModify,
                                ProjectsPermission: UserPermission.ReadAndModify,
                                UsersPermission: UserPermission.ReadAndModify
                            ),
                            NewlyRegistered: newlyRegistered
                        )
                    )
                );

            return this;
        }

        public ResolveUserHelper BuildHelper() =>
            new(getOrRegisterNewUserUseCase: GetOrRegisterNewUserUseCaseMock.Object);

        public void VerifyNoOtherCalls()
        {
            GetOrRegisterNewUserUseCaseMock.VerifyNoOtherCalls();
        }
    }
}
