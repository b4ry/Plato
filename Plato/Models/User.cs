using System.ComponentModel;

namespace Plato.Models
{
    public class User : INotifyPropertyChanged
    {
        private bool _hasNewMessage;

        public string Name { get; set; }
        public bool HasNewMessage
        {
            get => _hasNewMessage;
            set
            {
                _hasNewMessage = value;

                OnPropertyChanged(nameof(HasNewMessage));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
