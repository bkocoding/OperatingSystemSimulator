using System.ComponentModel;

namespace OperatingSystemSimulator.MemoryHelper;
public class PageBlock : INotifyPropertyChanged
{
    private bool isEmpty;
    private bool isAllocated;
    private bool isSelected = false;
    private int usedSpace;
    public PageBlock Instance { get; private set; }
    public int PageNumber { get; set; }
    public ProcessBlock? ProcessBlock { get; set; }
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (isSelected != value)
            {
                isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                OnPropertyChanged(nameof(Instance));
            }
        }
    }
    public int UsedSpace
    {
        get => usedSpace;
        set
        {
            if (usedSpace != value)
            {
                usedSpace = value;
                OnPropertyChanged(nameof(UsedSpace));
                OnPropertyChanged(nameof(Instance));
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
                OnPropertyChanged(nameof(Instance));
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
                OnPropertyChanged(nameof(Instance));
            }
        }
    }

    public bool IsAdditional { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public PageBlock()
    {
        Instance = this;
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
