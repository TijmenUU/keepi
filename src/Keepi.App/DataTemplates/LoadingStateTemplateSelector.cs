using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace Keepi.App.DataTemplates;

public enum LoadingState
{
    Loading,
    Loaded,
    Crashed,
}

public class LoadingStateTemplateSelector : IDataTemplate
{
    [Content]
    public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = [];

    public Control? Build(object? param)
    {
        var key = (param?.ToString()) ?? throw new ArgumentNullException(paramName: nameof(param));
        var value =
            AvailableTemplates[key]
            ?? throw new ArgumentException(
                message: $"No template is registered for the key {key}",
                paramName: nameof(param)
            );
        return value.Build(param);
    }

    public bool Match(object? data)
    {
        var key = data?.ToString();
        return data is LoadingState
            && !string.IsNullOrEmpty(key)
            && AvailableTemplates.ContainsKey(key);
    }
}
