using System.Text;
using Keepi.Core.Entries;
using Keepi.Core.Unit.Tests.Builders;
using Keepi.Core.UserProjects;
using Keepi.Core.Users;
using Microsoft.Extensions.Logging;

namespace Keepi.Core.Unit.Tests.Entries;

public class UpdateWeekUserEntriesUseCaseTests
{
    [Fact]
    public async Task Execute_stores_expected_entities()
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithUserProjectsSuccessResult(
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
                        new(
                            InvoiceItemId: 10,
                            Ordinal: 981,
                            Color: new(Red: 255, Green: 255, Blue: 255)
                        ),
                        new(
                            InvoiceItemId: 20,
                            Ordinal: 982,
                            Color: new(Red: 255, Green: 255, Blue: 255)
                        ),
                    ]
                )
            )
            .WithDeleteUserEntriesResult(Result.Success<DeleteUserEntriesForDateRangeError>())
            .WithSaveUserEntriesResult(Result.Success<SaveUserEntriesError>());

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            year: 2025,
            weekNumber: 25,
            input: new UpdateWeekUserEntriesUseCaseInput(
                Monday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: 10,
                            Minutes: 60,
                            Remark: "Nieuwe feature"
                        ),
                    ]
                ),
                Tuesday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: 10,
                            Minutes: 60,
                            Remark: "Nieuwe feature"
                        ),
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: 20,
                            Minutes: 30,
                            Remark: "Project Flyby"
                        ),
                    ]
                ),
                Wednesday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: 20,
                            Minutes: 15,
                            Remark: null
                        ),
                    ]
                ),
                Thursday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Friday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Saturday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Sunday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [])
            ),
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeTrue();

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.GetUserProjectsMock.Verify(x => x.Execute(42, It.IsAny<CancellationToken>()));
        context.DeleteUserEntriesForDateRangeMock.Verify(x =>
            x.Execute(
                It.Is<DeleteUserEntriesForDateRangeInput>(i =>
                    i.UserId == 42
                    && i.From == new DateOnly(2025, 6, 16)
                    && i.ToInclusive == new DateOnly(2025, 6, 22)
                    && i.ProjectIds.Length == 2
                    && i.ProjectIds[0] == 1
                    && i.ProjectIds[1] == 2
                ),
                It.IsAny<CancellationToken>()
            )
        );
        context.SaveUserEntriesMock.Verify(x =>
            x.Execute(
                It.Is<SaveUserEntriesInput>(i =>
                    i.UserId == 42
                    && i.Entries.Length == 4
                    && i.Entries[0].InvoiceItemId == 10
                    && i.Entries[0].Date == new DateOnly(2025, 6, 16)
                    && i.Entries[0].Minutes == 60
                    && i.Entries[0].Remark == "Nieuwe feature"
                    && i.Entries[1].InvoiceItemId == 10
                    && i.Entries[1].Date == new DateOnly(2025, 6, 17)
                    && i.Entries[1].Minutes == 60
                    && i.Entries[1].Remark == "Nieuwe feature"
                    && i.Entries[2].InvoiceItemId == 20
                    && i.Entries[2].Date == new DateOnly(2025, 6, 17)
                    && i.Entries[2].Minutes == 30
                    && i.Entries[2].Remark == "Project Flyby"
                    && i.Entries[3].InvoiceItemId == 20
                    && i.Entries[3].Date == new DateOnly(2025, 6, 18)
                    && i.Entries[3].Minutes == 15
                    && i.Entries[3].Remark == null
                ),
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_only_deleted_enabled_projects()
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithUserProjectsSuccessResult(
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
                            Enabled: false,
                            InvoiceItems: [new(Id: 20, Name: "Administratie")]
                        ),
                    ],
                    Customizations:
                    [
                        new(
                            InvoiceItemId: 10,
                            Ordinal: 981,
                            Color: new(Red: 255, Green: 255, Blue: 255)
                        ),
                        new(
                            InvoiceItemId: 20,
                            Ordinal: 982,
                            Color: new(Red: 255, Green: 255, Blue: 255)
                        ),
                    ]
                )
            )
            .WithDeleteUserEntriesResult(Result.Success<DeleteUserEntriesForDateRangeError>())
            .WithSaveUserEntriesResult(Result.Success<SaveUserEntriesError>());

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            year: 2025,
            weekNumber: 25,
            input: new UpdateWeekUserEntriesUseCaseInput(
                Monday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: 10,
                            Minutes: 60,
                            Remark: "Nieuwe feature"
                        ),
                    ]
                ),
                Tuesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Wednesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Thursday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Friday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Saturday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Sunday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [])
            ),
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeTrue();

        context.DeleteUserEntriesForDateRangeMock.Verify(x =>
            x.Execute(
                It.Is<DeleteUserEntriesForDateRangeInput>(i =>
                    i.ProjectIds.Length == 1 && i.ProjectIds[0] == 1
                ),
                It.IsAny<CancellationToken>()
            )
        );
    }

    [Fact]
    public async Task Execute_returns_error_for_non_existing_invoice_item()
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithUserProjectsSuccessResult(
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
                    Customizations:
                    [
                        new(
                            InvoiceItemId: 10,
                            Ordinal: 981,
                            Color: new(Red: 255, Green: 255, Blue: 255)
                        ),
                    ]
                )
            );

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            year: 2025,
            weekNumber: 25,
            input: new UpdateWeekUserEntriesUseCaseInput(
                Monday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: 20,
                            Minutes: 60,
                            Remark: null
                        ),
                    ]
                ),
                Tuesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Wednesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Thursday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Friday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Saturday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Sunday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [])
            ),
            cancellationToken: CancellationToken.None
        );

        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateWeekUserEntriesUseCaseError.UnknownUserInvoiceItem);

        context.DeleteUserEntriesForDateRangeMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_error_for_disabled_project()
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithUserProjectsSuccessResult(
                new(
                    Projects:
                    [
                        new(
                            Id: 1,
                            Name: "Algemeen",
                            Enabled: false,
                            InvoiceItems: [new(Id: 10, Name: "Dev")]
                        ),
                    ],
                    Customizations:
                    [
                        new(
                            InvoiceItemId: 10,
                            Ordinal: 981,
                            Color: new(Red: 255, Green: 255, Blue: 255)
                        ),
                    ]
                )
            );

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            year: 2025,
            weekNumber: 25,
            input: new UpdateWeekUserEntriesUseCaseInput(
                Monday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: 10,
                            Minutes: 60,
                            Remark: null
                        ),
                    ]
                ),
                Tuesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Wednesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Thursday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Friday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Saturday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Sunday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [])
            ),
            cancellationToken: CancellationToken.None
        );

        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateWeekUserEntriesUseCaseError.InvalidUserInvoiceItem);

        context.DeleteUserEntriesForDateRangeMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_error_for_invalid_minutes()
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithUserProjectsSuccessResult(
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
                    Customizations:
                    [
                        new(
                            InvoiceItemId: 10,
                            Ordinal: 981,
                            Color: new(Red: 255, Green: 255, Blue: 255)
                        ),
                    ]
                )
            );

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            year: 2025,
            weekNumber: 25,
            input: new UpdateWeekUserEntriesUseCaseInput(
                Monday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: 10,
                            Minutes: -45,
                            Remark: null
                        ),
                    ]
                ),
                Tuesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Wednesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Thursday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Friday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Saturday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Sunday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [])
            ),
            cancellationToken: CancellationToken.None
        );

        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateWeekUserEntriesUseCaseError.InvalidMinutes);

        context.DeleteUserEntriesForDateRangeMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_error_for_invalid_remark()
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithUserProjectsSuccessResult(
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
                    Customizations:
                    [
                        new(
                            InvoiceItemId: 10,
                            Ordinal: 981,
                            Color: new(Red: 255, Green: 255, Blue: 255)
                        ),
                    ]
                )
            );

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            year: 2025,
            weekNumber: 25,
            input: new UpdateWeekUserEntriesUseCaseInput(
                Monday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: 10,
                            Minutes: 60,
                            Remark: new StringBuilder()
                                .Append(value: 'a', repeatCount: 260)
                                .ToString()
                        ),
                    ]
                ),
                Tuesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Wednesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Thursday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Friday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Saturday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Sunday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [])
            ),
            cancellationToken: CancellationToken.None
        );

        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateWeekUserEntriesUseCaseError.InvalidRemark);

        context.DeleteUserEntriesForDateRangeMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_unknown_get_user_projects_error()
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithUserProjectsFailureResult(GetUserProjectsError.Unknown);

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            year: 2025,
            weekNumber: 25,
            input: new UpdateWeekUserEntriesUseCaseInput(
                Monday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: 10,
                            Minutes: 60,
                            Remark: null
                        ),
                    ]
                ),
                Tuesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Wednesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Thursday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Friday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Saturday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Sunday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [])
            ),
            cancellationToken: CancellationToken.None
        );

        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateWeekUserEntriesUseCaseError.Unknown);
    }

    [Fact]
    public async Task Execute_returns_delete_user_entries_error()
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithUserProjectsSuccessResult(
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
                    Customizations:
                    [
                        new(
                            InvoiceItemId: 10,
                            Ordinal: 981,
                            Color: new(Red: 255, Green: 255, Blue: 255)
                        ),
                    ]
                )
            )
            .WithDeleteUserEntriesResult(
                Result.Failure(DeleteUserEntriesForDateRangeError.Unknown)
            );

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            year: 2025,
            weekNumber: 25,
            input: new UpdateWeekUserEntriesUseCaseInput(
                Monday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: 10,
                            Minutes: 60,
                            Remark: null
                        ),
                    ]
                ),
                Tuesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Wednesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Thursday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Friday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Saturday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Sunday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [])
            ),
            cancellationToken: CancellationToken.None
        );

        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateWeekUserEntriesUseCaseError.Unknown);
    }

    [Fact]
    public async Task Execute_returns_save_user_entries_error()
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithUserProjectsSuccessResult(
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
                    Customizations:
                    [
                        new(
                            InvoiceItemId: 10,
                            Ordinal: 981,
                            Color: new(Red: 255, Green: 255, Blue: 255)
                        ),
                    ]
                )
            )
            .WithDeleteUserEntriesResult(Result.Success<DeleteUserEntriesForDateRangeError>())
            .WithSaveUserEntriesResult(Result.Failure(SaveUserEntriesError.Unknown));

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            year: 2025,
            weekNumber: 25,
            input: new UpdateWeekUserEntriesUseCaseInput(
                Monday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: 10,
                            Minutes: 60,
                            Remark: null
                        ),
                    ]
                ),
                Tuesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Wednesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Thursday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Friday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Saturday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                Sunday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [])
            ),
            cancellationToken: CancellationToken.None
        );

        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateWeekUserEntriesUseCaseError.Unknown);
    }

    [Theory]
    [InlineData(
        ResolveUserError.UserNotAuthenticated,
        UpdateWeekUserEntriesUseCaseError.UnauthenticatedUser
    )]
    [InlineData(ResolveUserError.MalformedUserClaims, UpdateWeekUserEntriesUseCaseError.Unknown)]
    [InlineData(
        ResolveUserError.UnsupportedIdentityProvider,
        UpdateWeekUserEntriesUseCaseError.Unknown
    )]
    [InlineData(ResolveUserError.UserNotFound, UpdateWeekUserEntriesUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserRegistrationFailed, UpdateWeekUserEntriesUseCaseError.Unknown)]
    public async Task Execute_returns_error_for_user_resolve_error(
        ResolveUserError resolveUserError,
        UpdateWeekUserEntriesUseCaseError expectedError
    )
    {
        var context = new TestContext().WithResolveUserError(resolveUserError);

        var result = await context
            .BuildUseCase()
            .Execute(
                year: 2025,
                weekNumber: 25,
                input: new UpdateWeekUserEntriesUseCaseInput(
                    Monday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                    Tuesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                    Wednesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                    Thursday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                    Friday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                    Saturday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                    Sunday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [])
                ),
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(expectedError);
    }

    [Theory]
    [InlineData(UserPermission.None)]
    [InlineData(UserPermission.Read)]
    public async Task Execute_returns_error_for_unauthorized_user(UserPermission entriesPermission)
    {
        var context = new TestContext().WithResolvedUser(
            user: ResolvedUserBuilder
                .AsAdministratorBob()
                .WithEntriesPermission(entriesPermission)
                .Build()
        );

        var result = await context
            .BuildUseCase()
            .Execute(
                year: 2025,
                weekNumber: 25,
                input: new UpdateWeekUserEntriesUseCaseInput(
                    Monday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                    Tuesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                    Wednesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                    Thursday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                    Friday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                    Saturday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
                    Sunday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [])
                ),
                cancellationToken: CancellationToken.None
            );
        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateWeekUserEntriesUseCaseError.UnauthorizedUser);
    }

    private class TestContext
    {
        public Mock<IResolveUser> ResolveUserMock { get; } = new(MockBehavior.Strict);
        public Mock<IGetUserProjects> GetUserProjectsMock { get; } = new(MockBehavior.Strict);
        public Mock<IDeleteUserEntriesForDateRange> DeleteUserEntriesForDateRangeMock { get; } =
            new(MockBehavior.Strict);
        public Mock<ISaveUserEntries> SaveUserEntriesMock { get; } = new(MockBehavior.Strict);

        public Mock<ILogger<UpdateWeekUserEntriesUseCase>> LoggerMock = new(MockBehavior.Loose);

        public TestContext WithResolvedUser(ResolvedUser user)
        {
            ResolveUserMock
                .Setup(x => x.Execute(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success<ResolvedUser, ResolveUserError>(user));

            return this;
        }

        public TestContext WithResolveUserError(ResolveUserError error)
        {
            ResolveUserMock
                .Setup(x => x.Execute(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<ResolvedUser, ResolveUserError>(error));

            return this;
        }

        public TestContext WithUserProjectsSuccessResult(GetUserProjectResult result)
        {
            GetUserProjectsMock
                .Setup(x => x.Execute(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success<GetUserProjectResult, GetUserProjectsError>(result));

            return this;
        }

        public TestContext WithUserProjectsFailureResult(GetUserProjectsError result)
        {
            GetUserProjectsMock
                .Setup(x => x.Execute(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<GetUserProjectResult, GetUserProjectsError>(result));

            return this;
        }

        public TestContext WithDeleteUserEntriesResult(
            IMaybeErrorResult<DeleteUserEntriesForDateRangeError> result
        )
        {
            DeleteUserEntriesForDateRangeMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<DeleteUserEntriesForDateRangeInput>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(result);

            return this;
        }

        public TestContext WithSaveUserEntriesResult(IMaybeErrorResult<SaveUserEntriesError> result)
        {
            SaveUserEntriesMock
                .Setup(x =>
                    x.Execute(It.IsAny<SaveUserEntriesInput>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(result);

            return this;
        }

        public UpdateWeekUserEntriesUseCase BuildUseCase() =>
            new(
                resolveUser: ResolveUserMock.Object,
                getUserProjects: GetUserProjectsMock.Object,
                deleteUserEntriesForDateRange: DeleteUserEntriesForDateRangeMock.Object,
                saveUserEntries: SaveUserEntriesMock.Object,
                logger: LoggerMock.Object
            );

        public void VerifyNoOtherCalls()
        {
            ResolveUserMock.VerifyNoOtherCalls();
            GetUserProjectsMock.VerifyNoOtherCalls();
            DeleteUserEntriesForDateRangeMock.VerifyNoOtherCalls();
            SaveUserEntriesMock.VerifyNoOtherCalls();
        }
    }
}
