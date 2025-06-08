using Keepi.Core.UserEntryCategories;

namespace Keepi.Core.Unit.Tests.UserEntryCategories;

public class CreateUserEntryCategoryUseCaseTests
{
    [Fact]
    public async Task Execute_stores_expected_entity_for_valid_input()
    {
        var context = new TestContext().WithSuccesfulUserEntryCategoryStore(
            new UserEntryCategoryEntity(
                id: 13,
                name: "Dev",
                enabled: true,
                activeFrom: new DateOnly(2024, 4, 25),
                activeTo: new DateOnly(2024, 12, 31)
            )
        );

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userId: 42,
            name: "Dev",
            enabled: true,
            activeFrom: new DateOnly(2024, 4, 25),
            activeTo: new DateOnly(2024, 12, 31),
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeTrue();
        result.SuccessOrThrow.UserEntryCategoryId.ShouldBe(13);

        context.StoreUserEntryCategoryMock.Verify(x =>
            x.Execute(
                42,
                "Dev",
                true,
                new DateOnly(2024, 4, 25),
                new DateOnly(2024, 12, 31),
                It.IsAny<CancellationToken>()
            )
        );
    }

    [Fact]
    public async Task Execute_returns_expected_error_for_duplicate_name()
    {
        var context = new TestContext().WithFailedUserEntryCategoryStore(
            StoreUserEntryCategoryError.DuplicateName
        );

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userId: 42,
            name: "Dev",
            enabled: true,
            activeFrom: new DateOnly(2024, 4, 25),
            activeTo: new DateOnly(2024, 12, 31),
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeFalse();
        result.ErrorOrNull.ShouldBe(CreateUserEntryCategoryUseCaseError.DuplicateName);

        context.StoreUserEntryCategoryMock.Verify(x =>
            x.Execute(
                42,
                "Dev",
                true,
                new DateOnly(2024, 4, 25),
                new DateOnly(2024, 12, 31),
                It.IsAny<CancellationToken>()
            )
        );
    }

    [Fact]
    public async Task Execute_returns_expected_error_for_unknown_failure()
    {
        var context = new TestContext().WithFailedUserEntryCategoryStore(
            StoreUserEntryCategoryError.Unknown
        );

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userId: 42,
            name: "Dev",
            enabled: true,
            activeFrom: new DateOnly(2024, 4, 25),
            activeTo: new DateOnly(2024, 12, 31),
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeFalse();
        result.ErrorOrNull.ShouldBe(CreateUserEntryCategoryUseCaseError.Unknown);

        context.StoreUserEntryCategoryMock.Verify(x =>
            x.Execute(
                42,
                "Dev",
                true,
                new DateOnly(2024, 4, 25),
                new DateOnly(2024, 12, 31),
                It.IsAny<CancellationToken>()
            )
        );
    }

    class TestContext
    {
        public Mock<IStoreUserEntryCategory> StoreUserEntryCategoryMock { get; } =
            new(MockBehavior.Strict);

        public TestContext WithSuccesfulUserEntryCategoryStore(
            UserEntryCategoryEntity returnedEntity
        )
        {
            StoreUserEntryCategoryMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<int>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<DateOnly?>(),
                        It.IsAny<DateOnly?>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    ValueOrErrorResult<
                        UserEntryCategoryEntity,
                        StoreUserEntryCategoryError
                    >.CreateSuccess(returnedEntity)
                );

            return this;
        }

        public TestContext WithFailedUserEntryCategoryStore(StoreUserEntryCategoryError error)
        {
            StoreUserEntryCategoryMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<int>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<DateOnly?>(),
                        It.IsAny<DateOnly?>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    ValueOrErrorResult<
                        UserEntryCategoryEntity,
                        StoreUserEntryCategoryError
                    >.CreateFailure(error)
                );

            return this;
        }

        public CreateUserEntryCategoryUseCase BuildUseCase() =>
            new(storeUserEntryCategory: StoreUserEntryCategoryMock.Object);
    }
}
