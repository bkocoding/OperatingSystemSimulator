using Microsoft.UI.Xaml.Controls.Primitives;
using OperatingSystemSimulator.Apps.Enums;
using OperatingSystemSimulator.Apps.Shell.Enums;
using OperatingSystemSimulator.ToolTipHelper.ToolTipTools;
using Windows.ApplicationModel.Resources;

namespace OperatingSystemSimulator.ToolTipHelper;
public class TooltipManager
{
    readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

    public void ApplyTooltip(UIElement target, ToolTipParameters parameters)
    {
        if (target == null)
            return;

        string message = GetTooltipMessage(parameters);
        double maxWidth = GetTooltipWidth(parameters);
        double maxHeight = GetTooltipHeight(parameters);


        var tooltip = new ToolTip
        {
            Content = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Padding = new Thickness(8),
                MaxWidth = maxWidth,
                MaxHeight = maxHeight,
                FontSize = 14,
                TextAlignment = TextAlignment.Justify
            },


        };

        ToolTipService.SetToolTip(target, tooltip);
    }

    private string GetTooltipMessage(ToolTipParameters parameters)
    {

        if (parameters.SType == ShellType.App)
            return parameters.AType switch
            {
                AppType.Notepad => GetFormattedString("NotepadAppAccesibility", parameters.ExtraParams),
                AppType.FileManager => GetFormattedString("FileManagerAppAccesibility", parameters.ExtraParams),
                AppType.TestApp => GetFormattedString("TestAppAccesibility", parameters.ExtraParams),
                AppType.WebBrowser => GetFormattedString("WebBrowserAppAccesibility", parameters.ExtraParams),
                AppType.TaskManager => GetFormattedString("TaskManagerAppAccesibility", parameters.ExtraParams),
                _ => GetFormattedString("UnknownAccesibility", parameters.ExtraParams)
            };

        else if (parameters.SType == ShellType.FileDialog)
        {
            return resourceLoader.GetString("FileDialogAccesibility")!;
        }

        else if (parameters.SType == ShellType.Message)
        {
            return resourceLoader.GetString("MessageBoxAccesibility")!;

        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private static double GetTooltipWidth(ToolTipParameters parameters)
    {
        if (parameters.SType == ShellType.App)
            return parameters.AType switch
            {
                AppType.Notepad => 400,
                AppType.FileManager => 400,
                AppType.WebBrowser => 500,
                AppType.TestApp => 180,
                AppType.TaskManager => 400,
                _ => 300
            };

        else if (parameters.SType == ShellType.FileDialog)
        {
            return 400;
        }

        else if (parameters.SType == ShellType.Message)
        {
            throw new NotImplementedException();

        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private static double GetTooltipHeight(ToolTipParameters parameters)
    {
        if (parameters.SType == ShellType.App)
            return parameters.AType switch
            {
                AppType.Notepad => 130,
                AppType.FileManager => 150,
                AppType.WebBrowser => 170,
                AppType.TaskManager => 150,
                _ => 120
            };

        else if (parameters.SType == ShellType.FileDialog)
        {
            return 100;
        }

        else if (parameters.SType == ShellType.Message)
        {
            throw new NotImplementedException();

        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private string GetFormattedString(string resourceKey, Dictionary<string, string>? parameters = null)
    {
        string? template = resourceLoader.GetString(resourceKey);

        if (string.IsNullOrWhiteSpace(template))
        {
            template = $"Couldn't load resource [{resourceKey}]";
        }

        if (parameters != null)
        {
            foreach (var pair in parameters)
            {
                template = template.Replace($"{{{pair.Key}}}", pair.Value);
            }
        }
        return template;
    }

}
