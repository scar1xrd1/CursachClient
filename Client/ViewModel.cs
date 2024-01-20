using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace Client
{
    public class ViewModel : INotifyPropertyChanged
    {
        CancellationTokenSource? cancellationTokenSource;
        public bool isClosed = false;

        Visibility passwordAcceptVisibility = Visibility.Collapsed;
        Visibility loginFrame = Visibility.Visible;
        Visibility mainFrame = Visibility.Hidden;
        string labelContent = "Регистрация";
        string buttonContent = "Войти";
        string messageText = "";
        double messageOpacity = 1.0;
        SolidColorBrush? messageColor;
        bool isLoading = false;

        public Visibility PasswordAcceptVisibility { get => passwordAcceptVisibility; set { passwordAcceptVisibility = value; OnPropertyChanged(nameof(PasswordAcceptVisibility)); } }
        public Visibility LoginFrame { get => loginFrame; set { loginFrame = value; OnPropertyChanged(nameof(LoginFrame)); } }
        public Visibility MainFrame { get => mainFrame; set { mainFrame = value; OnPropertyChanged(nameof(MainFrame)); } }
        public string LabelContent { get => labelContent; set{ labelContent = value; OnPropertyChanged(nameof(LabelContent)); } }
        public string MessageText { get => messageText; set{ messageText = value; OnPropertyChanged(nameof(MessageText)); } }
        public SolidColorBrush? MessageColor { get => messageColor; set{ messageColor = value; OnPropertyChanged(nameof(MessageColor)); } }
        public double MessageOpacity { get => messageOpacity; set{ messageOpacity = value; OnPropertyChanged(nameof(MessageOpacity)); } }
        public string ButtonContent { get => buttonContent; set{ buttonContent = value; OnPropertyChanged(nameof(ButtonContent)); } }
        public bool IsLoading { get => isLoading; set { isLoading = value; OnPropertyChanged(nameof(IsLoading)); } }

        public async void ShowMessage(string message, SolidColorBrush color)
        {
            if (cancellationTokenSource != null) cancellationTokenSource.Cancel();

            MessageText = message;
            MessageColor = color;
            MessageOpacity = 1.0;
            cancellationTokenSource = new CancellationTokenSource();
            try { await Task.Run(() => StartMessageFadeAnimation(cancellationTokenSource.Token)); }
            catch (OperationCanceledException) { }
        }

        public void SwitchFrame()
        {
            if(loginFrame == Visibility.Visible)
            {
                LoginFrame = Visibility.Hidden;
                MainFrame = Visibility.Visible;
            }
            else
            {
                MainFrame = Visibility.Hidden;
                LoginFrame = Visibility.Visible;                
            }
        }

        void StartMessageFadeAnimation(CancellationToken cancellationToken)
        {
            try
            {
                Task.Run(async () =>
                {
                    await Task.Delay(1500);
                    while (MessageOpacity >= 0.0 && !isClosed)
                    {
                        MessageOpacity -= 0.01;
                        if (cancellationToken.IsCancellationRequested) return;
                        await Task.Delay(10);
                    }
                }, cancellationToken).Wait();
            }
            catch (AggregateException) { }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
