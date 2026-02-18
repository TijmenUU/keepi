using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Keepi.App.ViewModels;
using Microsoft.Extensions.Logging;

namespace Keepi.App.Views;

public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow> logger;

    public MainWindow(ILogger<MainWindow> logger)
    {
        this.logger = logger;

        InitializeComponent();

        Opened += async (_, _) =>
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.PropertyChanged += OnViewModelPropertyChanged;

                await viewModel.InitializeAsync();
            }
        };
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            logger.LogDebug(
                "Viewmodel was not the expected type in {MethodName}",
                nameof(OnViewModelPropertyChanged)
            );
            return;
        }

        if (e.PropertyName == nameof(MainWindowViewModel.UserEntryWeekInputs))
        {
            // TODO Should the "old" grid be cleaned up? Bindings unbound, callbacks decoupled, ...?
            viewModel.WeekInputGrid = CreateWeekInputGrid(viewModel: viewModel);
        }
    }

    private Grid CreateWeekInputGrid(MainWindowViewModel viewModel)
    {
        var inputGrid = new Grid { RowSpacing = 4, ColumnSpacing = 2 };
        foreach (var _ in Enumerable.Range(start: 0, count: 9)) // Invoice item label, 7 weekdays, day total
        {
            inputGrid.ColumnDefinitions.Add(item: new ColumnDefinition(width: GridLength.Auto));
        }
        foreach (
            var _ in Enumerable.Range(start: 0, count: viewModel.UserEntryWeekInputs.Length + 3)
        ) // Add 1 for the header, 1 for the footer, 1 for button row
        {
            inputGrid.RowDefinitions.Add(item: new RowDefinition(height: GridLength.Auto));
        }

        inputGrid.Children.AddRange([
            CreateGridTextBlock(text: "Facturatie post", row: 0, column: 0),
            CreateGridTextBlock(text: "Ma", row: 0, column: 1).CenterHorizontally(),
            CreateGridTextBlock(text: "Di", row: 0, column: 2).CenterHorizontally(),
            CreateGridTextBlock(text: "Wo", row: 0, column: 3).CenterHorizontally(),
            CreateGridTextBlock(text: "Do", row: 0, column: 4).CenterHorizontally(),
            CreateGridTextBlock(text: "Vr", row: 0, column: 5).CenterHorizontally(),
            CreateGridTextBlock(text: "Za", row: 0, column: 6).CenterHorizontally(),
            CreateGridTextBlock(text: "Zo", row: 0, column: 7).CenterHorizontally(),
        ]);

        const int inputWidth = 74;
        for (var index = 0; index < viewModel.UserEntryWeekInputs.Length; ++index)
        {
            var rowIndex = index + 1;
            var inputIndex = index * 7;
            var isEnabled = viewModel.UserEntryWeekInputs[index].Enabled;

            inputGrid.Children.AddRange([
                CreateGridTextBlock(
                        text: viewModel.UserEntryWeekInputs[index].InvoiceItemName,
                        row: rowIndex,
                        column: 0
                    )
                    .CenterVertically(),
                CreateWeekInpuTextBox(index: inputIndex + 0, row: rowIndex, column: 1)
                    .FixWidth(width: inputWidth)
                    .SetEnabled(value: isEnabled),
                CreateWeekInpuTextBox(index: inputIndex + 1, row: rowIndex, column: 2)
                    .FixWidth(width: inputWidth)
                    .SetEnabled(value: isEnabled),
                CreateWeekInpuTextBox(index: inputIndex + 2, row: rowIndex, column: 3)
                    .FixWidth(width: inputWidth)
                    .SetEnabled(value: isEnabled),
                CreateWeekInpuTextBox(index: inputIndex + 3, row: rowIndex, column: 4)
                    .FixWidth(width: inputWidth)
                    .SetEnabled(value: isEnabled),
                CreateWeekInpuTextBox(index: inputIndex + 4, row: rowIndex, column: 5)
                    .FixWidth(width: inputWidth)
                    .SetEnabled(value: isEnabled),
                CreateWeekInpuTextBox(index: inputIndex + 5, row: rowIndex, column: 6)
                    .FixWidth(width: inputWidth)
                    .SetEnabled(value: isEnabled),
                CreateWeekInpuTextBox(index: inputIndex + 6, row: rowIndex, column: 7)
                    .FixWidth(width: inputWidth)
                    .SetEnabled(value: isEnabled),
                CreateArrayBoundGridTextBlock(
                        path: nameof(viewModel.WeekInputInvoiceItemTotals),
                        index: index,
                        row: rowIndex,
                        column: 8
                    )
                    .CenterVertically()
                    .CenterHorizontally()
                    .MinimumWidth(width: inputWidth),
            ]);
        }

        var footerRowIndex = viewModel.UserEntryWeekInputs.Length + 1;
        inputGrid.Children.AddRange([
            CreateArrayBoundGridTextBlock(
                    path: nameof(viewModel.WeekInputDayTotals),
                    index: 0,
                    row: footerRowIndex,
                    column: 1
                )
                .CenterHorizontally(),
            CreateArrayBoundGridTextBlock(
                    path: nameof(viewModel.WeekInputDayTotals),
                    index: 1,
                    row: footerRowIndex,
                    column: 2
                )
                .CenterHorizontally(),
            CreateArrayBoundGridTextBlock(
                    path: nameof(viewModel.WeekInputDayTotals),
                    index: 2,
                    row: footerRowIndex,
                    column: 3
                )
                .CenterHorizontally(),
            CreateArrayBoundGridTextBlock(
                    path: nameof(viewModel.WeekInputDayTotals),
                    index: 3,
                    row: footerRowIndex,
                    column: 4
                )
                .CenterHorizontally(),
            CreateArrayBoundGridTextBlock(
                    path: nameof(viewModel.WeekInputDayTotals),
                    index: 4,
                    row: footerRowIndex,
                    column: 5
                )
                .CenterHorizontally(),
            CreateArrayBoundGridTextBlock(
                    path: nameof(viewModel.WeekInputDayTotals),
                    index: 5,
                    row: footerRowIndex,
                    column: 6
                )
                .CenterHorizontally(),
            CreateArrayBoundGridTextBlock(
                    path: nameof(viewModel.WeekInputDayTotals),
                    index: 6,
                    row: footerRowIndex,
                    column: 7
                )
                .CenterHorizontally(),
            CreateBoundGridTextBlock(
                    path: nameof(viewModel.WeekInputGrandTotal),
                    row: footerRowIndex,
                    column: 8
                )
                .CenterHorizontally(),
        ]);

        var saveButton = new Button
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Content = "Opslaan",
        };
        saveButton.Click += SaveButton_OnClick;
        Grid.SetRow(element: saveButton, value: footerRowIndex + 1);
        Grid.SetColumn(element: saveButton, value: 7);
        Grid.SetColumnSpan(element: saveButton, value: 2);
        inputGrid.Children.Add(saveButton);

        return inputGrid;
    }

    private static TextBlock CreateGridTextBlock(string text, int row, int column) =>
        new TextBlock { Text = text }.InGrid(row: row, column: column);

    private TextBlock CreateBoundGridTextBlock(string path, int row, int column)
    {
        var block = new TextBlock() { DataContext = DataContext }.InGrid(row: row, column: column);

        block.Bind(
            property: TextBlock.TextProperty,
            binding: new Binding { Source = DataContext, Path = path }
        );

        return block;
    }

    private TextBlock CreateArrayBoundGridTextBlock(string path, int index, int row, int column)
    {
        var block = new TextBlock() { DataContext = DataContext }.InGrid(row: row, column: column);

        block.Bind(
            property: TextBlock.TextProperty,
            binding: new Binding
            {
                Source = DataContext,
                Path = path,
                Converter = new StringArrayToElementConverter(index: index),
            }
        );

        return block;
    }

    private TextBox CreateWeekInpuTextBox(int index, int row, int column)
    {
        var box = new TextBox
        {
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        }.InGrid(row: row, column: column);

        box.Bind(
            property: TextBox.TextProperty,
            binding: new Binding
            {
                Source = DataContext,
                Path = $"{nameof(MainWindowViewModel.WeekInputStrings)}[{index}]",
            }
        );

        // box.TextChanged += (sender, _) => OnWeekGridInput(index: index, sender: sender);

        return box;
    }

    private void OnWeekGridInput(int index, object? sender)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            logger.LogDebug(
                "Viewmodel was not the expected type in {MethodName}",
                nameof(OnWeekGridInput)
            );
            return;
        }
        if (index < 0 || index >= viewModel.WeekInputStrings.Count)
        {
            logger.LogError("Received input event for index {Index} that is out of bounds", index);
            return;
        }
        if (sender is not TextBox textBox)
        {
            logger.LogError("Received non textbox input event for index {Index}", index);
            return;
        }

        var newValue = textBox.Text ?? string.Empty;
        if (viewModel.WeekInputStrings[index] != newValue)
        {
            viewModel.WeekInputStrings[index] = newValue;
        }
    }

    private void SaveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Saved!"); // TODO
    }

    private class StringArrayToElementConverter : IValueConverter
    {
        private readonly int index;

        public StringArrayToElementConverter(int index)
        {
            this.index = index;
        }

        public object? Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture
        )
        {
            if (value is not string[] stringArray)
            {
                return new BindingNotification(
                    error: new InvalidOperationException(
                        message: "Only string arrays are supported"
                    ),
                    errorType: BindingErrorType.Error
                );
            }
            if (targetType != typeof(string))
            {
                return new BindingNotification(
                    error: new InvalidOperationException(
                        message: "Only conversion to string is supported"
                    ),
                    errorType: BindingErrorType.Error
                );
            }
            if (index < 0 || index >= stringArray.Length)
            {
                return new BindingNotification(
                    error: new InvalidOperationException(
                        message: $"The index {index} is out of range (length {stringArray.Length})"
                    ),
                    errorType: BindingErrorType.Error
                );
            }

            return stringArray[index];
        }

        public object? ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture
        )
        {
            return BindingOperations.DoNothing;
        }
    }
}
