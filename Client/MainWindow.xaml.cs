using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        UDP udp = new UDP();
        ViewModel viewModel = new ViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        void Send()
        {
            Task.Run(() => SendServer("aboba"));
        }

        async Task SendServer(string message)
        {
            string feedback = await udp.SendAsync(message);
            MessageBox.Show(feedback);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Send();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { 
            viewModel.LabelContent = viewModel.LabelContent == "Регистрация" ? "Вход" : "Регистрация";
            viewModel.ButtonContent = viewModel.ButtonContent == "Войти" ? "Зарегистрироваться" : "Войти"; 
            viewModel.PasswordAcceptVisibility = viewModel.PasswordAcceptVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}