using OperatingSystemSimulator.MemoryHelper;

namespace OperatingSystemSimulator.Hardware;
public sealed partial class PageListPage : Page
{
    public PageListPage()
    {
        InitializeComponent();
        DataContext = MemoryManager.Instance;
    }
}
