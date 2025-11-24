namespace Keepi.Api.UserEntries.GetWeek;

public record GetWeekUserEntriesResponse(
    GetWeekUserEntriesResponseDay Monday,
    GetWeekUserEntriesResponseDay Tuesday,
    GetWeekUserEntriesResponseDay Wednesday,
    GetWeekUserEntriesResponseDay Thursday,
    GetWeekUserEntriesResponseDay Friday,
    GetWeekUserEntriesResponseDay Saturday,
    GetWeekUserEntriesResponseDay Sunday
);

public record GetWeekUserEntriesResponseDay(GetWeekUserEntriesResponseDayEntry[] Entries);

public record GetWeekUserEntriesResponseDayEntry(int InvoiceItemId, int Minutes, string? Remark);
