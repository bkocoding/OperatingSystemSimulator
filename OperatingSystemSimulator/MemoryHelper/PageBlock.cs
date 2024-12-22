using System.ComponentModel;
using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.MemoryHelper;
public class PageBlock : INotifyPropertyChanged
{
    private bool isEmpty;
    private bool isAllocated;
    private int usedSpace;

    public int PageNumber { get; set; }
    public ProcessBlock? ProcessBlock { get; set; }

    public int UsedSpace
    {
        get => usedSpace;
        set
        {
            if (usedSpace != value)
            {
                usedSpace = value;
                OnPropertyChanged(nameof(UsedSpace));
            }
        }
    }

    public bool IsEmpty
    {
        get => isEmpty;
        set
        {
            if (isEmpty != value)
            {
                isEmpty = value;
                OnPropertyChanged(nameof(IsEmpty));
            }
        }
    }

    public bool IsAllocated
    {
        get => isAllocated;
        set
        {
            if (isAllocated != value)
            {
                isAllocated = value;
                OnPropertyChanged(nameof(IsAllocated));
            }
        }
    }

    public bool IsAdditional { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
