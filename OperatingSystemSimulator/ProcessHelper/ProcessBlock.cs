using Microsoft.UI.Xaml.Controls.Primitives;
using OperatingSystemSimulator.Apps;

namespace OperatingSystemSimulator.ProcessHelper;

public class ProcessBlock
{
    private readonly Random random = new Random();
    private double previousWidthOffset = 200;
    private double previousHeightOffset = 200;

    public int Pid { get; }
    public Popup? Popup { get; set; }
    public object? App { get; }
    public string Name { get; }
    public bool IsIdle { get; set; } = false;
    public bool IsRequired { get; set; } = false;
    public bool HasUI { get; set; } = true;
    public bool IsUtilizationEnough { get; set; } = true;
    public int Size { get; set; }

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
        IntializePID();
        InitializePopup();
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
        IntializePID();
    }
    
    private void IntializePID()
    {
        if (App is TestApp testApp)
        {
            testApp.Pid = Pid;
        }
        else if (App is TaskManagerApp taskManagerApp)
        {
            taskManagerApp.Pid = Pid;
        }
        else if (App is NotepadApp notepadApp)
        {
            notepadApp.Pid = Pid;
        }

    }

    private void InitializePopup()
    {

        Popup.Child = App as UIElement;

        double newWidthOffset;
        double newHeightOffset;

        do
        {
            newWidthOffset = random.Next(50, 601);
        } while (newWidthOffset == previousWidthOffset);

        do
        {
            newHeightOffset = random.Next(50, 601);
        } while (newHeightOffset == previousHeightOffset);

        previousWidthOffset = newWidthOffset;
        previousHeightOffset = newHeightOffset;

        Popup.HorizontalOffset = (Window.Current.Bounds.Width - newWidthOffset) / 2;
        Popup.VerticalOffset = (Window.Current.Bounds.Height - newHeightOffset) / 4;
        Popup.IsOpen = true;
    }
}
