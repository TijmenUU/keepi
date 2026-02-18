namespace Keepi.App.Models;

public sealed class UserEntryWeekInput
{
    public UserEntryWeekInput(
        bool enabled,
        int invoiceItemId,
        string invoiceItemName,
        int minutesMonday,
        int minutesTuesday,
        int minutesWednesday,
        int minutesThursday,
        int minutesFriday,
        int minutesSaturday,
        int minutesSunday
    )
    {
        Enabled = enabled;

        InvoiceItemId = invoiceItemId;
        InvoiceItemName = invoiceItemName;

        Monday = HoursMinuteNotation.Format(minutes: minutesMonday);
        Tuesday = HoursMinuteNotation.Format(minutes: minutesTuesday);
        Wednesday = HoursMinuteNotation.Format(minutes: minutesWednesday);
        Thursday = HoursMinuteNotation.Format(minutes: minutesThursday);
        Friday = HoursMinuteNotation.Format(minutes: minutesFriday);
        Saturday = HoursMinuteNotation.Format(minutes: minutesSaturday);
        Sunday = HoursMinuteNotation.Format(minutes: minutesSunday);
    }

    public bool Enabled { get; }

    public int InvoiceItemId { get; }
    public string InvoiceItemName { get; }

    public string Monday { get; set; }
    public string Tuesday { get; set; }
    public string Wednesday { get; set; }
    public string Thursday { get; set; }
    public string Friday { get; set; }
    public string Saturday { get; set; }
    public string Sunday { get; set; }
}
