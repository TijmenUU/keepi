using Keepi.Core.Aggregates;
using Keepi.Core.UserEntryCategories;

namespace Keepi.Core.Users;

public sealed class UserAggregate : IAggregateRoot
{
  public UserAggregate(
    UserEntity user,
    IReadOnlyList<UserEntryCategoryEntity> entryCategories)
  {
    EmailAddress = user.EmailAddress;
    Name = user.Name;
    IdentityOrigin = user.IdentityOrigin;

    EntryCategories = entryCategories;
  }

  public string EmailAddress { get; }
  public string Name { get; }
  public UserIdentityOrigin IdentityOrigin { get; }

  public IReadOnlyList<UserEntryCategoryEntity> EntryCategories { get; }
}