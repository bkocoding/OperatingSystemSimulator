using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls.Primitives;
using OperatingSystemSimulator.Apps;
using OperatingSystemSimulator.Extras.ConsoleLogger;
using Uno.Disposables;

namespace OperatingSystemSimulator.ProcessHelper;

public class ProcessManager
{
    private static ProcessManager? instance;
    private static readonly object lockObject = new();

    private Random random = new Random();
    public ObservableCollection<ProcessBlock> ProcessBlocks { get; private set; }
    private int nextPid = 100;

    public event Action<Popup?>? FocusedPopupChanged;

    private Popup? focusedPopup;
    public Popup? FocusedPopup
    {
        get => focusedPopup;
        set
        {
            if (focusedPopup != value)
            {
                focusedPopup = value;
                FocusedPopupChanged?.Invoke(focusedPopup);
            }
        }
    }

    private ProcessManager()
    {
        ProcessBlocks = [];
    }

    private Queue<string> operationQueue = new();
    private bool isProcessingQueue = false;

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

    public async Task<ProcessBlock> CreateProcess(object app, string name, bool isSingleInstance, bool isUtilizationEnough)
    {
        HardwarePageViewModel.Instance.SetHDOperation(HDOperations.AppData);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Running);

        Popup popup = new();

        if (isSingleInstance)
        {
            foreach (ProcessBlock block in ProcessBlocks)
            {
                if (block.App != null && app.GetType() == block.App.GetType())
                {
                    BringToFront(block.Pid);
                    return block;
                }
            }
        }

        int pid = nextPid++;
        var processBlock = new ProcessBlock(pid, popup, app, name, isUtilizationEnough);
        ProcessBlocks.Add(processBlock);
        OnProcessCreated(processBlock);

        await RandomDelay(true);
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
            if (FocusedPopup == processBlock.Popup)
            {
                FocusedPopup = null;
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
        if (FocusedPopup == null)
        {
            return false;
        }

        foreach (var block in ProcessBlocks)
        {
            if (block.Popup == FocusedPopup)
            {
                if (block.App is NotepadApp notepad)
                {
                    notepad.TryTerminate();
                }
                else
                {
                    TerminateProcess(block.Pid, TerminateReasons.Self);
                }
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
            else if (reason != TerminateReasons.Unexpected && processBlock.App is NotepadApp notepad)
            {
                notepad.TryTerminate();
            }
            else if(processBlock.App is NotepadApp notepadApp)
            {
                notepadApp.UnsubscribeToFocusedPopUpChangedEvent();
                TerminateProcess(processBlock.Pid, reason);
            }
            else
            {
                TerminateProcess(processBlock.Pid, reason);
            }
        }
        nextPid = 100;
    }

    public ProcessBlock? GetProcessByPid(int pid)
    {
        return ProcessBlocks.FirstOrDefault(p => p.Pid == pid);
    }

    private void EnqueueRunningProcess(string processName)
    {
        lock (operationQueue)
        {
            operationQueue.Enqueue(processName);
        }
        ProcessQueue();
    }

    private async void ProcessQueue()
    {
        if (isProcessingQueue) return;

        isProcessingQueue = true;

        while (true)
        {
            string? processName = null;

            lock (operationQueue)
            {
                if (operationQueue.Count > 0)
                {
                    processName = operationQueue.Dequeue();
                }
            }

            if (!string.IsNullOrEmpty(processName))
            {
                HardwarePageViewModel.Instance.SetRunningProcess(processName);
                await RandomDelay(false);
            }
            else
            {
                HardwarePageViewModel.Instance.SetRunningProcess("Kernel");
                break;
            }
        }

        isProcessingQueue = false;
    }

    private async Task RandomDelay(bool isSmallOperation)
    {
        int delay;
        if (!isSmallOperation)
        {
            delay = random.Next(300, 601);
        }
        else
        {
            delay = random.Next(250, 301);
        }
        await Task.Delay(delay);
    }

    public void StartOSServices()
    {
        ProcessBlock[] OSServices =
        {
            new(1, "Kernel", false),
            new(2, "Time Service", false),
            new(3, "Network Service", true)
        };

        foreach (var service in OSServices)
        {
            ProcessBlocks.Add(service);
            OnProcessCreated(service);
            EnqueueRunningProcess(service.Name);
        }
    }

    public void StartLogOnUser()
    {
        ProcessBlock[] LogOnServices =
        {
            new(4, "LogOn Service", true),
            new(5, "Desktop Service", true)
        };

        foreach (var service in LogOnServices)
        {
            ProcessBlocks.Add(service);
            OnProcessCreated(service);
            EnqueueRunningProcess(service.Name);
        }
    }

    public void BringToFront(int pid)
    {
        var processBlock = GetProcessByPid(pid);
        if (processBlock != null && FocusedPopup != processBlock.Popup)
        {
            FocusedPopup = processBlock.Popup;
            processBlock.Popup.IsOpen = false;
            processBlock.Popup.IsOpen = true;

            EnqueueRunningProcess(processBlock.Name);
        }
    }

    private void OnProcessCreated(ProcessBlock processBlock)
    {
        ConsoleLogger.Log($"{processBlock.Name} is initialized, PID: {processBlock.Pid}", LogType.Init);
        if (processBlock.Popup != null)
        {
            FocusedPopup = processBlock.Popup;
            EnqueueRunningProcess(processBlock.Name);
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
