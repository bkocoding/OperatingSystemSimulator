using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls.Primitives;
using OperatingSystemSimulator.Extras.ConsoleLogger;

namespace OperatingSystemSimulator.ProcessHelper;

public class ProcessManager
{
    private static ProcessManager? instance;
    private static readonly object lockObject = new();

    public ObservableCollection<ProcessBlock> ProcessBlocks { get; private set; }

    private int nextPid = 100;
    private Popup? focusedPopup;

    private ProcessManager()
    {
        ProcessBlocks = [];
    }

    public static ProcessManager Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new ProcessManager();
                    }
                }
            }
            return instance;
        }
    }

    public bool IsTurnedOn { get; set; } = true;

    public async Task<ProcessBlock> CreateProcess(Popup popup, object app, string name, bool isSingleInstance)
    {
        HardwarePageViewModel.Instance.SetHDOperation(HDOperations.AppData);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Running);
        if (isSingleInstance)
        {
            foreach (ProcessBlock block in ProcessBlocks)
            {
                if (block.App != null)
                {
                    if (app.GetType() == block.App.GetType())
                    {
                        BringToFront(block.Pid);
                        return block;
                    }
                }
            }
        }

        int pid = nextPid++;
        var processBlock = new ProcessBlock(pid, popup, app, name);
        ProcessBlocks.Add(processBlock);
        OnProcessCreated(processBlock);
        await Task.Delay(100);
        HardwarePageViewModel.Instance.SetHDOperation(HDOperations.Idle);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Idle);
        return processBlock;
    }

    public bool TerminateProcess(int pid, TerminateReasons reason)
    {
        var processBlock = GetProcessByPid(pid);

        if (processBlock == null)
        {
            return false;
        }

        if (!processBlock.IsRequired)
        {
            processBlock.Popup.IsOpen = false;
            processBlock.Popup.Child = null;
            if (focusedPopup == processBlock.Popup)
            {
                focusedPopup = null;
            }
            processBlock.Popup = null;

            OnProcessTerminated(processBlock, reason);
            GC.Collect();
            return ProcessBlocks.Remove(processBlock);
        }
        else
        {
            if (reason == TerminateReasons.System)
            {
                OnProcessTerminated(processBlock, reason);
                return ProcessBlocks.Remove(processBlock);
            }
            else
            {
                string[] parameters = { "PID: " + pid + "\nProcess Name: " + processBlock.Name, "CRITICAL_PROCESS_DIED" };
                Frame currentFrame = (Frame)Window.Current.Content;
                OnProcessTerminated(processBlock, reason);
                ProcessBlocks.Remove(processBlock);
                currentFrame.Navigate(typeof(BugCheckPage), parameters);
                return true;
            }

        }

    }

    public bool TerminateFocusedProcess()
    {
        if (focusedPopup == null)
        {
            return false;
        }

        foreach (var block in ProcessBlocks)
        {
            if (block.Popup == focusedPopup)
            {
                TerminateProcess(block.Pid, TerminateReasons.Self);
                return true;
            }
        }
        return false;

    }
   
    public void TerminateAllProcesses(TerminateReasons reason)
    {
        foreach (var processBlock in ProcessBlocks.ToList().OrderByDescending(p => p.Pid))
        {
            if (processBlock.Pid == 1)
            {
                continue;
            }
            TerminateProcess(processBlock.Pid, reason);
        }
        nextPid = 100;
    }

    public ProcessBlock? GetProcessByPid(int pid)
    {
        return ProcessBlocks.FirstOrDefault(p => p.Pid == pid);
    }

    public void StartOSServices()
    {
        ProcessBlock[] OSServices =
        [
            new(1, "Kernel"),
            new(2, "Time Service"),
            new(3, "Network Service")
        ];
        foreach (var service in OSServices)
        {
            ProcessBlocks.Add(service);
            OnProcessCreated(service);
        }
    }

    public void StartLogOnUser()
    {
        ProcessBlock[] LogOnServices =
        [
            new(4, "LogOn Service"),
            new(5, "Desktop Service")
        ];
        foreach (var service in LogOnServices)
        {
            ProcessBlocks.Add(service);
            OnProcessCreated(service);
        }
    }

    public async void BringToFront(int pid)
    {
        var processBlock = GetProcessByPid(pid);
        if (processBlock != null)
        {
            if (focusedPopup != processBlock.Popup)
            {
                HardwarePageViewModel.Instance.SetRunningProcess(processBlock.Name);
                focusedPopup = processBlock.Popup;
                processBlock.Popup.IsOpen = false;
                processBlock.Popup.IsOpen = true;
                await Task.Delay(400);
                HardwarePageViewModel.Instance.SetRunningProcess("Kernel");
            }
        }
    }

    private async void OnProcessCreated(ProcessBlock processBlock)
    {
        ConsoleLogger.Log($"{processBlock.Name} is initialized, PID: {processBlock.Pid}", LogType.Init);
        if (processBlock.Popup != null)
        {
            focusedPopup = processBlock.Popup;
            HardwarePageViewModel.Instance.SetRunningProcess(processBlock.Name);
            await Task.Delay(1000);
            HardwarePageViewModel.Instance.SetRunningProcess("Kernel");
        }
    }

    private static void OnProcessTerminated(ProcessBlock processBlock, TerminateReasons reason)
    {
        string log = $"{processBlock.Name} is terminated, PID: {processBlock.Pid}, Reason: {reason.GetDescription()}";
        LogType logType;

        switch (reason)
        {
            case TerminateReasons.Self:
                logType = LogType.Info;
                break;

            case TerminateReasons.System:
                logType = LogType.Warning;
                break;

            case TerminateReasons.User:
                logType = LogType.Warning;
                break;

            default:
                logType = LogType.Error;
                break;
        }

        ConsoleLogger.Log(log, logType);
    }

}
