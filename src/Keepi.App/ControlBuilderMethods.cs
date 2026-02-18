using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;

internal static class ControlBuilderMethods
{
    internal static TLayoutable CenterHorizontally<TLayoutable>(this TLayoutable item)
        where TLayoutable : Layoutable
    {
        item.HorizontalAlignment = HorizontalAlignment.Center;
        return item;
    }

    internal static TLayoutable CenterVertically<TLayoutable>(this TLayoutable item)
        where TLayoutable : Layoutable
    {
        item.VerticalAlignment = VerticalAlignment.Center;
        return item;
    }

    internal static TLayoutable FixWidth<TLayoutable>(this TLayoutable item, double width)
        where TLayoutable : Layoutable
    {
        item.MinWidth = width;
        item.Width = width;
        item.MaxWidth = width;
        return item;
    }

    internal static TLayoutable MinimumWidth<TLayoutable>(this TLayoutable item, double width)
        where TLayoutable : Layoutable
    {
        item.MinWidth = width;
        return item;
    }

    internal static TInput SetEnabled<TInput>(this TInput item, bool value)
        where TInput : InputElement
    {
        item.IsEnabled = value;
        return item;
    }

    internal static TControl InGrid<TControl>(this TControl control, int row, int column)
        where TControl : Control
    {
        Grid.SetRow(element: control, value: row);
        Grid.SetColumn(element: control, value: column);

        return control;
    }
}
