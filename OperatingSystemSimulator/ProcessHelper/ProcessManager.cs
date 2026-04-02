using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls.Primitives;
using OperatingSystemSimulator.Apps.Interfaces;
using OperatingSystemSimulator.Apps.Shell.Enums;

namespace OperatingSystemSimulator.ProcessHelper;

public class ProcessManager
{
    private static ProcessManager? instance;
    private static readonly object lockObject = new();

    private readonly Random random = new();
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

    public Queue<ProcessBlock> operationQueue = new();
    private bool isProcessingQueue = false;

    private bool isInterruptActive = false;
    private string? interruptOperation;

    private readonly BIOSSettingsService _biosSettingsService = (Application.Current as App)?.Host?.Services.GetRequiredService<BIOSSettingsService>()!;

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

    public bool IsTurnedOn = true;


    public async Task<ProcessBlock?> CreateProcess(IApp app, string name, bool isSingleInstance, bool isUtilizationEnough)
    {
        HardwarePageViewModel.Instance.SetHDOperation(HDOperations.AppData);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Running);
        await Task.Delay(100);
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

        var processBlock = new ProcessBlock(nextPid, popup, app, name, isUtilizationEnough);

        UtilizationResult result = MemoryManager.Instance.AllocateMemory(processBlock);

        if (result == UtilizationResult.OutOfMemory)
        {
            await RandomDelay(true);
            HardwarePageViewModel.Instance.SetHDOperation(HDOperations.Idle);
            HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Idle);
            ConsoleLogger.Log($"Couldn't initialize {processBlock.Name}, Reason: {result.GetDescription()}", LogType.Error);
            MessageManager.Instance.CreateMessage(1, "Out Of Resources",
                $"You can not start {processBlock.Name} because there are not enough memory space. Try closing some of your apps.\n\nError Code: {result.GetDescription()}", ShellType.None);
            return null;
        }

        nextPid++;
        ProcessBlocks.Add(processBlock);

        //await RandomDelay(true);
        HardwarePageViewModel.Instance.SetHDOperation(HDOperations.Idle);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Idle);
        await OnProcessCreated(processBlock);
        return processBlock;
    }

    public async Task<bool> TerminateProcess(int pid, TerminateReasons reason, bool? needsPrinting = true)
    {
        var processBlock = GetProcessByPid(pid);

        if (processBlock == null)
        {
            return false;
        }

        if (!processBlock.IsRequired)
        {
            if (reason != TerminateReasons.Unexpected)
            {
                await InterruptQueueAsync(pid);
            }

            processBlock.Popup!.IsOpen = false;
            processBlock.Popup.Child = null;
            if (FocusedPopup == processBlock.Popup)
            {
                foreach (var block in ProcessBlocks.Reverse())
                {
                    if (block.Pid != pid && block.Popup != null)
                    {
                        FocusedPopup = block.Popup;
                        break;
                    }
                    else
                    {
                        FocusedPopup = null;
                    }
                }

            }
            processBlock.Popup = null;

            if (needsPrinting == true)
            {
                OnProcessTerminated(processBlock, reason);
            }

            if (processBlock.App is WebBrowserApp webBrowserApp)
            {
                webBrowserApp.BrowserViewModel.TryDispose();
            }
            FileDialogManager.Instance.TerminateAllFileDialogs(pid);
            MessageManager.Instance.TerminateAllMessages(pid);
            MemoryManager.Instance.DeallocateMemory(processBlock);
            GC.Collect();
            return ProcessBlocks.Remove(processBlock);
        }
        else
        {
            if (reason == TerminateReasons.System)
            {
                if (needsPrinting == true)
                {
                    OnProcessTerminated(processBlock, reason);
                }

                MemoryManager.Instance.DeallocateMemory(processBlock);
                return ProcessBlocks.Remove(processBlock);
            }
            else
            {
                string[] parameters = { "PID: " + pid, "Process Name: " + processBlock.Name, "CRITICAL_PROCESS_DIED" };
                Frame currentFrame = (Frame)Window.Current!.Content!;
                OnProcessTerminated(processBlock, reason);
                MemoryManager.Instance.DeallocateMemory(processBlock);
                ProcessBlocks.Remove(processBlock);
                currentFrame!.Navigate(typeof(BugCheckPage), parameters);
                return true;
            }

        }

    }

    public async Task<bool> TerminateFocusedProcess()
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
                   await TerminateProcess(block.Pid, TerminateReasons.Self);
                }
                return true;
            }
        }
        return false;

    }

    public async void TerminateAllProcesses(TerminateReasons reason)
    {
        ProcessManagerScheduler.StopRunServiceScheduler();
        foreach (var processBlock in ProcessBlocks.ToList().OrderByDescending(p => p.Pid))
        {
            if (processBlock.Pid == 1)
            {
                continue;
            }
            else if (reason != TerminateReasons.Unexpected && reason != TerminateReasons.System && processBlock.App is NotepadApp notepad)
            {
                notepad.TryTerminate();
            }
            else if (processBlock.App is NotepadApp notepadApp)
            {
                notepadApp.UnsubscribeToFocusedPopUpChangedEvent();
                await TerminateProcess(processBlock.Pid, reason);
            }
            else if (processBlock.App is FileExplorerApp fileExplorerApp)
            {
                fileExplorerApp.UnsubscribeToFocusedPopUpChangedEvent();
                await TerminateProcess(processBlock.Pid, reason);
            }
            else
            {
                await TerminateProcess(processBlock.Pid, reason);
            }
        }
        nextPid = 100;
    }

    public ProcessBlock? GetProcessByPid(int pid)
    {
        return ProcessBlocks.FirstOrDefault(p => p.Pid == pid);
    }

    public async Task EnqueueRunningProcessAsync(int pid)
    {
        var processBlock = GetProcessByPid(pid);
        if (processBlock == null)
            return;

        lock (operationQueue)
        {
            operationQueue.Enqueue(processBlock);
        }
        ConsoleLogger.Log($"{processBlock.Name} is enqueued, PID: {processBlock.Pid}", LogType.Queue);
        await ProcessQueueAsync();
    }

    public async Task<bool> InterruptQueueAsync(int pid)
    {
        var processBlock = GetProcessByPid(pid);
        if (processBlock == null)
            return false;

        TaskCompletionSource<bool> tcs = new();

        lock (operationQueue)
        {
            isInterruptActive = true;
            interruptOperation = processBlock.Name;
        }

        ConsoleLogger.Log($"New interrupt request by process {processBlock.Name}, PID: {pid}", LogType.Interrupt);
        await ProcessQueueAsync();

        tcs.SetResult(true);
        return await tcs.Task;
    }

    private async Task ProcessQueueAsync()
    {
        if (isProcessingQueue) return;

        isProcessingQueue = true;
        string lastProcessName = "Kernel";

        while (true)
        {
            ProcessBlock? processBlock = null;
            string? processName = null;
            string queueState = string.Empty;

            //lock (operationQueue)
            //{
            if (isInterruptActive && interruptOperation != null)
            {
                processName = interruptOperation;
                isInterruptActive = false;
                interruptOperation = null;
            }
            else if (operationQueue.Count > 0)
            {
                processBlock = operationQueue.Dequeue();
                processName = processBlock.Name;
            }

            if (operationQueue.Count > 0)
            {
                queueState = $"[{string.Join(", ", operationQueue)}]";
            }
            //}

            if (!string.IsNullOrEmpty(processName))
            {
                if (processName != lastProcessName)
                {
                    HardwarePageViewModel.Instance.IndicateContextSwitch(lastProcessName, processName);
                    // Log Context Switch for observable visibility in terminal logs
                    ConsoleLogger.Log($"Context Switch: Dispatching '{processName}', saving context for '{lastProcessName}'", LogType.Queue);
                    lastProcessName = processName;
                    await Task.Delay(50); // Simulating context switch penalty time
                }

                HardwarePageViewModel.Instance.SetRunningProcess($"{processName} {queueState}");

                ConsoleLogger.Log($"{processName} is processing" +
                $"{(processBlock != null ? $", PID: {processBlock.Pid}" : "")}" +
                $"{(!string.IsNullOrEmpty(queueState) ? $". Current Queue: {queueState}" : "")}",
                LogType.Queue);


                if (processBlock != null)
                {
                    if (!processBlock.IsInitialized)
                    {
                        processBlock.InitializePopup();
                        processBlock.IsInitialized = true;
                    }
                }

                await RandomDelay(false);
            }
            else
            {
                HardwarePageViewModel.Instance.SetRunningProcess($"Kernel {queueState}");
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
            delay = random.Next(201, 250);
        }
        else
        {
            delay = random.Next(100, 200);
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

        OSServices[0].Size = 6171428;
        OSServices[1].Size = 79588;
        OSServices[2].Size = 3125425;

        foreach (var service in OSServices)
        {
            ProcessBlocks.Add(service);
            MemoryManager.Instance.AllocateMemory(service);
            OnProcessCreated(service);
            EnqueueRunningProcessAsync(service.Pid);
        }
        ProcessManagerScheduler.StartRunServiceScheduler();
    }

    public void StartLogOnUser()
    {
        ProcessBlock[] LogOnServices =
        {
            new(4, "LogOn Service", true),
            new(5, "Desktop Service", true)
        };

        LogOnServices[0].Size = 1280000;
        LogOnServices[1].Size = 2280000;

        foreach (var service in LogOnServices)
        {
            ProcessBlocks.Add(service);
            MemoryManager.Instance.AllocateMemory(service);
            OnProcessCreated(service);
            EnqueueRunningProcessAsync(service.Pid);
        }
    }

    public async void BringToFront(int pid)
    {
        var processBlock = GetProcessByPid(pid);
        if (processBlock != null && FocusedPopup != processBlock.Popup && processBlock.Popup != null)
        {
            FocusedPopup = processBlock.Popup;
            processBlock.Popup.IsOpen = false;
            processBlock.Popup.IsOpen = true;

            await InterruptQueueAsync(processBlock.Pid);
        }
    }

    private async Task OnProcessCreated(ProcessBlock processBlock)
    {
        if (processBlock.Popup != null)
        {
            FocusedPopup = processBlock.Popup;
            await EnqueueRunningProcessAsync(processBlock.Pid);
        }
        ConsoleLogger.Log($"{processBlock.Name} is initialized, PID: {processBlock.Pid}", LogType.Init);
    }

    private static async void OnProcessTerminated(ProcessBlock processBlock, TerminateReasons reason)
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
        await Task.Delay(300);
        ConsoleLogger.Log(log, logType);
    }

    public void RunService(int pId)
    {
        EnqueueRunningProcessAsync(pId);
    }

}
