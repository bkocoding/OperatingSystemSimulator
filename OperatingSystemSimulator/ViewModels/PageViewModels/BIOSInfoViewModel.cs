using System.ComponentModel;
using System.Runtime.CompilerServices;
using OperatingSystemSimulator.MemoryHelper;

namespace OperatingSystemSimulator.ViewModels.PageViewModels
{
    public class BIOSInfoViewModel : INotifyPropertyChanged
    {
        private string _currentTime;
        private string _currentDate;
        private Timer _timer;


        public event PropertyChangedEventHandler PropertyChanged;

        public string CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                OnPropertyChanged();
            }
        }

        public string CurrentDate
        {
            get => _currentDate;
            set
            {
                _currentDate = value;
                OnPropertyChanged();
            }
        }

        public string MemorySize { 
            get => $": {(MemoryManager.memorySize / 1000000)} MB";
        }

        public BIOSInfoViewModel()
        {            
            UpdateTimeAndDate(); 
            _timer = new Timer(UpdateTimeAndDateCallback, null, 0, 1000);
        }

        private void UpdateTimeAndDateCallback(object state)
        {
            UpdateTimeAndDate();
        }

        private void UpdateTimeAndDate()
        {
            CurrentTime = ": " + DateTime.Now.ToString("HH:mm:ss");
            CurrentDate = ": " + DateTime.Now.ToString("dd.MM.yyyy");
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void StopTimer()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void StartTimer()
        {
            _timer?.Change(0, 1000);
        }
    }
}
