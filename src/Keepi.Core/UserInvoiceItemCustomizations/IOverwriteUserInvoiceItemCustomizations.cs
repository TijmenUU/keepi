using Keepi.Core.InvoiceItems;
using Keepi.Core.Users;

namespace Keepi.Core.UserInvoiceItemCustomizations;

public interface IOverwriteUserInvoiceItemCustomizations
{
    Task<IMaybeErrorResult<OverwriteUserInvoiceItemCustomizationsError>> Execute(
        OverwriteUserInvoiceItemCustomizationsInput input,
        CancellationToken cancellationToken
    );
}

public enum OverwriteUserInvoiceItemCustomizationsError
{
    Unknown = 0,
    UnknownInvoiceItemId,
}

public record OverwriteUserInvoiceItemCustomizationsInput(
    UserId UserId,
    OverwriteUserInvoiceItemCustomizationsInputInvoiceItem[] InvoiceItems
);

public record OverwriteUserInvoiceItemCustomizationsInputInvoiceItem(
    InvoiceItemId InvoiceItemId,
    UserInvoiceITemCustomizationOrdinal Ordinal,
    Color? Color
);
