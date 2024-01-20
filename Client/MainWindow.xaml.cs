using Client.Images;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {        
        ViewModel viewModel = new ViewModel();
        ClientClass client = new ClientClass();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            viewModel.isClosed = true;
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
                if (loginTextBox.Text.Length >= 6 && loginTextBox.Text.Length <= 50)
                {
                    if (passwordTextBox.Password.Length >= 6 && passwordTextBox.Password.Length <= 50)
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
                            loginTextBox.Text = string.Empty;
                            passwordTextBox.Password = string.Empty;
                            passwordAcceptTextBox.Password = string.Empty;

                            viewModel.SwitchFrame();
                        }
                        else
                        {
                            loginGrid.Opacity = 1.0;
                            loginGrid.Effect = null;
                            viewModel.IsLoading = false;

                            viewModel.ShowMessage("Неверный логин или пароль", MessageColor.Red);
                        }
                    } 
                    else viewModel.ShowMessage("Минимум 6 символов", MessageColor.Red);  
                }
                else viewModel.ShowMessage("Минимум 6 символов", MessageColor.Red);
            }
            else
            {
                if (loginTextBox.Text.Length >= 6 && !string.IsNullOrEmpty(loginTextBox.Text) && loginTextBox.Text.Length <= 50)
                {
                    if (passwordTextBox.Password.Length >= 6 && !string.IsNullOrEmpty(passwordTextBox.Password) && passwordTextBox.Password.Length <= 50)
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
                                viewModel.ShowMessage("Логин занят", MessageColor.Red);
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

                                    await client.SendServer("updateusers");
                                }
                                catch (Exception ex) { MessageBox.Show(ex.Message); }

                                loginTextBox.Text = string.Empty;
                                passwordTextBox.Password = string.Empty;
                                passwordAcceptTextBox.Password = string.Empty;
                                viewModel.ShowMessage("Аккаунт создан", MessageColor.Green);
                                loginGrid.Opacity = 1.0;
                                loginGrid.Effect = null;
                                viewModel.IsLoading = false;
                                TextBlock_MouseLeftButtonUp(messageTextBlock, new MouseButtonEventArgs(Mouse.PrimaryDevice, 1, MouseButton.Left));
                            }
                        }
                        else viewModel.ShowMessage("Пароли не совпадают", MessageColor.Red);
                    }
                    else viewModel.ShowMessage("Минимум 6 символов", MessageColor.Red);
                }
                else viewModel.ShowMessage("Минимум 6 символов", MessageColor.Red);
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

        void ApplyBlurEffect(UIElement target, int radius)
        {
            if (target == null) return;
            BlurEffect blurEffect = new BlurEffect();
            blurEffect.Radius = radius;
            target.Effect = blurEffect;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SwitchFrame();
        }
    }
}