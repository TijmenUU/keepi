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
    int UserId,
    OverwriteUserInvoiceItemCustomizationsInputInvoiceItem[] InvoiceItems
);

public record OverwriteUserInvoiceItemCustomizationsInputInvoiceItem(
    int InvoiceItemId,
    int Ordinal,
    Color? Color
);
