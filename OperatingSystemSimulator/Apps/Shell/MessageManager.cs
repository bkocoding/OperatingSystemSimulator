using System.Collections.ObjectModel;
using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.Apps.Shell;
public class MessageManager
{
    private static MessageManager? instance;
    private static readonly object lockObject = new();

    public ObservableCollection<MessageBlock> MessageBlocks { get; private set; }
    private int nextMid = 10;

    public static MessageManager Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new MessageManager();
                    }
                }
            }
            return instance;
        }
    }
    private MessageManager()
    {
        MessageBlocks = [];
    }

    public MessageBlock CreateMessage(int pid, string title, string message, bool hasCancel)
    {
        MessageBlock messageBlock = new(nextMid, pid, title, message, hasCancel);
        nextMid++;
        MessageBlocks.Add(messageBlock);
        messageBlock.Show();
        BringToFront(messageBlock.Mid);
        ProcessManager.Instance.FocusedPopup = null;
        return messageBlock;

    }
    public void BringToFront(int mid)
    {
        MessageBlock? messageBlock = GetMessageBlock(mid);
        if (messageBlock != null)
        {
            ProcessManager.Instance.FocusedPopup = null;
            messageBlock.Popup.IsOpen = false;
            messageBlock.Popup.IsOpen = true;
        }
    }
    public void Close(int mid)
    {
        MessageBlock? messageBlock = GetMessageBlock(mid);
        if (messageBlock != null)
        {
            messageBlock.Popup.IsOpen = false;
            messageBlock.Popup.Child = null;
            messageBlock.Popup = null;
            messageBlock.MessageBox = null;
            MessageBlocks.Remove(messageBlock);
            GC.Collect();
        }
    }
    public MessageBlock? GetMessageBlock(int mid)
    {
        return MessageBlocks.FirstOrDefault(p => p.Mid == mid);
    }
}
