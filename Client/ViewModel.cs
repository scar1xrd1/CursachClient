using System.ComponentModel;
using System.Windows;

namespace Client
{
    public class ViewModel : INotifyPropertyChanged
    {
        Visibility passwordAcceptVisibility = Visibility.Collapsed;
        string labelContent = "Регистрация";
        string buttonContent = "Войти";
        bool isLoading = false;

        public Visibility PasswordAcceptVisibility { get => passwordAcceptVisibility; set { passwordAcceptVisibility = value; OnPropertyChanged(nameof(PasswordAcceptVisibility)); } }
        public string LabelContent { get => labelContent; set{ labelContent = value; OnPropertyChanged(nameof(LabelContent)); } }
        public string ButtonContent { get => buttonContent; set{ buttonContent = value; OnPropertyChanged(nameof(ButtonContent)); } }
        public bool IsLoading { get => isLoading; set { isLoading = value; OnPropertyChanged(nameof(IsLoading)); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
