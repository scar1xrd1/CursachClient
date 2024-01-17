using Client.Images;
using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;

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

        async Task SendServer(string message)
        {
            string feedback = await udp.SendAsync(message);
            //MessageBox.Show(feedback);
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

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string? content = (sender as Button).Content.ToString();

            if (content == "Войти")
            {
                if (loginTextBox.Text.Length >= 6)
                {
                    if (passwordTextBox.Password.Length >= 6)
                    {
                        loginGrid.Opacity = 0.3;
                        ApplyBlurEffect(loginGrid, 15);
                        viewModel.IsLoading = true;

                        string login = loginTextBox.Text;
                        string password = passwordTextBox.Password;
                        bool result = await Task.Run(() => IsUserDataSuccess(login, password));

                        if (result)
                        {
                            loginGrid.Opacity = 1.0;
                            loginGrid.Effect = null;
                            viewModel.IsLoading = false;

                            MessageBox.Show("Успешный вход");
                        }
                        else
                        {
                            loginGrid.Opacity = 1.0;
                            loginGrid.Effect = null;
                            viewModel.IsLoading = false;

                            MessageBox.Show("Не удалось войти");
                        }
                    }
                }
            }
            else
            {
                if (loginTextBox.Text.Length >= 6)
                {
                    if (passwordTextBox.Password.Length >= 6)
                    {
                        if (passwordTextBox.Password == passwordAcceptTextBox.Password)
                        {
                            loginGrid.Opacity = 0.3;
                            ApplyBlurEffect(loginGrid, 15);
                            viewModel.IsLoading = true;

                            string login = loginTextBox.Text;
                            bool result = await Task.Run(() => IsLoginExist(login));
                            if (result)
                            {
                                //MessageBox.Show("Логин занят!");
                                loginGrid.Opacity = 1.0;
                                loginGrid.Effect = null;
                                viewModel.IsLoading = false;
                            }
                            else
                            {
                                string password = passwordTextBox.Password;
                                string salt = PasswordHasher.GenerateSalt();

                                try
                                {
                                    using (DatabaseContext db = new DatabaseContext())
                                    {
                                        db.Users.Add(new User(login, PasswordHasher.HashPassword(password, salt), salt));
                                        db.SaveChanges();
                                    }

                                    await SendServer("updateusers");
                                }
                                catch (Exception ex) { MessageBox.Show(ex.Message); }

                                //MessageBox.Show("логин свободн");
                                loginGrid.Opacity = 1.0;
                                loginGrid.Effect = null;
                                viewModel.IsLoading = false;
                            }
                        }
                    }
                }
            }
        }

        async Task<bool> IsLoginExist(string login)
        {
            try
            {
                using(var db = new DatabaseContext())
                {
                    return db.IsLoginExist(login);
                }
            }
            catch { }
            return false;
        }

        async Task<bool> IsUserDataSuccess(string login, string password)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    return db.IsUserDataSuccess(login, password);
                }
            }
            catch { }
            return false;
        }

        static void ApplyBlurEffect(UIElement target, int radius)
        {
            if (target == null) return;
            BlurEffect blurEffect = new BlurEffect();
            blurEffect.Radius = radius;
            target.Effect = blurEffect;
        }
    }
}