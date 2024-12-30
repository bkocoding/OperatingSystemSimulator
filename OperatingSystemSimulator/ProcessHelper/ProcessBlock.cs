using Microsoft.UI.Xaml.Controls.Primitives;
using Newtonsoft.Json;
using OperatingSystemSimulator.Apps;
using OperatingSystemSimulator.Apps.WebBrowser;
using OperatingSystemSimulator.MemoryHelper;

namespace OperatingSystemSimulator.ProcessHelper;

public class ProcessBlock
{
    private readonly Random random = new Random();
    private double previousWidthOffset = 200;
    private double previousHeightOffset = 200;

    public int Pid { get; }

    [JsonIgnore]
    public Popup? Popup { get; set; }

    [JsonIgnore]
    public object? App { get; }
    public string Name { get; }
    public bool IsIdle { get; set; } = false;

    [JsonIgnore]
    public bool IsRequired { get; set; } = false;

    [JsonIgnore]
    public bool HasUI { get; set; } = true;

    [JsonIgnore]
    public bool IsUtilizationEnough { get; set; } = true;
    public int Size { get; set; }

    [JsonIgnore]
    public bool IsInitialized { get; set; } = false;

    public List<PageBlock> PageBlocks { get; set; } = [];

    /// <summary>
    /// For creating a new UI application Process Block, User Level and not required for OS
    /// </summary>
    /// <param name="pid"></param>
    /// <param name="popup"></param>
    /// <param name="app"></param>
    /// <param name="name"></param>
    /// <param name="isUtilizationEnough"></param>
    public ProcessBlock(int pid, Popup popup, object app, string name, bool isUtilizationEnough)
    {
        Pid = pid;
        Popup = popup;
        App = app;
        Name = name;
        IsUtilizationEnough = isUtilizationEnough;
        IntializeApp();
    }
    /// <summary>
    /// For creating a new UI-less application Process Block, Kernel Level and required for OS
    /// </summary>
    /// <param name="pid"></param>
    /// <param name="name"></param>
    public ProcessBlock(int pid, string name, bool isUtilizationEnough) 
    {
        Pid = pid;
        Name = name;
        IsUtilizationEnough = isUtilizationEnough;
        IsRequired = true;
        HasUI = false;
        IntializeApp();
    }
    
    private void IntializeApp()
    {
        if (App is TestApp testApp)
        {
            testApp.Pid = Pid;
            Size = 1280000;
        }
        else if (App is TaskManagerApp taskManagerApp)
        {
            taskManagerApp.Pid = Pid;
            Size = 3200465;
        }
        else if (App is NotepadApp notepadApp)
        {
            notepadApp.Pid = Pid;
            Size = 4171428;
        }
        else if (App is FileExplorerApp explorerApp)
        {
            explorerApp.Pid = Pid;
            Size = 7171428;
        }
        else if (App is WebBrowserApp webBrowserApp) 
        {
            webBrowserApp.Pid = Pid;
            Size = 9171429;
        }

    }

    public void InitializePopup()
    {
        if (!HasUI)
            return;

        Popup!.Child = App as UIElement;

        double newWidthOffset;
        double newHeightOffset;

        do
        {
            newWidthOffset = random.Next(50, (int)(Window.Current!.Bounds.Width)/2);
        } while (newWidthOffset == previousWidthOffset);
        
        do
        {
            newHeightOffset = random.Next(50, (int)(Window.Current.Bounds.Width)/2);
        } while (newHeightOffset == previousHeightOffset);

        previousWidthOffset = newWidthOffset;
        previousHeightOffset = newHeightOffset;

        int verticalOne = 2;
        int horizontalOne = 4;

        if (App is FileExplorerApp || App is WebBrowserApp)
        {
            verticalOne = 4;
            horizontalOne = 8;
        }

     

        Popup.HorizontalOffset = (Window.Current.Bounds.Width - newWidthOffset) / verticalOne;
        Popup.VerticalOffset = (Window.Current.Bounds.Height - newHeightOffset) / horizontalOne;
        Popup.IsOpen = true;
    }

    public override string ToString()
    {
        return Name;
    }
}
