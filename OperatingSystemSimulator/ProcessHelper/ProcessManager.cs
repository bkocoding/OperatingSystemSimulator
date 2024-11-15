using Microsoft.UI.Xaml.Controls.Primitives;
using OperatingSystemSimulator.Extras.ConsoleLogger;

namespace OperatingSystemSimulator.ProcessHelper
{
    public class ProcessManager
    {
        private static ProcessManager? instance;
        private static readonly object lockObject = new();

        private readonly Dictionary<int, ProcessBlock> processBlocks;
        private int nextPid = 100;

        private ProcessManager()
        {
            processBlocks = new Dictionary<int, ProcessBlock>();
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

        public ProcessBlock CreateProcess(Popup popup, object app, string name)
        {
            int pid = nextPid++;
            var processBlock = new ProcessBlock(pid, popup, app, name);

            processBlocks[pid] = processBlock;
            OnProcessCreated(processBlock);

            return processBlock;
        }

        public bool TerminateProcess(int pid, TerminateReasons reason)
        {
            var processBlock = GetProcessByPid(pid);
            processBlock.Popup.IsOpen = false;
            processBlock.Popup.Child = null;
            processBlock.Popup = null;
            
            OnProcessTerminated(processBlock, reason);

            return processBlocks.Remove(pid);
        }

        public void TerminateAllProcesses(TerminateReasons reason) {
            foreach (var processBlock in processBlocks.Values) {
                TerminateProcess(processBlock.Pid, reason);
                nextPid = 100;
            }
        }

        public ProcessBlock? GetProcessByPid(int pid)
        {
            processBlocks.TryGetValue(pid, out var processBlock);
            return processBlock;
        }

        public void BringToFront(int pid)
        {
            Popup popupInstance = ProcessManager.Instance.GetProcessByPid(pid).Popup;
            popupInstance.IsOpen = false;
            popupInstance.IsOpen = true;

        }

        private void OnProcessCreated(ProcessBlock processBlock)
        {
            ConsoleLogger.Log($"{processBlock.Name} is initialized, PID: {processBlock.Pid}", LogType.Init);
        }

        private void OnProcessTerminated(ProcessBlock processBlock, TerminateReasons reason) 
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

                default:
                    logType = LogType.Error;
                    break;
            }

            ConsoleLogger.Log(log,logType);

        }

    }
}
