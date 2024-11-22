using System.Collections.ObjectModel;
using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.ViewModels.AppViewModels;
public class TaskManagerViewModel
{
    public ObservableCollection<ProcessBlock> Processes { get; set; }

    public TaskManagerViewModel()
    {
        Processes = ProcessManager.Instance.ProcessBlocks;
    }

}
