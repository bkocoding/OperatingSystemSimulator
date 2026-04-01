using OperatingSystemSimulator.Apps.Shell.Enums;
using OperatingSystemSimulator.ToolTipHelper;
using OperatingSystemSimulator.ToolTipHelper.ToolTipTools;

namespace OperatingSystemSimulator.Hardware;
public sealed partial class PageListPage : Page
{
    public PageListPage()
    {
        InitializeComponent();
        DataContext = MemoryManager.Instance;
        Loaded += PageListPage_Loaded;
    }


    private readonly TooltipManager _tooltipManager = new();

    private void Border_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        foreach (var page in MemoryManager.Instance.Pages)
        {
            page.IsSelected = false;
        }

        var border = sender as Border;
        var pageBlock = border?.DataContext as PageBlock;

        if (pageBlock!.PageNumber <= 14)
        {
            for (var i = 0; i <= 14; i++)
            {
                MemoryManager.Instance.Pages[i].IsSelected = true;
            }
            HardwarePageViewModel.Instance.ShowBiosInfo();
            return;
        }

        if (pageBlock?.ProcessBlock != null)
        {
            foreach (var block in pageBlock.ProcessBlock.PageBlocks)
            {
                block.IsSelected = true;
            }
            HardwarePageViewModel.Instance.ShowInfo(pageBlock.ProcessBlock);
        }
        else
        {
            HardwarePageViewModel.Instance.DismissInfo();
        }

    }

    private static IEnumerable<T> GetAllChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t)
                yield return t;

            foreach (var subchild in GetAllChildren<T>(child))
                yield return subchild;
        }
    }

    private void PageListPage_Loaded(object sender, RoutedEventArgs e)
    {
        foreach (var element in GetAllChildren<FrameworkElement>(this))
        {
            if (element is TextBlock or Button or Border)
            {
                var parameters = new ToolTipParameters
                {
                    SType = ShellType.None,
                    ExtraParams = new Dictionary<string, string>
                    {
                        { "Sender", "PageListWindow" }
                    }
                };

                _tooltipManager.ApplyTooltip(element, parameters);
            }
        }
    }
}
