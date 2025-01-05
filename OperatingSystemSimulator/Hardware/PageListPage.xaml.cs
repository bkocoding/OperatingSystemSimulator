namespace OperatingSystemSimulator.Hardware;
public sealed partial class PageListPage : Page
{
    public PageListPage()
    {
        InitializeComponent();
        DataContext = MemoryManager.Instance;
    }


    private void Border_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        foreach(var page in MemoryManager.Instance.Pages) 
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
}
