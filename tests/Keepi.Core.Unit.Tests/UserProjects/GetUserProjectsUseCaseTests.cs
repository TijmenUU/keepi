using Keepi.Core.UserProjects;

namespace Keepi.Core.Unit.Tests.UserProjects;

public class GetUserProjectsUseCaseTests
{
    [Fact]
    public async Task Execute_returns_success_output()
    {
        var context = new TestContext().WithGetUserProjectsSuccessResult(
            new(
                Projects:
                [
                    new(
                        Id: 1,
                        Name: "Algemeen",
                        Enabled: true,
                        InvoiceItems: [new(Id: 10, Name: "Dev")]
                    ),
                    new(
                        Id: 2,
                        Name: "Intern",
                        Enabled: true,
                        InvoiceItems: [new(Id: 20, Name: "Administratie")]
                    ),
                ],
                Customizations:
                [
                    new(InvoiceItemId: 10, Ordinal: 981, Color: Color.FromUint32(0xFF0000u)),
                    new(InvoiceItemId: 20, Ordinal: 982, Color: Color.FromUint32(0xFF00u)),
                ]
            )
        );

        var result = await context
            .BuildUseCase()
            .Execute(userId: 42, cancellationToken: CancellationToken.None);

        result.TrySuccess(out var successResult, out _).ShouldBeTrue();
        successResult.ShouldBeEquivalentTo(
            new GetUserProjectsUseCaseOutput(
                Projects:
                [
                    new GetUserProjectsUseCaseOutputProject(
                        Id: 1,
                        Name: "Algemeen",
                        Enabled: true,
                        InvoiceItems:
                        [
                            new(
                                Id: 10,
                                Name: "Dev",
                                Customization: new(Ordinal: 981, Color: Color.FromUint32(0xFF0000u))
                            ),
                        ]
                    ),
                    new GetUserProjectsUseCaseOutputProject(
                        Id: 2,
                        Name: "Intern",
                        Enabled: true,
                        InvoiceItems:
                        [
                            new(
                                Id: 20,
                                Name: "Administratie",
                                Customization: new(Ordinal: 982, Color: Color.FromUint32(0xFF00u))
                            ),
                        ]
                    ),
                ]
            )
        );
    }

    [Fact]
    public async Task Execute_returns_success_with_default_customization()
    {
        var context = new TestContext().WithGetUserProjectsSuccessResult(
            new(
                Projects:
                [
                    new(
                        Id: 1,
                        Name: "Algemeen",
                        Enabled: true,
                        InvoiceItems: [new(Id: 10, Name: "Dev")]
                    ),
                ],
                Customizations: []
            )
        );

        var result = await context
            .BuildUseCase()
            .Execute(userId: 42, cancellationToken: CancellationToken.None);

        result.TrySuccess(out var successResult, out _).ShouldBeTrue();
        successResult.ShouldBeEquivalentTo(
            new GetUserProjectsUseCaseOutput(
                Projects:
                [
                    new GetUserProjectsUseCaseOutputProject(
                        Id: 1,
                        Name: "Algemeen",
                        Enabled: true,
                        InvoiceItems:
                        [
                            new(
                                Id: 10,
                                Name: "Dev",
                                Customization: new(Ordinal: int.MaxValue, Color: null)
                            ),
                        ]
                    ),
                ]
            )
        );
    }

    [Fact]
    public async Task Execute_returns_unknown_get_user_projects_error()
    {
        var context = new TestContext().WithGetUserProjectsFailureResult(
            GetUserProjectsError.Unknown
        );

        var result = await context
            .BuildUseCase()
            .Execute(userId: 42, cancellationToken: CancellationToken.None);

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(GetUserProjectsUseCaseError.Unknown);
    }

    private class TestContext
    {
        public Mock<IGetUserProjects> GetUserProjectsMock { get; } = new(MockBehavior.Strict);

        public TestContext WithGetUserProjectsSuccessResult(GetUserProjectResult result)
        {
            GetUserProjectsMock
                .Setup(x => x.Execute(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success<GetUserProjectResult, GetUserProjectsError>(result));

            return this;
        }

        public TestContext WithGetUserProjectsFailureResult(GetUserProjectsError result)
        {
            GetUserProjectsMock
                .Setup(x => x.Execute(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<GetUserProjectResult, GetUserProjectsError>(result));

            return this;
        }

        public GetUserProjectsUseCase BuildUseCase() =>
            new(getUserProjects: GetUserProjectsMock.Object);
    }
}
