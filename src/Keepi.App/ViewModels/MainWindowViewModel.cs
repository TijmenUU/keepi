using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using AvaloniaDialogs.Views;
using Keepi.App.Cancellation;
using Keepi.App.DataTemplates;
using Keepi.App.Models;
using Keepi.App.Services;
using Keepi.Core;

namespace Keepi.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly IUserEntryWeekService userEntryWeekService;
    private readonly ICancellationTokenFactory cancellationTokenFactory;

    public MainWindowViewModel(
        IUserEntryWeekService userEntryWeekService,
        ICancellationTokenFactory cancellationTokenFactory
    )
    {
        this.userEntryWeekService = userEntryWeekService;
        this.cancellationTokenFactory = cancellationTokenFactory;

        weekInputStrings.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(WeekInputMinutes));
            OnPropertyChanged(nameof(WeekInputInvoiceItemTotals));
            OnPropertyChanged(nameof(WeekInputDayTotals));
            OnPropertyChanged(nameof(WeekInputGrandTotal));
        };
    }

    public async Task InitializeAsync()
    {
        var currentWeek = WeekNumberHelper.GetCurrentWeek();
        var result = await userEntryWeekService.GetWeekInputs(
            year: currentWeek.Year,
            weekNumber: currentWeek.Number,
            cancellationToken: cancellationTokenFactory.GetShutdownCancellationToken()
        );

        if (result.TrySuccess(out var successResult, out var errorResult))
        {
            Dispatcher.UIThread.Post(() =>
            {
                // The order of operations is important here. Probably should be refactored such that
                // a single collection is initialized here instead of the inputs and grid separately.
                // See the view cs file for the event listener on the UserEntryWeekInputs property
                WeekInputStrings.Clear();
                foreach (
                    var input in successResult.SelectMany(s =>
                        new[]
                        {
                            s.Monday,
                            s.Tuesday,
                            s.Wednesday,
                            s.Thursday,
                            s.Friday,
                            s.Saturday,
                            s.Sunday,
                        }
                    )
                )
                {
                    weekInputStrings.Add(input);
                }
                UserEntryWeekInputs = successResult;
                LoadingState = LoadingState.Loaded;
            });
        }
        else
        {
            // TODO if the error is no projects configured then switch tabs automatically

            Dispatcher.UIThread.Post(() =>
            {
                LoadingState = LoadingState.Crashed;
            });

            // TODO Decide if this dialog is still necessary since the crashed datatemplate also notifies the user of the error
            SingleActionDialog dialog = new()
            {
                Message =
                    $"Er is een onverwachtte fout opgetreden waardoor de applicatie niet verder kan. Foutcode {errorResult}.",
                ButtonText = "Afsluiten",
            };
            await dialog.ShowAsync();

            if (
                Avalonia.Application.Current?.ApplicationLifetime
                is IControlledApplicationLifetime lifetime
            )
            {
                lifetime.Shutdown();
            }
        }
    }

    public void Dispose()
    {
        // TODO ?
    }

    private LoadingState loadingState = LoadingState.Loading;
    public LoadingState LoadingState
    {
        get => loadingState;
        set
        {
            loadingState = value;
            OnPropertyChanged();
        }
    }

    private UserEntryWeekInput[] userEntryWeekInputs = [];
    public UserEntryWeekInput[] UserEntryWeekInputs
    {
        get => userEntryWeekInputs;
        set
        {
            userEntryWeekInputs = value;
            OnPropertyChanged();
        }
    }

    private Control? weekInputGrid;
    public Control? WeekInputGrid
    {
        get => weekInputGrid;
        set
        {
            weekInputGrid = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<string> weekInputStrings = [];
    public ObservableCollection<string> WeekInputStrings
    {
        get => weekInputStrings;
    }

    // TODO cache this? Caching could perhaps be done by making the collection changed handler smarter
    public int[] WeekInputMinutes =>
        WeekInputStrings
            .Select(s => HoursMinuteNotation.ParseOrFallback(input: s, fallback: 0))
            .ToArray();

    public string[] WeekInputDayTotals
    {
        get
        {
            var dayTotals = new int[7];
            foreach (var week in WeekInputMinutes.Chunk(size: 7).Select(c => c.ToArray()))
            {
                if (week.Length != 7)
                {
                    break;
                }

                dayTotals[0] += week[0];
                dayTotals[1] += week[1];
                dayTotals[2] += week[2];
                dayTotals[3] += week[3];
                dayTotals[4] += week[4];
                dayTotals[5] += week[5];
                dayTotals[6] += week[6];
            }

            return dayTotals.Select(HoursMinuteNotation.Format).ToArray();
        }
    }

    public string[] WeekInputInvoiceItemTotals =>
        WeekInputMinutes.Chunk(size: 7).Select(c => HoursMinuteNotation.Format(c.Sum())).ToArray();

    public string WeekInputGrandTotal => HoursMinuteNotation.Format(WeekInputMinutes.Sum());
}
