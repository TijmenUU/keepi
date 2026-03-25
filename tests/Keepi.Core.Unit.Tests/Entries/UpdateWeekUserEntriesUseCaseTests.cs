using Keepi.Core.Entries;
using Keepi.Core.InvoiceItems;
using Keepi.Core.Projects;
using Keepi.Core.Unit.Tests.Builders;
using Keepi.Core.UserInvoiceItemCustomizations;
using Keepi.Core.UserProjects;
using Keepi.Core.Users;
using Keepi.Generators;

namespace Keepi.Core.Unit.Tests.Entries;

public class UpdateWeekUserEntriesUseCaseTests
{
    [Fact]
    public async Task Execute_stores_expected_entities()
    {
        var context = new UpdateWeekUserEntriesUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithGetUserProjectsSuccess(
                new(
                    Projects:
                    [
                        new(
                            Id: ProjectId.From(1),
                            Name: ProjectName.From("Algemeen"),
                            Enabled: true,
                            InvoiceItems:
                            [
                                new(Id: InvoiceItemId.From(10), Name: InvoiceItemName.From("Dev")),
                            ]
                        ),
                        new(
                            Id: ProjectId.From(2),
                            Name: ProjectName.From("Intern"),
                            Enabled: true,
                            InvoiceItems:
                            [
                                new(
                                    Id: InvoiceItemId.From(20),
                                    Name: InvoiceItemName.From("Administratie")
                                ),
                            ]
                        ),
                    ],
                    Customizations:
                    [
                        new(
                            InvoiceItemId: InvoiceItemId.From(10),
                            Ordinal: UserInvoiceITemCustomizationOrdinal.From(981),
                            Color: Color.From(0xFFFFFF)
                        ),
                        new(
                            InvoiceItemId: InvoiceItemId.From(20),
                            Ordinal: UserInvoiceITemCustomizationOrdinal.From(982),
                            Color: Color.From(0xFFFFFF)
                        ),
                    ]
                )
            )
            .WithDeleteUserEntriesForDateRangeSuccess()
            .WithSaveUserEntriesSuccess();

        var useCase = context.BuildTarget();

        var result = await useCase.Execute(
            year: Year.From(2025),
            weekNumber: WeekNumber.From(25),
            input: new UpdateWeekUserEntriesUseCaseInput(
                Monday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: InvoiceItemId.From(10),
                            Minutes: UserEntryMinutes.From(60),
                            Remark: UserEntryRemark.From("Nieuwe feature")
                        ),
                    ]
                ),
                Tuesday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: InvoiceItemId.From(10),
                            Minutes: UserEntryMinutes.From(60),
                            Remark: UserEntryRemark.From("Nieuwe feature")
                        ),
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: InvoiceItemId.From(20),
                            Minutes: UserEntryMinutes.From(30),
                            Remark: UserEntryRemark.From("Project Flyby")
                        ),
                    ]
                ),
                Wednesday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: InvoiceItemId.From(20),
                            Minutes: UserEntryMinutes.From(15),
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
        context.GetUserProjectsMock.Verify(x =>
            x.Execute(UserId.From(42), It.IsAny<CancellationToken>())
        );
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
                    && i.Entries[0].Remark == UserEntryRemark.From("Nieuwe feature")
                    && i.Entries[1].InvoiceItemId == 10
                    && i.Entries[1].Date == new DateOnly(2025, 6, 17)
                    && i.Entries[1].Minutes == 60
                    && i.Entries[1].Remark == UserEntryRemark.From("Nieuwe feature")
                    && i.Entries[2].InvoiceItemId == 20
                    && i.Entries[2].Date == new DateOnly(2025, 6, 17)
                    && i.Entries[2].Minutes == 30
                    && i.Entries[2].Remark == UserEntryRemark.From("Project Flyby")
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
        var context = new UpdateWeekUserEntriesUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithGetUserProjectsSuccess(
                new(
                    Projects:
                    [
                        new(
                            Id: ProjectId.From(1),
                            Name: ProjectName.From("Algemeen"),
                            Enabled: true,
                            InvoiceItems:
                            [
                                new(Id: InvoiceItemId.From(10), Name: InvoiceItemName.From("Dev")),
                            ]
                        ),
                        new(
                            Id: ProjectId.From(2),
                            Name: ProjectName.From("Intern"),
                            Enabled: false,
                            InvoiceItems:
                            [
                                new(
                                    Id: InvoiceItemId.From(20),
                                    Name: InvoiceItemName.From("Administratie")
                                ),
                            ]
                        ),
                    ],
                    Customizations:
                    [
                        new(
                            InvoiceItemId: InvoiceItemId.From(10),
                            Ordinal: UserInvoiceITemCustomizationOrdinal.From(981),
                            Color: Color.From(0xFFFFFF)
                        ),
                        new(
                            InvoiceItemId: InvoiceItemId.From(20),
                            Ordinal: UserInvoiceITemCustomizationOrdinal.From(982),
                            Color: Color.From(0xFFFFFF)
                        ),
                    ]
                )
            )
            .WithDeleteUserEntriesForDateRangeSuccess()
            .WithSaveUserEntriesSuccess();

        var useCase = context.BuildTarget();

        var result = await useCase.Execute(
            year: Year.From(2025),
            weekNumber: WeekNumber.From(25),
            input: new UpdateWeekUserEntriesUseCaseInput(
                Monday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: InvoiceItemId.From(10),
                            Minutes: UserEntryMinutes.From(60),
                            Remark: UserEntryRemark.From("Nieuwe feature")
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
        var context = new UpdateWeekUserEntriesUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithGetUserProjectsSuccess(
                new(
                    Projects:
                    [
                        new(
                            Id: ProjectId.From(1),
                            Name: ProjectName.From("Algemeen"),
                            Enabled: true,
                            InvoiceItems:
                            [
                                new(Id: InvoiceItemId.From(10), Name: InvoiceItemName.From("Dev")),
                            ]
                        ),
                    ],
                    Customizations:
                    [
                        new(
                            InvoiceItemId: InvoiceItemId.From(10),
                            Ordinal: UserInvoiceITemCustomizationOrdinal.From(981),
                            Color: Color.From(0xFFFFFF)
                        ),
                    ]
                )
            );

        var useCase = context.BuildTarget();

        var result = await useCase.Execute(
            year: Year.From(2025),
            weekNumber: WeekNumber.From(25),
            input: new UpdateWeekUserEntriesUseCaseInput(
                Monday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: InvoiceItemId.From(20),
                            Minutes: UserEntryMinutes.From(60),
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
        var context = new UpdateWeekUserEntriesUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithGetUserProjectsSuccess(
                new(
                    Projects:
                    [
                        new(
                            Id: ProjectId.From(1),
                            Name: ProjectName.From("Algemeen"),
                            Enabled: false,
                            InvoiceItems:
                            [
                                new(Id: InvoiceItemId.From(10), Name: InvoiceItemName.From("Dev")),
                            ]
                        ),
                    ],
                    Customizations:
                    [
                        new(
                            InvoiceItemId: InvoiceItemId.From(10),
                            Ordinal: UserInvoiceITemCustomizationOrdinal.From(981),
                            Color: Color.From(0xFFFFFF)
                        ),
                    ]
                )
            );

        var useCase = context.BuildTarget();

        var result = await useCase.Execute(
            year: Year.From(2025),
            weekNumber: WeekNumber.From(25),
            input: new UpdateWeekUserEntriesUseCaseInput(
                Monday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: InvoiceItemId.From(10),
                            Minutes: UserEntryMinutes.From(60),
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
    public async Task Execute_returns_unknown_get_user_projects_error()
    {
        var context = new UpdateWeekUserEntriesUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithGetUserProjectsError(GetUserProjectsError.Unknown);

        var useCase = context.BuildTarget();

        var result = await useCase.Execute(
            year: Year.From(2025),
            weekNumber: WeekNumber.From(25),
            input: new UpdateWeekUserEntriesUseCaseInput(
                Monday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: InvoiceItemId.From(10),
                            Minutes: UserEntryMinutes.From(60),
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
        var context = new UpdateWeekUserEntriesUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithGetUserProjectsSuccess(
                new(
                    Projects:
                    [
                        new(
                            Id: ProjectId.From(1),
                            Name: ProjectName.From("Algemeen"),
                            Enabled: true,
                            InvoiceItems:
                            [
                                new(Id: InvoiceItemId.From(10), Name: InvoiceItemName.From("Dev")),
                            ]
                        ),
                    ],
                    Customizations:
                    [
                        new(
                            InvoiceItemId: InvoiceItemId.From(10),
                            Ordinal: UserInvoiceITemCustomizationOrdinal.From(981),
                            Color: Color.From(0xFFFFFF)
                        ),
                    ]
                )
            )
            .WithDeleteUserEntriesForDateRangeError(DeleteUserEntriesForDateRangeError.Unknown);

        var useCase = context.BuildTarget();

        var result = await useCase.Execute(
            year: Year.From(2025),
            weekNumber: WeekNumber.From(25),
            input: new UpdateWeekUserEntriesUseCaseInput(
                Monday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: InvoiceItemId.From(10),
                            Minutes: UserEntryMinutes.From(60),
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
        var context = new UpdateWeekUserEntriesUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithGetUserProjectsSuccess(
                new(
                    Projects:
                    [
                        new(
                            Id: ProjectId.From(1),
                            Name: ProjectName.From("Algemeen"),
                            Enabled: true,
                            InvoiceItems:
                            [
                                new(Id: InvoiceItemId.From(10), Name: InvoiceItemName.From("Dev")),
                            ]
                        ),
                    ],
                    Customizations:
                    [
                        new(
                            InvoiceItemId: InvoiceItemId.From(10),
                            Ordinal: UserInvoiceITemCustomizationOrdinal.From(981),
                            Color: Color.From(0xFFFFFF)
                        ),
                    ]
                )
            )
            .WithDeleteUserEntriesForDateRangeSuccess()
            .WithSaveUserEntriesError(SaveUserEntriesError.Unknown);

        var useCase = context.BuildTarget();

        var result = await useCase.Execute(
            year: Year.From(2025),
            weekNumber: WeekNumber.From(25),
            input: new UpdateWeekUserEntriesUseCaseInput(
                Monday: new UpdateWeekUserEntriesUseCaseInputDay(
                    Entries:
                    [
                        new UpdateWeekUserEntriesUseCaseInputDayEntry(
                            InvoiceItemId: InvoiceItemId.From(10),
                            Minutes: UserEntryMinutes.From(60),
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
        var context = new UpdateWeekUserEntriesUseCaseTestContext().WithResolveUserError(
            resolveUserError
        );

        var result = await context
            .BuildTarget()
            .Execute(
                year: Year.From(2025),
                weekNumber: WeekNumber.From(25),
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
        var context = new UpdateWeekUserEntriesUseCaseTestContext().WithResolveUserSuccess(
            ResolvedUserBuilder
                .AsAdministratorBob()
                .WithEntriesPermission(entriesPermission)
                .Build()
        );

        var result = await context
            .BuildTarget()
            .Execute(
                year: Year.From(2025),
                weekNumber: WeekNumber.From(25),
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
}

[GenerateTestContext(TargetType = typeof(UpdateWeekUserEntriesUseCase), GenerateWithMethods = true)]
internal partial class UpdateWeekUserEntriesUseCaseTestContext { }
