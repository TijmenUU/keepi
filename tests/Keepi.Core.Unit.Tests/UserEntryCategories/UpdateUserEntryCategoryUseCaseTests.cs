using System.Text;
using Keepi.Core.UserEntryCategories;

namespace Keepi.Core.Unit.Tests.UserEntryCategories;

public class UpdateUserEntryCategoryUseCaseTests
{
    [Fact]
    public async Task Execute_stores_updated_entry_category_without_existing_entity_with_ordinal()
    {
        var context = new TestContext()
            .WithoutExistingOrdinal()
            .WithSuccesfulUserEntryCategoryUpdate();

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userEntryCategoryId: 42,
            userId: 981,
            name: "Development",
            ordinal: 1,
            enabled: true,
            activeFrom: new DateOnly(2020, 12, 31),
            activeTo: new DateOnly(2030, 12, 31),
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeTrue();

        context.GetUserEntryCategoryIdByOrdinalMock.Verify(x =>
            x.Execute(981, 1, It.IsAny<CancellationToken>())
        );
        context.UpdateUserEntryCategoryMock.Verify(x =>
            x.Execute(
                42,
                981,
                "Development",
                1,
                true,
                new DateOnly(2020, 12, 31),
                new DateOnly(2030, 12, 31),
                It.IsAny<CancellationToken>()
            )
        );
    }

    [Fact]
    public async Task Execute_stores_updated_entry_category_with_ordinal_already_used_by_itself()
    {
        var context = new TestContext()
            .WithExistingOrdinal(userEntryCategoryId: 42)
            .WithSuccesfulUserEntryCategoryUpdate();

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userEntryCategoryId: 42,
            userId: 981,
            name: "Development",
            ordinal: 1,
            enabled: true,
            activeFrom: new DateOnly(2020, 12, 31),
            activeTo: new DateOnly(2030, 12, 31),
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeTrue();

        context.GetUserEntryCategoryIdByOrdinalMock.Verify(x =>
            x.Execute(981, 1, It.IsAny<CancellationToken>())
        );
        context.UpdateUserEntryCategoryMock.Verify(x =>
            x.Execute(
                42,
                981,
                "Development",
                1,
                true,
                new DateOnly(2020, 12, 31),
                new DateOnly(2030, 12, 31),
                It.IsAny<CancellationToken>()
            )
        );
    }

