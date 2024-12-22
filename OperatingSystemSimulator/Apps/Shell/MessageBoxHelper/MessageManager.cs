using System.Collections.ObjectModel;
using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.Apps.Shell.MessageBoxHelper;
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

    /// <summary>
    /// For creating a message box with all buttons and customized names.
    /// </summary>
    /// <param name="pid"></param>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="OKButtonText"></param>
    /// <param name="notOKButtonText"></param>
    /// <param name="cancelButtonText"></param>
    /// <returns></returns>
    public MessageBlock CreateMessage(int pid, string title, string message, string OKButtonText, string notOKButtonText, string cancelButtonText)
    {
        MessageBlock messageBlock = new(nextMid, pid, title, message, OKButtonText, notOKButtonText, cancelButtonText);
        nextMid++;
        MessageBlocks.Add(messageBlock);
        messageBlock.Show();
        BringToFront(messageBlock.MId);
        ProcessManager.Instance.FocusedPopup = null;
        return messageBlock;

    }

    /// <summary>
    /// For creating a message box with only OK button and it's default name.
    /// </summary>
    /// <param name="pid"></param>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public MessageBlock CreateMessage(int pid, string title, string message)
    {
        MessageBlock messageBlock = new(nextMid, pid, title, message);
        nextMid++;
        MessageBlocks.Add(messageBlock);
        messageBlock.Show();
        BringToFront(messageBlock.MId);
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
            messageBlock.Popup!.IsOpen = false;
            messageBlock.Popup.Child = null;
            messageBlock.Popup = null;
            messageBlock.MessageBox = null;
            MessageBlocks.Remove(messageBlock);
            GC.Collect();
        }
    }
    public MessageBlock? GetMessageBlock(int mid)
    {
        return MessageBlocks.FirstOrDefault(p => p.MId == mid);
    }

    public void TerminateAllMessages()
    {
        var messageBlocksCopy = MessageBlocks.ToList();
        foreach (var messageBlock in messageBlocksCopy)
        {
            Close(messageBlock.MId);
        }
    }
}
