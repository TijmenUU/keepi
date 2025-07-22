using Keepi.Core.UserEntryCategories;

namespace Keepi.Core.Unit.Tests.UserEntryCategories;

public class UpdateUserEntryCategoriesUseCaseTests
{
    [Fact]
    public async Task Execute_stores_existing_user_entry_categories()
    {
        var context = new TestContext().WithSuccessfulUpdateUserentryCategoryStore();

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userId: 981,
            userEntryCategories:
            [
                new UpdateUserEntryCategoriesUseCaseInput(
                    Id: 42,
                    Name: "Development",
                    Ordinal: 1,
                    Enabled: true,
                    ActiveFrom: new DateOnly(2020, 12, 31),
                    ActiveTo: new DateOnly(2030, 12, 31)
                ),
                new UpdateUserEntryCategoriesUseCaseInput(
                    Id: 43,
                    Name: "Administratie",
                    Ordinal: 2,
                    Enabled: false,
                    ActiveFrom: null,
                    ActiveTo: null
                ),
            ],
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeTrue();

        context.UpdateUserEntryCategoryMock.Verify(x =>
            x.Execute(
                981,
                It.Is<UserEntryCategoryEntity[]>(i =>
                    i.Length == 2
                    && i[0].Id == 42
                    && i[0].Name == "Development"
                    && i[0].Ordinal == 1
                    && i[0].Enabled == true
                    && i[0].ActiveFrom == new DateOnly(2020, 12, 31)
                    && i[0].ActiveTo == new DateOnly(2030, 12, 31)
                    && i[1].Id == 43
                    && i[1].Name == "Administratie"
                    && i[1].Ordinal == 2
                    && i[1].Enabled == false
                    && i[1].ActiveFrom == null
                    && i[1].ActiveTo == null
                ),
                It.IsAny<CancellationToken>()
            )
        );
    }

    [Fact]
    public async Task Execute_stores_new_user_entry_categories()
    {
        var context = new TestContext().WithSuccessfulUpdateUserentryCategoryStore();

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userId: 981,
            userEntryCategories:
            [
                new UpdateUserEntryCategoriesUseCaseInput(
                    Id: null,
                    Name: "Development",
                    Ordinal: 1,
                    Enabled: true,
                    ActiveFrom: new DateOnly(2020, 12, 31),
                    ActiveTo: new DateOnly(2030, 12, 31)
                ),
                new UpdateUserEntryCategoriesUseCaseInput(
                    Id: null,
                    Name: "Administratie",
                    Ordinal: 2,
                    Enabled: false,
                    ActiveFrom: null,
                    ActiveTo: null
                ),
            ],
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeTrue();

        context.UpdateUserEntryCategoryMock.Verify(x =>
            x.Execute(
                981,
                It.Is<UserEntryCategoryEntity[]>(i =>
                    i.Length == 2
                    && i[0].Id == 0
                    && i[0].Name == "Development"
                    && i[0].Ordinal == 1
                    && i[0].Enabled == true
                    && i[0].ActiveFrom == new DateOnly(2020, 12, 31)
                    && i[0].ActiveTo == new DateOnly(2030, 12, 31)
                    && i[1].Id == 0
                    && i[1].Name == "Administratie"
                    && i[1].Ordinal == 2
                    && i[1].Enabled == false
                    && i[1].ActiveFrom == null
                    && i[1].ActiveTo == null
                ),
                It.IsAny<CancellationToken>()
            )
        );
    }

    [Fact]
    public async Task Execute_rejects_duplicate_ids()
    {
        var context = new TestContext().WithSuccessfulUpdateUserentryCategoryStore();

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userId: 981,
            userEntryCategories:
            [
                new UpdateUserEntryCategoriesUseCaseInput(
                    Id: 42,
                    Name: "Development",
                    Ordinal: 1,
                    Enabled: true,
                    ActiveFrom: new DateOnly(2020, 12, 31),
                    ActiveTo: new DateOnly(2030, 12, 31)
                ),
                new UpdateUserEntryCategoriesUseCaseInput(
                    Id: 42,
                    Name: "Administratie",
                    Ordinal: 2,
                    Enabled: false,
                    ActiveFrom: null,
                    ActiveTo: null
                ),
            ],
            cancellationToken: CancellationToken.None
        );

