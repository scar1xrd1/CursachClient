using System.ComponentModel;
using System.Windows;

namespace Client
{
    public class ViewModel : INotifyPropertyChanged
    {
        Visibility passwordAcceptVisibility = Visibility.Collapsed;
        string labelContent = "Регистрация";

        public Visibility PasswordAcceptVisibility { get => passwordAcceptVisibility; set { passwordAcceptVisibility = value; OnPropertyChanged(nameof(PasswordAcceptVisibility)); } }
        public string LabelContent { get => labelContent; set{ labelContent = value; OnPropertyChanged(nameof(LabelContent)); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
