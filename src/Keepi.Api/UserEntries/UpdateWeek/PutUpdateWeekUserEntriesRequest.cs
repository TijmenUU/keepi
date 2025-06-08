namespace Keepi.Api.UserEntries.UpdateWeek;

public class PutUpdateWeekUserEntriesRequest
{
    public PutUpdateWeekUserEntriesRequestDay? Monday { get; set; }
    public PutUpdateWeekUserEntriesRequestDay? Tuesday { get; set; }
    public PutUpdateWeekUserEntriesRequestDay? Wednesday { get; set; }
    public PutUpdateWeekUserEntriesRequestDay? Thursday { get; set; }
    public PutUpdateWeekUserEntriesRequestDay? Friday { get; set; }
    public PutUpdateWeekUserEntriesRequestDay? Saturday { get; set; }
    public PutUpdateWeekUserEntriesRequestDay? Sunday { get; set; }
}

public class PutUpdateWeekUserEntriesRequestDay
{
    public PutUpdateWeekUserEntriesRequestDayEntry?[]? Entries { get; set; }
}

public class PutUpdateWeekUserEntriesRequestDayEntry
{
    public int? EntryCategoryId { get; set; }
    public int? Minutes { get; set; }
    public string? Remark { get; set; }
}