    [Fact]
    public async Task Execute_returns_expected_error_for_non_existing_entity()
    {
        var context = new TestContext()
            .WithExistingOrdinal(userEntryCategoryId: 42)
            .WithFailedUserEntryCategoryUpdate(
                UpdateUserEntryCategoryError.UserEntryCategoryDoesNotExist
            );

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userEntryCategoryId: 42,
            userId: 981,
            name: "Development",
            ordinal: 1,
            enabled: true,
            activeFrom: new DateOnly(2020, 12, 31),
            activeTo: new DateOnly(2030, 12, 31),
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeFalse();
        result.ErrorOrNull.ShouldBe(UpdateUserEntryCategoryUseCaseError.UnknownUserEntryCategory);

        context.UpdateUserEntryCategoryMock.Verify(x =>
            x.Execute(
                42,
                981,
                "Development",
                1,
                true,
                new DateOnly(2020, 12, 31),
                new DateOnly(2030, 12, 31),
                It.IsAny<CancellationToken>()
            )
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Execute_returns_expected_error_for_empty_or_null_name(string? name)
    {
        var context = new TestContext().WithExistingOrdinal(userEntryCategoryId: 42);

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userEntryCategoryId: 42,
            userId: 981,
            name: name,
            ordinal: 1,
            enabled: true,
            activeFrom: new DateOnly(2020, 12, 31),
            activeTo: new DateOnly(2030, 12, 31),
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeFalse();
        result.ErrorOrNull.ShouldBe(UpdateUserEntryCategoryUseCaseError.MalformedName);

        context.UpdateUserEntryCategoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_expected_error_for_too_long_name()
    {
        var context = new TestContext().WithExistingOrdinal(userEntryCategoryId: 42);

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userEntryCategoryId: 42,
            userId: 981,
            name: new StringBuilder().Append('a', repeatCount: 65).ToString(),
            ordinal: 1,
            enabled: true,
            activeFrom: new DateOnly(2020, 12, 31),
            activeTo: new DateOnly(2030, 12, 31),
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeFalse();
        result.ErrorOrNull.ShouldBe(UpdateUserEntryCategoryUseCaseError.MalformedName);

        context.UpdateUserEntryCategoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_expected_error_for_duplicate_ordinal()
    {
        var context = new TestContext().WithExistingOrdinal(userEntryCategoryId: 43); // When the ID differs it means that the ordinal is already used by an entity with another ID

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userEntryCategoryId: 42,
            userId: 981,
            name: "Development",
            ordinal: 1,
            enabled: true,
            activeFrom: new DateOnly(2020, 12, 31),
            activeTo: new DateOnly(2030, 12, 31),
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeFalse();
        result.ErrorOrNull.ShouldBe(UpdateUserEntryCategoryUseCaseError.DuplicateOrdinal);

        context.UpdateUserEntryCategoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_expected_error_for_invalid_range()
    {
        var context = new TestContext().WithExistingOrdinal(userEntryCategoryId: 42);

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userEntryCategoryId: 42,
            userId: 981,
            name: "Development",
            ordinal: 1,
            enabled: true,
            activeFrom: new DateOnly(2030, 12, 31),
            activeTo: new DateOnly(2020, 12, 31),
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeFalse();
        result.ErrorOrNull.ShouldBe(UpdateUserEntryCategoryUseCaseError.InvalidActiveDateRange);

        context.UpdateUserEntryCategoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_expected_error_for_entity_that_belongs_to_other_user()
    {
        var context = new TestContext()
            .WithExistingOrdinal(userEntryCategoryId: 42)
            .WithFailedUserEntryCategoryUpdate(
                UpdateUserEntryCategoryError.UserEntryCategoryBelongsToOtherUser
            );

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userEntryCategoryId: 42,
            userId: 981,
            name: "Development",
            ordinal: 1,
            enabled: true,
            activeFrom: new DateOnly(2020, 12, 31),
            activeTo: new DateOnly(2030, 12, 31),
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeFalse();
        result.ErrorOrNull.ShouldBe(UpdateUserEntryCategoryUseCaseError.UnknownUserEntryCategory);

        context.UpdateUserEntryCategoryMock.Verify(x =>
            x.Execute(
                42,
                981,
                "Development",
                1,
                true,
                new DateOnly(2020, 12, 31),
                new DateOnly(2030, 12, 31),
                It.IsAny<CancellationToken>()
            )
        );
    }

    [Fact]
    public async Task Execute_returns_expected_error_for_duplicate_name()
    {
        var context = new TestContext()
            .WithExistingOrdinal(userEntryCategoryId: 42)
            .WithFailedUserEntryCategoryUpdate(UpdateUserEntryCategoryError.DuplicateName);

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userEntryCategoryId: 42,
            userId: 981,
            name: "Development",
            ordinal: 1,
            enabled: true,
            activeFrom: new DateOnly(2020, 12, 31),
            activeTo: new DateOnly(2030, 12, 31),
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeFalse();
        result.ErrorOrNull.ShouldBe(UpdateUserEntryCategoryUseCaseError.DuplicateName);

        context.UpdateUserEntryCategoryMock.Verify(x =>
            x.Execute(
                42,
                981,
                "Development",
                1,
                true,
                new DateOnly(2020, 12, 31),
                new DateOnly(2030, 12, 31),
                It.IsAny<CancellationToken>()
            )
        );
    }

    class TestContext
    {
        public Mock<IUpdateUserEntryCategory> UpdateUserEntryCategoryMock { get; } =
            new(MockBehavior.Strict);

        public Mock<IGetUserEntryCategoryIdByOrdinal> GetUserEntryCategoryIdByOrdinalMock { get; } =
            new(MockBehavior.Strict);

        public TestContext WithExistingOrdinal(int userEntryCategoryId)
        {
            GetUserEntryCategoryIdByOrdinalMock
                .Setup(x =>
                    x.Execute(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(userEntryCategoryId);

            return this;
        }

        public TestContext WithoutExistingOrdinal()
        {
            GetUserEntryCategoryIdByOrdinalMock
                .Setup(x =>
                    x.Execute(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync((int?)null);

            return this;
        }

        public TestContext WithSuccesfulUserEntryCategoryUpdate()
        {
            UpdateUserEntryCategoryMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<string>(),
                        It.IsAny<int>(),
                        It.IsAny<bool>(),
                        It.IsAny<DateOnly?>(),
                        It.IsAny<DateOnly?>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(MaybeErrorResult<UpdateUserEntryCategoryError>.CreateSuccess());

            return this;
        }

        public TestContext WithFailedUserEntryCategoryUpdate(UpdateUserEntryCategoryError error)
        {
            UpdateUserEntryCategoryMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<string>(),
                        It.IsAny<int>(),
                        It.IsAny<bool>(),
                        It.IsAny<DateOnly?>(),
                        It.IsAny<DateOnly?>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(MaybeErrorResult<UpdateUserEntryCategoryError>.CreateFailure(error));

            return this;
        }

        public UpdateUserEntryCategoryUseCase BuildUseCase() =>
            new(
                updateUserEntryCategory: UpdateUserEntryCategoryMock.Object,
                getUserEntryCategoryIdByOrdinal: GetUserEntryCategoryIdByOrdinalMock.Object
            );
    }
}
