using System.Collections.ObjectModel;
using System.Collections.Specialized;
using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.ViewModels.AppViewModels;
public class TaskManagerViewModel
{
    public ObservableCollection<ProcessBlock> Processes { get; private set; }

    public TaskManagerViewModel()
    {
        Processes = [];

        var originalProcesses = ProcessManager.Instance.ProcessBlocks;
        foreach (var process in originalProcesses.Skip(1))
        {
            Processes.Add(process);
        }
        originalProcesses.CollectionChanged += OnOriginalProcessesChanged;
    }

    private void OnOriginalProcessesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (ProcessBlock newProcess in e.NewItems!)
                {
                    if (ProcessManager.Instance.ProcessBlocks.IndexOf(newProcess) != 0)
                    {
                        Processes.Add(newProcess);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                foreach (ProcessBlock removedProcess in e.OldItems!)
                {
                    Processes.Remove(removedProcess);
                }
                break;

            case NotifyCollectionChangedAction.Replace:
                foreach (ProcessBlock oldProcess in e.OldItems!)
                {
                    Processes.Remove(oldProcess);
                }
                foreach (ProcessBlock newProcess in e.NewItems!)
                {
                    if (ProcessManager.Instance.ProcessBlocks.IndexOf(newProcess) != 0)
                    {
                        Processes.Add(newProcess);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                Processes.Clear();
                foreach (var process in ProcessManager.Instance.ProcessBlocks.Skip(1))
                {
                    Processes.Add(process);
                }
                break;
        }
    }
}