        result.ErrorOrNull.ShouldBe(UpdateUserEntryCategoriesUseCaseError.DuplicateId);
    }

    [Fact]
    public async Task Execute_rejects_duplicate_names()
    {
        var context = new TestContext().WithSuccessfulUpdateUserentryCategoryStore();

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userId: 981,
            userEntryCategories:
            [
                new UpdateUserEntryCategoriesUseCaseInput(
                    Id: 42,
                    Name: "Development",
                    Ordinal: 1,
                    Enabled: true,
                    ActiveFrom: new DateOnly(2020, 12, 31),
                    ActiveTo: new DateOnly(2030, 12, 31)
                ),
                new UpdateUserEntryCategoriesUseCaseInput(
                    Id: 43,
                    Name: "Development",
                    Ordinal: 2,
                    Enabled: false,
                    ActiveFrom: null,
                    ActiveTo: null
                ),
            ],
            cancellationToken: CancellationToken.None
        );

        result.ErrorOrNull.ShouldBe(UpdateUserEntryCategoriesUseCaseError.DuplicateName);
    }

    [Fact]
    public async Task Execute_rejects_duplicate_ordinals()
    {
        var context = new TestContext().WithSuccessfulUpdateUserentryCategoryStore();

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userId: 981,
            userEntryCategories:
            [
                new UpdateUserEntryCategoriesUseCaseInput(
                    Id: 42,
                    Name: "Development",
                    Ordinal: 1,
                    Enabled: true,
                    ActiveFrom: new DateOnly(2020, 12, 31),
                    ActiveTo: new DateOnly(2030, 12, 31)
                ),
                new UpdateUserEntryCategoriesUseCaseInput(
                    Id: 43,
                    Name: "Administratie",
                    Ordinal: 1,
                    Enabled: false,
                    ActiveFrom: null,
                    ActiveTo: null
                ),
            ],
            cancellationToken: CancellationToken.None
        );

        result.ErrorOrNull.ShouldBe(UpdateUserEntryCategoriesUseCaseError.DuplicateOrdinal);
    }

    [Theory]
    [InlineData(
        UpdateUserEntryCategoriesError.DuplicateName,
        UpdateUserEntryCategoriesUseCaseError.DuplicateName
    )]
    [InlineData(
        UpdateUserEntryCategoriesError.Unknown,
        UpdateUserEntryCategoriesUseCaseError.Unknown
    )]
    [InlineData(
        UpdateUserEntryCategoriesError.UserEntryCategoryDoesNotExist,
        UpdateUserEntryCategoriesUseCaseError.UserEntryCategoryDoesNotExist
    )]
    public async Task Execute_forwards_expected_error(
        UpdateUserEntryCategoriesError storeError,
        UpdateUserEntryCategoriesUseCaseError expectedError
    )
    {
        var context = new TestContext().WithUpdateUserentryCategoryStoreError(storeError);

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userId: 981,
            userEntryCategories:
            [
                new UpdateUserEntryCategoriesUseCaseInput(
                    Id: 42,
                    Name: "Development",
                    Ordinal: 1,
                    Enabled: true,
                    ActiveFrom: new DateOnly(2020, 12, 31),
                    ActiveTo: new DateOnly(2030, 12, 31)
                ),
            ],
            cancellationToken: CancellationToken.None
        );

        result.ErrorOrNull.ShouldBe(expectedError);
    }

    class TestContext
    {
        public Mock<IUpdateUserEntryCategories> UpdateUserEntryCategoryMock { get; } =
            new(MockBehavior.Strict);

        public TestContext WithSuccessfulUpdateUserentryCategoryStore()
        {
            UpdateUserEntryCategoryMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<int>(),
                        It.IsAny<UserEntryCategoryEntity[]>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(MaybeErrorResult<UpdateUserEntryCategoriesError>.CreateSuccess());

            return this;
        }

        public TestContext WithUpdateUserentryCategoryStoreError(
            UpdateUserEntryCategoriesError error
        )
        {
            UpdateUserEntryCategoryMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<int>(),
                        It.IsAny<UserEntryCategoryEntity[]>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    MaybeErrorResult<UpdateUserEntryCategoriesError>.CreateFailure(error)
                );

            return this;
        }

        public UpdateUserEntryCategoriesUseCase BuildUseCase() =>
            new(updateUserEntryCategories: UpdateUserEntryCategoryMock.Object);
    }
}
